using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Extensions;

namespace XUnity.AutoTranslator.Plugin.Core.Parsing
{
   internal class GameLogTextParser
   {
      private static readonly Regex RepeatedLogCounterRegex = new Regex( @"\s*[\(（][xX]\d+[\)）]\s*$", RegexOptions.Compiled );
      private static readonly Regex AngleBracketWrappedLineRegex = new Regex( @"^(?<leading>\s*(?:<[^>]+>)*)<(?<padding>\s*)(?<text>[^<>\r\n]+?)(?<closingPadding>\s*)>(?<trailing>(?:</[^>]+>\s*)*)$", RegexOptions.Compiled );
      private static readonly Regex AngleBracketPrefixedLineRegex = new Regex( @"^(?<leading>\s*(?:<[^>]+>)*)<(?<padding>\s*)(?<text>[^<>\r\n]+?)(?<trailing>(?:</[^>]+>\s*)*)$", RegexOptions.Compiled );
      private static readonly HashSet<string> RichTextTagNames = new HashSet<string>( StringComparer.OrdinalIgnoreCase )
      {
         "a",
         "align",
         "allcaps",
         "alpha",
         "b",
         "br",
         "color",
         "cspace",
         "font",
         "font-weight",
         "gradient",
         "i",
         "indent",
         "line-height",
         "line-indent",
         "link",
         "lowercase",
         "margin",
         "mark",
         "mspace",
         "nobr",
         "noparse",
         "page",
         "pos",
         "rotate",
         "size",
         "smallcaps",
         "space",
         "sprite",
         "s",
         "style",
         "sub",
         "sup",
         "u",
         "uppercase",
         "voffset",
         "width"
      };

      public GameLogTextParser()
      {
      }

      public bool CanApply( object ui )
      {
         return ui.SupportsLineParser();
      }

      public ParserResult Parse( string input, int scope, IReadOnlyTextTranslationCache cache )
      {
         var reader = new StringReader( input );
         bool containsTranslatable = false;
         bool containsResolvedTranslations = false;
         bool containsRepeatedLogCounters = false;
         var template = new StringBuilder( input.Length );
         var args = new List<ArgumentedUntranslatedTextInfo>();
         var arg = 'A';

         string line = null;
         while( ( line = reader.ReadLine() ) != null )
         {
            if( !string.IsNullOrEmpty( line ) )
            {
               if( IsEmptyRichTextLine( line ) )
               {
                  template.Append( line ).Append( '\n' );
               }
               else if( TryGetRepeatedLogCounterLine( line, out var baseLine, out var counterSuffix ) )
               {
                  containsRepeatedLogCounters = true;

                  if( TryGetExistingTranslation( cache, baseLine, scope, out var existingTranslation ) )
                  {
                     containsResolvedTranslations = true;

                     template.Append( existingTranslation ).Append( counterSuffix ).Append( '\n' );
                  }
                  else if( cache.IsTranslatable( baseLine, true, scope ) )
                  {
                     containsTranslatable = true;

                     var key = "[[" + ( arg++ ) + "]]";
                     template.Append( key ).Append( counterSuffix ).Append( '\n' );
                     args.Add( new ArgumentedUntranslatedTextInfo
                     {
                        Key = key,
                        Info = new RepeatedLogCounterUntranslatedTextInfo( baseLine )
                     } );
                  }
                  else
                  {
                     template.Append( line ).Append( '\n' );
                  }
               }
               else if( TryGetAngleBracketWrappedLine( line, out var wrappedTemplatePrefix, out var wrappedBaseLine, out var wrappedTemplateSuffix ) )
               {
                  if( TryGetExistingTranslation( cache, wrappedBaseLine, scope, out var existingTranslation ) )
                  {
                     containsResolvedTranslations = true;

                     template.Append( wrappedTemplatePrefix ).Append( existingTranslation ).Append( wrappedTemplateSuffix ).Append( '\n' );
                  }
                  else if( cache.IsTranslatable( wrappedBaseLine, true, scope ) )
                  {
                     containsTranslatable = true;

                     var key = "[[" + ( arg++ ) + "]]";
                     template.Append( wrappedTemplatePrefix ).Append( key ).Append( wrappedTemplateSuffix ).Append( '\n' );
                     args.Add( new ArgumentedUntranslatedTextInfo
                     {
                        Key = key,
                        Info = new AngleBracketWrappedUntranslatedTextInfo( wrappedBaseLine )
                     } );
                  }
                  else
                  {
                     template.Append( line ).Append( '\n' );
                  }
               }
               else if( TryGetAngleBracketPrefixedLine( line, out var angleTemplatePrefix, out var angleBaseLine, out var angleTemplateSuffix ) )
               {
                  if( TryGetExistingTranslation( cache, angleBaseLine, scope, out var existingTranslation ) )
                  {
                     containsResolvedTranslations = true;

                     template.Append( angleTemplatePrefix ).Append( existingTranslation ).Append( angleTemplateSuffix ).Append( '\n' );
                  }
                  else if( cache.IsTranslatable( angleBaseLine, true, scope ) )
                  {
                     containsTranslatable = true;

                     var key = "[[" + ( arg++ ) + "]]";
                     template.Append( angleTemplatePrefix ).Append( key ).Append( angleTemplateSuffix ).Append( '\n' );
                     args.Add( new ArgumentedUntranslatedTextInfo
                     {
                        Key = key,
                        Info = new AngleBracketPrefixedUntranslatedTextInfo( angleBaseLine )
                     } );
                  }
                  else
                  {
                     template.Append( line ).Append( '\n' );
                  }
               }
               else if( TryGetExistingTranslation( cache, line, scope, out var existingTranslation ) )
               {
                  containsResolvedTranslations = true;

                  template.Append( existingTranslation ).Append( '\n' );
               }
               else if( cache.IsTranslatable( line, true, scope ) )
               {
                  // template it!
                  containsTranslatable = true;

                  var key = "[[" + ( arg++ ) + "]]";
                  template.Append( key ).Append( '\n' );
                  args.Add( new ArgumentedUntranslatedTextInfo
                  {
                     Key = key,
                     Info = new GameLogLineUntranslatedTextInfo( line )
                  } );
               }
               else
               {
                  // add it
                  //containsTranslated = true;
                  template.Append( line ).Append( '\n' );
               }
            }
            else
            {
               // add new line
               template.Append( '\n' );
            }
         }

         if( !containsTranslatable && !containsResolvedTranslations && !containsRepeatedLogCounters ) return null;

         if( template.Length > 0 && !input.EndsWith( "\r\n" ) && !input.EndsWith( "\n" ) ) template.Remove( template.Length - 1, 1 );

         if( args.Count > 0 || containsResolvedTranslations || containsRepeatedLogCounters )
         {
            return new ParserResult( ParserResultOrigin.GameLogTextParser, input, template.ToString(), false, false, false, true, args );
         }

         return null;
      }

