using System;
using System.Collections.Generic;
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
      private const string DefaultEndpointUrl = "https://api.deepseek.com/chat/completions";
      private const string DefaultModel = "deepseek-v4-flash";
      private const string LegacyChineseOnlySystemPrompt = "Translate the user JSON array of game UI text into Simplified Chinese. Return JSON array only.";
      private const string DefaultSystemPromptTemplate = "You are a dedicated translator for game UI text. The input is a JSON array of strings. Translate each array element into {DestinationLanguageName}. Return exactly one valid JSON array with the same number of elements and the same order as the input. Output only the JSON array. Do not output markdown, code fences, comments, explanations, or any extra text. Preserve escape sequences such as \\n, \\r, and \\t exactly as they appear. Preserve placeholders and markup such as <...> exactly. Keep personal names translated or transliterated consistently into {DestinationLanguageName}, unless the text is a hotkey, single-letter label, ID, serial number, or obvious technical identifier.";
      private const string DefaultSystemPromptZh = @"你是Ostranauts（星际漂流者）的专用翻译器。输入是一个JSON数组，数组中的每个元素都是一个待翻译字符串。你的任务是返回一个JSON数组，元素数量、顺序必须与输入完全一致，每个元素都是对应的简体中文译文字符串。你必须严格遵守以下规则：1. 只能输出一个合法的JSON数组。2. 输出内容的第一个字符必须是[，最后一个字符必须是]。3. 除这个JSON数组外，禁止输出任何其他内容。4. 禁止输出Markdown。5. 禁止输出代码块。6. 禁止输出任何围栏标记或语言标识，例如```或```json。7. 禁止输出说明、注释、前缀、后缀、标题、提示语、空行、自然语言解释。8. 数组中的每个元素都必须是合法JSON字符串；若译文中出现双引号，必须按JSON规则转义。9. 必须原样保留每个字符串中的转义符及其位置，包括\n、\r、\t，不得新增、删除、移动、合并或拆分。10. <...>中的内容视为特殊标记，必须逐字保留，不翻译、不改写。11. 默认把人物姓名音译/翻译为简体中文，并且同一名字在不同条目里必须保持一致；像 Camila Oluwakemi Graves 这类人名不要原样保留英文。只有单个字母、快捷键、明显代号、注册号、纯ID或明确不应翻译的技术标识保留原文。12. 不要合并、拆分、补全、重排或省略任何数组元素。13. 如果某一项无法确定含义，可以保留原文，但仍然只能输出合法JSON数组。唯一允许的输出形式是：[""译文1"",""译文2""]。";
      private const string DefaultSystemPromptRu = @"你是Ostranauts（星际漂流者）的专用翻译器。输入是一个JSON数组，数组中的每个元素都是一个待翻译字符串。你的任务是返回一个JSON数组，元素数量、顺序必须与输入完全一致，每个元素都是对应的俄文译文字符串。你必须严格遵守以下规则：1. 只能输出一个合法的JSON数组。2. 输出内容的第一个字符必须是[，最后一个字符必须是]。3. 除这个JSON数组外，禁止输出任何其他内容。4. 禁止输出Markdown。5. 禁止输出代码块。6. 禁止输出任何围栏标记或语言标识，例如```或```json。7. 禁止输出说明、注释、前缀、后缀、标题、提示语、空行、自然语言解释。8. 数组中的每个元素都必须是合法JSON字符串；若译文中出现双引号，必须按JSON规则转义。9. 必须原样保留每个字符串中的转义符及其位置，包括\n、\r、\t，不得新增、删除、移动、合并或拆分。10. <...>中的内容视为特殊标记，必须逐字保留，不翻译、不改写。11. 默认把人物姓名音译/翻译为俄文，并且同一名字在不同条目里必须保持一致；像 Camila Oluwakemi Graves 这类人名不要原样保留英文。只有单个字母、快捷键、明显代号、注册号、纯ID或明确不应翻译的技术标识保留原文。12. 不要合并、拆分、补全、重排或省略任何数组元素。13. 如果某一项无法确定含义，可以保留原文，但仍然只能输出合法JSON数组。唯一允许的输出形式是：[""перевод1"",""перевод2""]。";
      private static readonly Dictionary<string, string> LanguageDisplayNames = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase )
      {
         { "auto", "the detected source language" },
         { "en", "English" },
         { "ja", "Japanese" },
         { "ko", "Korean" },
         { "romaji", "Romaji" },
         { "ru", "Russian" },
         { "zh", "Simplified Chinese" },
         { "zh-CN", "Simplified Chinese" },
         { "zh-Hans", "Simplified Chinese" },
         { "zh-TW", "Traditional Chinese" },
         { "zh-Hant", "Traditional Chinese" },
      };
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
         _endpoint = context.GetOrCreateSetting( "LLMTranslate", "Url", DefaultEndpointUrl );
         _apiKey = context.GetOrCreateSetting( "LLMTranslate", "ApiKey", string.Empty );
         _model = context.GetOrCreateSetting( "LLMTranslate", "Model", DefaultModel );
         context.GetOrCreateSetting( "LLMTranslate", "SystemPrompt_zh", DefaultSystemPromptZh );
         context.GetOrCreateSetting( "LLMTranslate", "SystemPrompt_ru", DefaultSystemPromptRu );
         var configuredSystemPrompt = context.GetOrCreateSetting( "LLMTranslate", "SystemPrompt", DefaultSystemPromptTemplate );
         _systemPrompt = ResolveSystemPrompt( context, configuredSystemPrompt );
         _temperature = context.GetOrCreateSetting( "LLMTranslate", "Temperature", 0.2f );
         _maxTokens = Math.Max( 1, context.GetOrCreateSetting( "LLMTranslate", "MaxTokens", 8192 ) );
         _maxTranslationsPerRequest = Math.Max( 1, context.GetOrCreateSetting( "LLMTranslate", "BatchSize", 100 ) );
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

      private static string ResolveSystemPrompt( IInitializationContext context, string configuredPrompt )
      {
         var languageSpecificPrompt = GetLanguageSpecificSystemPrompt( context );
         var template = !IsNullOrWhiteSpace( languageSpecificPrompt ) ? languageSpecificPrompt : configuredPrompt;

         if( IsNullOrWhiteSpace( template ) )
         {
            template = DefaultSystemPromptTemplate;
         }
         else if( string.Equals( template, LegacyChineseOnlySystemPrompt, StringComparison.Ordinal ) )
         {
            XuaLogger.AutoTranslator.Info( "[LLMTranslate] Detected the legacy Chinese-only SystemPrompt. Using the language-aware default prompt instead." );
            template = DefaultSystemPromptTemplate;
         }

         return ApplyPromptPlaceholders( template, context.SourceLanguage, context.DestinationLanguage );
      }

      private static string GetLanguageSpecificSystemPrompt( IInitializationContext context )
      {
         var normalizedDestinationLanguage = NormalizeLanguageCode( context.DestinationLanguage );
         if( string.IsNullOrEmpty( normalizedDestinationLanguage ) ) return string.Empty;

         var exactKey = "SystemPrompt_" + normalizedDestinationLanguage.Replace( '-', '_' );
         var prompt = context.GetOrCreateSetting( "LLMTranslate", exactKey, string.Empty );
         if( !IsNullOrWhiteSpace( prompt ) ) return prompt;

         var separatorIndex = normalizedDestinationLanguage.IndexOf( '-' );
         if( separatorIndex > 0 )
         {
            var neutralLanguage = normalizedDestinationLanguage.Substring( 0, separatorIndex );
            if( neutralLanguage.Length > 0 )
            {
               var neutralKey = "SystemPrompt_" + neutralLanguage;
               if( !string.Equals( neutralKey, exactKey, StringComparison.OrdinalIgnoreCase ) )
               {
                  prompt = context.GetOrCreateSetting( "LLMTranslate", neutralKey, string.Empty );
                  if( !IsNullOrWhiteSpace( prompt ) ) return prompt;
               }
            }
         }

         return string.Empty;
      }

      private static string GetDefaultSystemPromptForLanguage( string normalizedLanguage )
      {
         if( string.IsNullOrEmpty( normalizedLanguage ) ) return string.Empty;

         if( string.Equals( normalizedLanguage, "zh", StringComparison.OrdinalIgnoreCase )
            || string.Equals( normalizedLanguage, "zh-CN", StringComparison.OrdinalIgnoreCase )
            || string.Equals( normalizedLanguage, "zh-Hans", StringComparison.OrdinalIgnoreCase ) )
         {
            return DefaultSystemPromptZh;
         }

         if( string.Equals( normalizedLanguage, "ru", StringComparison.OrdinalIgnoreCase ) )
         {
            return DefaultSystemPromptRu;
         }

         return string.Empty;
      }

      private static string ApplyPromptPlaceholders( string template, string sourceLanguage, string destinationLanguage )
      {
         var normalizedSourceLanguage = NormalizeLanguageCode( sourceLanguage );
         var normalizedDestinationLanguage = NormalizeLanguageCode( destinationLanguage );
         var sourceLanguageName = GetLanguageDisplayName( normalizedSourceLanguage, "the detected source language" );
         var destinationLanguageName = GetLanguageDisplayName( normalizedDestinationLanguage, "the requested target language" );

         return ( template ?? string.Empty )
            .Replace( "{SourceLanguageCode}", normalizedSourceLanguage ?? string.Empty )
            .Replace( "{FromLanguageCode}", normalizedSourceLanguage ?? string.Empty )
            .Replace( "{DestinationLanguageCode}", normalizedDestinationLanguage ?? string.Empty )
            .Replace( "{ToLanguageCode}", normalizedDestinationLanguage ?? string.Empty )
            .Replace( "{SourceLanguageName}", sourceLanguageName )
            .Replace( "{FromLanguageName}", sourceLanguageName )
            .Replace( "{DestinationLanguageName}", destinationLanguageName )
            .Replace( "{ToLanguageName}", destinationLanguageName );
      }

      private static string GetLanguageDisplayName( string languageCode, string fallback )
      {
         if( string.IsNullOrEmpty( languageCode ) ) return fallback;

         string displayName;
         if( LanguageDisplayNames.TryGetValue( languageCode, out displayName ) )
         {
            return displayName;
         }

         try
         {
            var cultureCode = string.Equals( languageCode, "zh", StringComparison.OrdinalIgnoreCase )
               ? "zh-CN"
               : languageCode;
            var culture = CultureInfo.GetCultureInfo( cultureCode );
            if( culture != null && !string.IsNullOrEmpty( culture.EnglishName ) )
            {
               return culture.EnglishName;
            }
         }
         catch( Exception )
         {
         }

         var separatorIndex = languageCode.IndexOf( '-' );
         if( separatorIndex > 0 )
         {
            return GetLanguageDisplayName( languageCode.Substring( 0, separatorIndex ), fallback );
         }

         return languageCode;
      }

      private static string NormalizeLanguageCode( string languageCode )
      {
         if( string.IsNullOrEmpty( languageCode ) ) return string.Empty;
         return languageCode.Trim().Replace( '_', '-' );
      }

      private static bool IsNullOrWhiteSpace( string text )
      {
         return string.IsNullOrEmpty( text ) || text.Trim().Length == 0;
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
