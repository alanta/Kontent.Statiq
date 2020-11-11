using Statiq.Common;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Kontent.Statiq.Tests
{
    public class LinqExtensionsTests
    {
        [Fact]
        public void DistinctRemovesDuplicateEntriesFromASequence()
        {
            // Arrange
            var assets = new List<KontentImageDownload> {
                new KontentImageDownload("a", new NormalizedPath("/a/")),
                new KontentImageDownload("a", new NormalizedPath("/a/")),
                new KontentImageDownload("b", new NormalizedPath("/b/"))
            };

            // Act
            var assetUrls = assets.DistinctBy(a => a.LocalPath);

            // Assert
            Assert.Equal(2, assetUrls.Count());
            Assert.NotEqual(assetUrls.FirstOrDefault().LocalPath, assetUrls.LastOrDefault().LocalPath);
        }
    }
}
