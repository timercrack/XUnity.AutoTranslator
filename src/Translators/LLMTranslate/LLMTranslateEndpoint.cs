using System;
using System.Globalization;
using System.Net;
using System.Text;
using SimpleJSON;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Endpoints.Http;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.AutoTranslator.Plugin.Core.Web;
using XUnity.Common.Logging;

namespace LLMTranslate
{
   internal class LLMTranslateEndpoint : HttpEndpoint
   {
      private const string JsonContentType = "application/json";
      private string _endpoint;
      private string _friendlyName;
      private string _apiKey;
      private string _model;
      private string _systemPrompt;
      private float _temperature;
      private int _maxTokens;
      private int _maxTranslationsPerRequest = 1;
      private int _maxConcurrency = 1;
      private bool _enableShortDelay;
      private bool _disableSpamChecks;

      public LLMTranslateEndpoint()
      {
         _friendlyName = "LLM Translate";
      }

      public override string Id => "LLMTranslate";

      public override string FriendlyName => _friendlyName;

      public override int MaxTranslationsPerRequest => _maxTranslationsPerRequest;

      public override int MaxConcurrency => _maxConcurrency;

      public override void Initialize( IInitializationContext context )
      {
         _endpoint = context.GetOrCreateSetting( "LLMTranslate", "Url", string.Empty );
         _apiKey = context.GetOrCreateSetting( "LLMTranslate", "ApiKey", string.Empty );
         _model = context.GetOrCreateSetting( "LLMTranslate", "Model", string.Empty );
         _systemPrompt = context.GetOrCreateSetting( "LLMTranslate", "SystemPrompt", string.Empty );
         _temperature = context.GetOrCreateSetting( "LLMTranslate", "Temperature", 0.2f );
         _maxTokens = Math.Max( 1, context.GetOrCreateSetting( "LLMTranslate", "MaxTokens", 8192 ) );
         _maxTranslationsPerRequest = Math.Max( 1, context.GetOrCreateSetting( "LLMTranslate", "BatchSize", 10 ) );
         _maxConcurrency = Math.Max( 1, context.GetOrCreateSetting( "LLMTranslate", "MaxConcurrency", 1 ) );
         _enableShortDelay = context.GetOrCreateSetting( "LLMTranslate", "EnableShortDelay", false );
         _disableSpamChecks = context.GetOrCreateSetting( "LLMTranslate", "DisableSpamChecks", false );

         if( string.IsNullOrEmpty( _endpoint ) ) throw new EndpointInitializationException( "The LLMTranslate endpoint requires a url which has not been provided." );
         if( string.IsNullOrEmpty( _model ) ) throw new EndpointInitializationException( "The LLMTranslate endpoint requires a model which has not been provided." );

         var uri = new Uri( _endpoint );
         context.DisableCertificateChecksFor( uri.Host );

         _friendlyName += " (" + uri.Host + ")";

         if( _enableShortDelay ) context.SetTranslationDelay( 0.1f );
         if( _disableSpamChecks ) context.DisableSpamChecks();
      }

      public override void OnCreateRequest( IHttpRequestCreationContext context )
      {
         var untranslatedTexts = context.UntranslatedTexts;
         if( untranslatedTexts == null || untranslatedTexts.Length == 0 )
         {
            untranslatedTexts = new[] { context.UntranslatedText };
         }

         var textArray = BuildJsonStringArray( untranslatedTexts );
         LogOutgoingTextJson( textArray );

         var payload = BuildRequestPayload( textArray );
         var request = new XUnityWebRequest( "POST", _endpoint, payload );
         if( !string.IsNullOrEmpty( _apiKey ) )
         {
            request.Headers[ HttpRequestHeader.Authorization ] = "Bearer " + _apiKey;
         }

         request.Headers[ HttpRequestHeader.ContentType ] = JsonContentType + "; charset=utf-8";
         request.Headers[ HttpRequestHeader.Accept ] = JsonContentType;

         context.Complete( request );
      }

      public override void OnExtractTranslation( IHttpTranslationExtractionContext context )
      {
         var expectedCount = context.UntranslatedTexts != null ? context.UntranslatedTexts.Length : 1;
         var raw = context.Response.Data;

         var translations = TryParseBatchResponse( raw, expectedCount );
         if( translations == null )
         {
            LogFailedResponse( raw, expectedCount );
            context.Fail( "The LLMTranslate endpoint returned an invalid response. Expected a JSON array (or object with a translations array) matching the request batch size." );
            return;
         }

         if( expectedCount <= 1 )
         {
            context.Complete( translations[ 0 ] );
            return;
         }

         context.Complete( translations );
      }