      private static bool TryGetRepeatedLogCounterLine( string line, out string baseLine, out string counterSuffix )
      {
         baseLine = null;
         counterSuffix = null;

         if( string.IsNullOrEmpty( line ) ) return false;

         var match = RepeatedLogCounterRegex.Match( line );
         if( !match.Success ) return false;

         for( int i = 0; i < match.Index; i++ )
         {
            if( !char.IsWhiteSpace( line[ i ] ) )
            {
               baseLine = line.Substring( 0, match.Index );
               counterSuffix = line.Substring( match.Index );
               return true;
            }
         }

         return false;
      }

      private static bool TryGetAngleBracketWrappedLine( string line, out string templatePrefix, out string baseLine, out string templateSuffix )
      {
         templatePrefix = null;
         baseLine = null;
         templateSuffix = null;

         if( string.IsNullOrEmpty( line ) ) return false;

         var match = AngleBracketWrappedLineRegex.Match( line );
         if( !match.Success ) return false;

         baseLine = match.Groups[ "text" ].Value;
         if( string.IsNullOrEmpty( baseLine ) || baseLine.Trim().Length == 0 )
         {
            baseLine = null;
            return false;
         }

         if( LooksLikeRichTextTag( baseLine, 0, baseLine.Length - 1 ) )
         {
            baseLine = null;
            return false;
         }

         templatePrefix = match.Groups[ "leading" ].Value + "<" + match.Groups[ "padding" ].Value;
         templateSuffix = match.Groups[ "closingPadding" ].Value + ">" + match.Groups[ "trailing" ].Value;
         return true;
      }

      private static bool TryGetAngleBracketPrefixedLine( string line, out string templatePrefix, out string baseLine, out string templateSuffix )
      {
         templatePrefix = null;
         baseLine = null;
         templateSuffix = null;

         if( string.IsNullOrEmpty( line ) ) return false;

         var match = AngleBracketPrefixedLineRegex.Match( line );
         if( !match.Success ) return false;

         baseLine = match.Groups[ "text" ].Value;
         if( string.IsNullOrEmpty( baseLine ) || baseLine.Trim().Length == 0 )
         {
            baseLine = null;
            return false;
         }

         templatePrefix = match.Groups[ "leading" ].Value + "<" + match.Groups[ "padding" ].Value;
         templateSuffix = match.Groups[ "trailing" ].Value;
         return true;
      }

