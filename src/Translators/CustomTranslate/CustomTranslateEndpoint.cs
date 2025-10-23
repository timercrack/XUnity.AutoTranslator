using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using SimpleJSON;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Endpoints.Http;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.AutoTranslator.Plugin.Core.Web;

namespace CustomTranslate
{
   internal class CustomTranslateEndpoint : HttpEndpoint
   {
      private const string JsonContentType = "application/json";
      private string _endpoint;
      private string _friendlyName;
      private bool _enableShortDelay;
      private bool _disableSpamChecks;
      private int _maxTranslationsPerRequest = 1;
      private int _maxConcurrency = 1;

      public CustomTranslateEndpoint()
      {
         _friendlyName = "Custom";
      }

      public override string Id => "CustomTranslate";

      public override string FriendlyName => _friendlyName;

      public override int MaxTranslationsPerRequest => _maxTranslationsPerRequest;

      public override int MaxConcurrency => _maxConcurrency;

      public override void Initialize( IInitializationContext context )
      {
         _endpoint = context.GetOrCreateSetting( "Custom", "Url", "" );
         _enableShortDelay = context.GetOrCreateSetting( "Custom", "EnableShortDelay", false );
         _disableSpamChecks = context.GetOrCreateSetting( "Custom", "DisableSpamChecks", false );
         _maxTranslationsPerRequest = Math.Max( 1, context.GetOrCreateSetting( "Custom", "BatchSize", 1 ) );
         _maxConcurrency = Math.Max( 1, context.GetOrCreateSetting( "Custom", "MaxConcurrency", 1 ) );

         if( string.IsNullOrEmpty( _endpoint ) ) throw new EndpointInitializationException( "The custom endpoint requires a url which has not been provided." );

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

         var payload = BuildBatchPayload( context.SourceLanguage, context.DestinationLanguage, untranslatedTexts );
         var request = new XUnityWebRequest( "POST", _endpoint, payload );
         request.Headers[ HttpRequestHeader.ContentType ] = JsonContentType + "; charset=utf-8";
         request.Headers[ HttpRequestHeader.Accept ] = JsonContentType;

         context.Complete( request );
      }

      public override void OnExtractTranslation( IHttpTranslationExtractionContext context )
      {
         var expectedCount = context.UntranslatedTexts != null ? context.UntranslatedTexts.Length : 1;
         var raw = context.Response.Data;

         var translations = TryParseBatchResponse( context, raw, expectedCount );

         if( translations == null )
         {
            context.Fail( "Batch operation received incorrect number of translations." );
            return;
         }

         if( expectedCount <= 1 )
         {
            context.Complete( translations[ 0 ] );
            return;
         }

         context.Complete( translations );
      }

      private static string BuildBatchPayload( string sourceLanguage, string destinationLanguage, string[] untranslatedTexts )
      {
         var builder = new StringBuilder();
         builder.Append( '{' );
         builder.Append( "\"from\":" );
         builder.Append( '"' );
         builder.Append( JsonHelper.Escape( sourceLanguage ?? string.Empty ) );
         builder.Append( '"' );
         builder.Append( ',' );
         builder.Append( "\"to\":" );
         builder.Append( '"' );
         builder.Append( JsonHelper.Escape( destinationLanguage ?? string.Empty ) );
         builder.Append( '"' );
         builder.Append( ',' );
         builder.Append( "\"texts\":[" );

         for( int i = 0; i < untranslatedTexts.Length; i++ )
         {
            if( i != 0 ) builder.Append( ',' );

            builder.Append( '"' );
            builder.Append( JsonHelper.Escape( untranslatedTexts[ i ] ?? string.Empty ) );
            builder.Append( '"' );
         }

         builder.Append( "]}" );

         return builder.ToString();
      }

      private static string[] TryParseBatchResponse( IHttpTranslationExtractionContext context, string data, int expectedCount )
      {
         if( string.IsNullOrEmpty( data ) ) return null;

         var parsed = ParseJsonTranslations( context, data, expectedCount );
         if( parsed != null )
         {
            return parsed;
         }

         return ParseLineSeparatedTranslations( data, expectedCount );
      }

      private static string[] ParseJsonTranslations( IHttpTranslationExtractionContext context, string data, int expectedCount )
      {
         try
         {
            var trimmed = StripCodeFences( data );
            var looksLikeJson = LooksLikeJson( context, trimmed );

            if( !looksLikeJson ) return null;

            var node = JSON.Parse( trimmed );
            if( node == null ) return null;

            JSONArray array = node as JSONArray;
            if( array == null )
            {
               var translationsNode = node[ "translations" ];
               array = translationsNode as JSONArray;
            }

            if( array == null ) return null;
            if( array.Count != expectedCount ) return null;

            var results = new string[ expectedCount ];
            for( int i = 0; i < expectedCount; i++ )
            {
               var entry = array[ i ];
               results[ i ] = entry != null ? entry.Value : string.Empty;
            }

            return results;
         }
         catch( Exception )
         {
            return null;
         }
      }

      private static string[] ParseLineSeparatedTranslations( string data, int expectedCount )
      {
         var lines = data.Replace( "\r", string.Empty ).Split( new[] { '\n' }, StringSplitOptions.None );

         if( lines.Length != expectedCount ) return null;

         for( int i = 0; i < lines.Length; i++ )
         {
            lines[ i ] = lines[ i ] ?? string.Empty;
         }

         return lines;
      }

      private static bool LooksLikeJson( IHttpTranslationExtractionContext context, string data )
      {
         if( string.IsNullOrEmpty( data ) ) return false;

         var headers = context.Response != null ? context.Response.Headers : null;
         if( headers != null )
         {
            var contentType = headers[ "Content-Type" ];
            if( !string.IsNullOrEmpty( contentType ) && contentType.IndexOf( "json", StringComparison.OrdinalIgnoreCase ) >= 0 )
            {
               return true;
            }
         }

         var trimmed = data.TrimStart();
         return trimmed.StartsWith( "{" ) || trimmed.StartsWith( "[" );
      }

      private static string StripCodeFences( string data )
      {
         if( string.IsNullOrEmpty( data ) ) return data;

         var trimmed = data.Trim();
         if( trimmed.StartsWith( "```" ) )
         {
            var newlineIndex = trimmed.IndexOf( '\n' );
            if( newlineIndex >= 0 )
            {
               trimmed = trimmed.Substring( newlineIndex + 1 );
            }

            if( trimmed.EndsWith( "```", StringComparison.Ordinal ) )
            {
               trimmed = trimmed.Substring( 0, trimmed.Length - 3 );
            }

            return trimmed.Trim();
         }

         return data;
      }
   }
}
