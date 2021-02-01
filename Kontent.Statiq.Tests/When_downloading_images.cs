using FluentAssertions;
using Statiq.Common;
using Statiq.Core;
using System.Collections.Generic;
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
            var pipeline = new Pipeline
            {
                InputModules =
                {
                    new ReplaceDocuments( Config.FromContext(ctx =>
                    {
                        var metadata = new[]
                        {
                            new KeyValuePair<string, object>(KontentKeys.Images.Downloads, downloadUrls)
                        };
                        return ctx.CreateDocument(metadata).Yield();
                    })),
                    new KontentDownloadImages()
                }
            };

            var results = await Execute(pipeline);

            // Assert
            results.Should().HaveCount(1);
            results.First().Destination.Should().Be("img/icon.png");
        }
        
        private static Task<IPipelineOutputs> Execute(Pipeline pipeline)
        {
            var engine = new Engine();
            engine.Pipelines.Add("test", pipeline);
            return engine.ExecuteAsync();
        }
    }
}
