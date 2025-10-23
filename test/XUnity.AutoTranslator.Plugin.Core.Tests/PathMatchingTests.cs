using System.Collections.Generic;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using Xunit;

namespace XUnity.AutoTranslator.Plugin.Core.Tests
{
    public class PathMatchingTests
    {
        [Theory]
        [InlineData( "/Root/Child", "/Root/Child", true )]
        [InlineData( "/Root/Child/Grand", "/Root/Child", true )]
        [InlineData( "/Root/Child/Grand", "/Root/Child/", true )]
        [InlineData( "/Root/Child/Grand", "   /Root/Child   ", true )]
        [InlineData( "/Root/Childish", "/Root/Child", false )]
        [InlineData( "/Root/Child", "Root/Child", false )]
        [InlineData( "/Root/Child", "", false )]
        [InlineData( "/Root/Child", null, false )]
        public void MatchesPath_HandlesCandidateVariants( string path, string candidate, bool expected )
        {
            var candidates = new HashSet<string>();
            if( candidate != null )
            {
                candidates.Add( candidate );
            }

            var actual = PathMatching.MatchesPath( path, candidates );

            Assert.Equal( expected, actual );
        }

        [Fact]
        public void MatchesPath_ReturnsTrue_WhenAnyCandidateMatches()
        {
            var candidates = new HashSet<string>
         {
            "/Other",
            "/CanvasScreen/pnlOptions/pnl/pnlVideo/pnlResolution/ddRes"
         };

            var result = PathMatching.MatchesPath( "CanvasScreen/pnlOptions/pnl/pnlVideo/pnlResolution/ddRes/Dropdown List/Viewport/Content/Item 26: 3840x2160*/Item Label", candidates );

            Assert.True( result );
        }
    }
}
