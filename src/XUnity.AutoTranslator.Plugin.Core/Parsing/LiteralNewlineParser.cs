using System;
using System.Collections.Generic;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.Common.Extensions;

namespace XUnity.AutoTranslator.Plugin.Core.Parsing
{
    internal class LiteralNewlineParser
    {
        public ParserResult Parse( string input, int scope, IReadOnlyTextTranslationCache cache )
        {
            if( string.IsNullOrEmpty( input ) )
            {
                return null;
            }

            if( !ContainsLiteralSeparators( input ) )
            {
                return null;
            }

            var separators = new List<string>();
            var segments = SplitSegmentsPreservingDelimiters( input, separators );
            if( segments.Count <= 1 )
            {
                return null;
            }

            var template = new StringBuilder( input.Length + 8 );
            var args = new List<ArgumentedUntranslatedTextInfo>();
            char argument = 'A';
            bool anyArguments = false;

            for( int i = 0; i < segments.Count; i++ )
            {
                var segment = segments[ i ];
                if( segment.Length > 0 )
                {
                    if( segment.IsNullOrWhiteSpace() )
                    {
                        template.Append( segment );
                    }
                    else
                    {
                        var key = "[[" + ( argument++ ) + "]]";
                        args.Add( new ArgumentedUntranslatedTextInfo
                        {
                            Key = key,
                            Info = new UntranslatedTextInfo( segment )
                        } );
                        template.Append( key );
                        anyArguments = true;
                    }
                }

                if( i < separators.Count )
                {
                    template.Append( separators[ i ] );
                }
            }

            if( !anyArguments )
            {
                return null;
            }

            return new ParserResult( ParserResultOrigin.LiteralNewlineParser, input, template.ToString(), false, true, true, true, args );
        }

        private static bool ContainsLiteralSeparators( string text )
        {
            if( string.IsNullOrEmpty( text ) )
            {
                return false;
            }

            return text.IndexOf( "\\n", StringComparison.Ordinal ) >= 0;
        }

        private static List<string> SplitSegmentsPreservingDelimiters( string text, List<string> separators )
        {
            var segments = new List<string>();
            if( text == null )
            {
                segments.Add( string.Empty );
                return segments;
            }

            var builder = new StringBuilder( text.Length );
            var length = text.Length;

            for( int i = 0; i < length; )
            {
                var c = text[ i ];

                if( c == '\\' )
                {
                    if( i + 1 < length && text[ i + 1 ] == 'n' )
                    {
                        segments.Add( builder.ToString() );
                        builder.Length = 0;
                        separators.Add( "\\n" );
                        i += 2;
                        continue;
                    }
                }

                builder.Append( c );
                i++;
            }

            segments.Add( builder.ToString() );
            return segments;
        }
    }
}
