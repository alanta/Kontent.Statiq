using FluentAssertions;
using Statiq.Common;
using Statiq.Core;
using Statiq.Testing;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Kontent.Statiq.Tests
{
    public class When_downloading_images
    {
        
        [Fact]
        public async Task It_should_set_the_destination()
        {
            // Arrange
            var downloadUrls = new []
            {
                new KontentImageDownload("https://raw.githubusercontent.com/alanta/Kontent.Statiq/main/icon.png",
                    new NormalizedPath("img/icon.png"))
            };
            
            // Act
            var pipeline = new IModule[]
            {
                new ReplaceDocuments(Config.FromContext(ctx =>
                {
                    var metadata = new[]
                    {
                        new KeyValuePair<string, object>(KontentKeys.Images.Downloads, downloadUrls)
                    };
                    return ctx.CreateDocument(metadata).Yield();
                })),
                new KontentDownloadImages()
            };

            var results = await Execute(pipeline);

            // Assert
            results.Should().HaveCount(1);
            results.First().Destination.Should().Be("img/icon.png");
        }

        [Fact]
        public async Task It_should_not_download_cached_images()
        {
            // Arrange
            var downloadUrls = new[]
            {
                new KontentImageDownload("https://raw.githubusercontent.com/alanta/Kontent.Statiq/main/icon.png",
                    new NormalizedPath("img/icon.png"))
            };

            // Act
            var pipeline = new IModule[]
            {
                new ReplaceDocuments(Config.FromContext(ctx =>
                {
                    var metadata = new[]
                    {
                        new KeyValuePair<string, object>(KontentKeys.Images.Downloads, downloadUrls)
                    };
                    return ctx.CreateDocument(metadata).Yield();
                })),
                new KontentDownloadImages()
            };

            var results = await Execute(pipeline);

            // Act
            var results2 = await Execute(pipeline);

            // Assert
            results.Length.Should().Be(1);
            results2.Length.Should().Be(1);
            
            // Note: It's currently impossible to verify that the data is not being fetched twice
            results[0].Destination.Should().Be(results2[0].Destination);
        }
        
        private static Task<ImmutableArray<IDocument>> Execute(params IModule[] modules)
        {
            var context = new TestExecutionContext();
            return context.ExecuteModulesAsync(modules);
        }
    }
}
