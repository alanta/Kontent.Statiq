using FluentAssertions;
using Shouldly;
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
            var assetUrls = assets.DistinctBy(a => a.LocalPath).ToArray();

            // Assert
            assetUrls.Should().HaveCount(2);
            assetUrls.Select( i => i.LocalPath ).Should()
              .BeEquivalentTo(
                new[]{ 
                    new NormalizedPath("/a/"), 
                    new NormalizedPath("/b/") 
                    });
        }
    }
}