      private static bool IsEmptyRichTextLine( string line )
      {
         if( string.IsNullOrEmpty( line ) ) return false;

         var stripped = StripKnownRichTextTags( line );
         if( stripped.Length == line.Length ) return false;

         return stripped.Trim().Length == 0;
      }

      private static string StripKnownRichTextTags( string text )
      {
         if( string.IsNullOrEmpty( text ) ) return text;

         var builder = new StringBuilder( text.Length );
         bool changed = false;

         for( int i = 0; i < text.Length; i++ )
         {
            if( text[ i ] == '<' && TryGetRichTextTagEnd( text, i, out var closingIndex ) )
            {
               changed = true;
               i = closingIndex;
               continue;
            }

            builder.Append( text[ i ] );
         }

         return changed ? builder.ToString() : text;
      }

      private static bool TryGetRichTextTagEnd( string text, int startIndex, out int closingIndex )
      {
         closingIndex = -1;

         var endIndex = text.IndexOf( '>', startIndex + 1 );
         if( endIndex < 0 ) return false;

         for( int i = startIndex + 1; i < endIndex; i++ )
         {
            if( text[ i ] == '<' )
            {
               return false;
            }
         }

         if( !LooksLikeRichTextTag( text, startIndex + 1, endIndex - 1 ) ) return false;

         closingIndex = endIndex;
         return true;
      }

      private static bool LooksLikeRichTextTag( string text, int startIndex, int endIndex )
      {
         while( startIndex <= endIndex && char.IsWhiteSpace( text[ startIndex ] ) ) startIndex++;
         while( endIndex >= startIndex && char.IsWhiteSpace( text[ endIndex ] ) ) endIndex--;

         if( startIndex > endIndex ) return false;

         if( text[ startIndex ] == '/' )
         {
            startIndex++;
            while( startIndex <= endIndex && char.IsWhiteSpace( text[ startIndex ] ) ) startIndex++;
            if( startIndex > endIndex ) return false;
         }

         int nameEndIndex = startIndex;
         while( nameEndIndex <= endIndex )
         {
            var c = text[ nameEndIndex ];
            if( char.IsLetterOrDigit( c ) || c == '-' )
            {
               nameEndIndex++;
               continue;
            }

            break;
         }

         if( nameEndIndex == startIndex ) return false;

         var tagName = text.Substring( startIndex, nameEndIndex - startIndex );
         return RichTextTagNames.Contains( tagName );
      }

      private static bool TryGetExistingTranslation( IReadOnlyTextTranslationCache cache, string text, int scope, out string translation )
      {
         translation = null;

         if( string.IsNullOrEmpty( text ) ) return false;

         var textKey = new UntranslatedText( text, false, false, Settings.FromLanguageUsesWhitespaceBetweenWords, true, Settings.TemplateAllNumberAway );
         if( !cache.TryGetTranslation( textKey, true, true, scope, out translation ) ) return false;

         translation = textKey.Untemplate( translation ) ?? string.Empty;
         return true;
      }
   }

   internal abstract class GameLogSpecializedUntranslatedTextInfo : UntranslatedTextInfo
   {
      protected GameLogSpecializedUntranslatedTextInfo( string untranslatedText ) : base( untranslatedText )
      {
      }
   }

   internal class RepeatedLogCounterUntranslatedTextInfo : GameLogSpecializedUntranslatedTextInfo
   {
      public RepeatedLogCounterUntranslatedTextInfo( string untranslatedText ) : base( untranslatedText )
      {
      }
   }

   internal class AngleBracketPrefixedUntranslatedTextInfo : GameLogSpecializedUntranslatedTextInfo
   {
      public AngleBracketPrefixedUntranslatedTextInfo( string untranslatedText ) : base( untranslatedText )
      {
      }
   }

   internal class AngleBracketWrappedUntranslatedTextInfo : GameLogSpecializedUntranslatedTextInfo
   {
      public AngleBracketWrappedUntranslatedTextInfo( string untranslatedText ) : base( untranslatedText )
      {
      }
   }

   internal class GameLogLineUntranslatedTextInfo : GameLogSpecializedUntranslatedTextInfo
   {
      public GameLogLineUntranslatedTextInfo( string untranslatedText ) : base( untranslatedText )
      {
      }
   }
}