      private string BuildRequestPayload( string textArray )
      {
         var builder = new StringBuilder();
         builder.Append( '{' );
         builder.Append( "\"model\":\"" );
         builder.Append( JsonHelper.Escape( _model ?? string.Empty ) );
         builder.Append( "\"," );
         builder.Append( "\"messages\":[" );
         AppendMessage( builder, "system", _systemPrompt ?? string.Empty );
         builder.Append( ',' );
         AppendMessage( builder, "user", textArray );
         builder.Append( "]," );
         builder.Append( "\"temperature\":" );
         builder.Append( _temperature.ToString( CultureInfo.InvariantCulture ) );
         builder.Append( ',' );
         builder.Append( "\"max_tokens\":" );
         builder.Append( _maxTokens.ToString( CultureInfo.InvariantCulture ) );
         builder.Append( ',' );
         builder.Append( "\"thinking\":{\"type\":\"disabled\"}" );
         builder.Append( '}' );
         return builder.ToString();
      }

      private static void AppendMessage( StringBuilder builder, string role, string content )
      {
         builder.Append( '{' );
         builder.Append( "\"role\":\"" );
         builder.Append( JsonHelper.Escape( role ?? string.Empty ) );
         builder.Append( "\"," );
         builder.Append( "\"content\":\"" );
         builder.Append( JsonHelper.Escape( content ?? string.Empty ) );
         builder.Append( "\"}" );
      }

      private static string BuildJsonStringArray( string[] untranslatedTexts )
      {
         var builder = new StringBuilder();
         builder.Append( '[' );
         for( int i = 0; i < untranslatedTexts.Length; i++ )
         {
            if( i != 0 ) builder.Append( ',' );
            builder.Append( '"' );
            builder.Append( JsonHelper.Escape( untranslatedTexts[ i ] ?? string.Empty ) );
            builder.Append( '"' );
         }
         builder.Append( ']' );
         return builder.ToString();
      }

      private static string[] TryParseBatchResponse( string data, int expectedCount )
      {
         if( string.IsNullOrEmpty( data ) ) return null;

         try
         {
            var response = JSON.Parse( data ) as JSONObject;
            if( response == null ) return null;

            var choices = response[ "choices" ] as JSONArray;
            if( choices == null || choices.Count == 0 ) return null;

            var choice = choices[ 0 ] as JSONObject;
            if( choice == null ) return null;

            var message = choice[ "message" ] as JSONObject;
            if( message == null ) return null;

            var contentNode = message[ "content" ];
            var rawContent = contentNode != null ? contentNode.Value : null;
            if( string.IsNullOrEmpty( rawContent ) ) return null;

            return ParseJsonTranslations( rawContent, expectedCount );
         }
         catch( Exception )
         {
            return null;
         }
      }

      private static string[] ParseJsonTranslations( string data, int expectedCount )
      {
         try
         {
            var node = JSON.Parse( data );
            if( node == null ) return null;

            JSONArray array = node as JSONArray;
            if( array == null )
            {
               var obj = node as JSONObject;
               if( obj != null )
               {
                  array = obj[ "translations" ] as JSONArray;
               }
            }

            if( array == null ) return null;
            if( array.Count != expectedCount ) return null;

            var results = new string[ expectedCount ];
            for( int i = 0; i < expectedCount; i++ )
            {
               results[ i ] = ConvertNodeToString( array[ i ] );
            }

            return results;
         }
         catch( Exception )
         {
            return null;
         }
      }

      private static string ConvertNodeToString( JSONNode node )
      {
         if( node == null || node.IsNull ) return string.Empty;
         if( node.IsString ) return node.Value;
         return node.ToString();
      }

      private static void LogFailedResponse( string rawResponse, int expectedCount )
      {
         XuaLogger.AutoTranslator.Error( $"[LLMTranslate] Failed to parse translation response. Expected {expectedCount} translated entries." );

         if( string.IsNullOrEmpty( rawResponse ) )
         {
            XuaLogger.AutoTranslator.Error( "[LLMTranslate] Raw response body was empty." );
            return;
         }

         XuaLogger.AutoTranslator.Error( "[LLMTranslate] Raw response body:" );
         XuaLogger.AutoTranslator.Error( rawResponse );

         var assistantContent = TryExtractAssistantContent( rawResponse );
         if( !string.IsNullOrEmpty( assistantContent ) && !string.Equals( assistantContent, rawResponse, StringComparison.Ordinal ) )
         {
            XuaLogger.AutoTranslator.Error( "[LLMTranslate] Raw assistant content:" );
            XuaLogger.AutoTranslator.Error( assistantContent );
         }
      }

      private static void LogOutgoingTextJson( string textArray )
      {
         XuaLogger.AutoTranslator.Info( textArray ?? "[]" );
      }

      private static string TryExtractAssistantContent( string rawResponse )
      {
         if( string.IsNullOrEmpty( rawResponse ) ) return null;

         try
         {
            var response = JSON.Parse( rawResponse ) as JSONObject;
            if( response == null ) return null;

            var choices = response[ "choices" ] as JSONArray;
            if( choices == null || choices.Count == 0 ) return null;

            var choice = choices[ 0 ] as JSONObject;
            if( choice == null ) return null;

            var message = choice[ "message" ] as JSONObject;
            if( message == null ) return null;

            var contentNode = message[ "content" ];
            return contentNode != null ? contentNode.Value : null;
         }
         catch( Exception )
         {
            return null;
         }
      }
   }
}
