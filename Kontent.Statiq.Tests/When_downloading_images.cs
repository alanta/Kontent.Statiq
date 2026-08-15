using FluentAssertions;
using Statiq.Common;
using Statiq.Core;
using Statiq.Testing;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;

namespace Kontent.Statiq.Tests
{
    public class When_downloading_images
    {
        private const string ImageUrl = "https://the.cms/assets/icon.png";
        private static readonly byte[] ImageData = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

        [Fact]
        public async Task It_should_set_the_destination()
        {
            // Arrange
            var server = new FakeAssetServer();
            var pipeline = CreatePipeline(new KontentImageDownload(ImageUrl, new NormalizedPath("img/icon.png")));

            // Act
            var results = await Execute(server, pipeline);

            // Assert
            results.Should().HaveCount(1);
            results.First().Destination.Should().Be("img/icon.png");
        }

        [Fact]
        public async Task It_should_not_download_cached_images()
        {
            // Arrange
            var server = new FakeAssetServer();

            // The module instance is shared between both executions, just like it is when Statiq
            // re-renders the site in preview mode.
            var pipeline = CreatePipeline(new KontentImageDownload(ImageUrl, new NormalizedPath("img/icon.png")));

            // Act
            var results = await Execute(server, pipeline);
            var requestsAfterFirstRun = server.RequestCount;

            var results2 = await Execute(server, pipeline);

            // Assert
            results.Length.Should().Be(1);
            results2.Length.Should().Be(1);
            results2[0].Destination.Should().Be(results[0].Destination);

            requestsAfterFirstRun.Should().Be(1, "the image should be downloaded on the first run");
            server.RequestCount.Should().Be(1, "the second run should be served from the cache");
        }

        [Fact]
        public async Task It_should_serve_the_cached_content_on_a_second_run()
        {
            // Arrange
            var server = new FakeAssetServer();
            var pipeline = CreatePipeline(new KontentImageDownload(ImageUrl, new NormalizedPath("img/icon.png")));

            // Act
            var results = await Execute(server, pipeline);
            var results2 = await Execute(server, pipeline);

            // Assert
            (await results[0].GetContentBytesAsync()).Should().Equal(ImageData);
            (await results2[0].GetContentBytesAsync()).Should().Equal(ImageData);
            results2[0].ContentProvider.MediaType.Should().Be(results[0].ContentProvider.MediaType);
        }

        [Fact]
        public async Task It_should_download_an_asset_used_by_several_documents_only_once()
        {
            // Arrange
            var server = new FakeAssetServer();
            var download = new KontentImageDownload(ImageUrl, new NormalizedPath("img/icon.png"));

            // Two documents referencing the same asset
            var pipeline = new IModule[]
            {
                new ReplaceDocuments(Config.FromContext(ctx => new[]
                {
                    ctx.CreateDocument(WithDownloads(download)),
                    ctx.CreateDocument(WithDownloads(download))
                }.AsEnumerable())),
                new KontentDownloadImages()
            };

            // Act
            var results = await Execute(server, pipeline);

            // Assert
            results.Should().HaveCount(1);
            server.RequestCount.Should().Be(1);
        }

        private static IModule[] CreatePipeline(params KontentImageDownload[] downloads) => new IModule[]
        {
            new ReplaceDocuments(Config.FromContext(ctx => ctx.CreateDocument(WithDownloads(downloads)).Yield())),
            new KontentDownloadImages()
        };

        private static KeyValuePair<string, object>[] WithDownloads(params KontentImageDownload[] downloads) => new[]
        {
            new KeyValuePair<string, object>(KontentKeys.Images.Downloads, downloads)
        };

        private static Task<ImmutableArray<IDocument>> Execute(FakeAssetServer server, params IModule[] modules)
        {
            var context = new TestExecutionContext
            {
                HttpResponseFunc = (request, _) => server.Respond(request)
            };
            return context.ExecuteModulesAsync(modules);
        }

        /// <summary>
        /// Serves image data and keeps track of what was requested, so tests can tell the
        /// difference between a download and a cache hit.
        /// </summary>
        private sealed class FakeAssetServer
        {
            private readonly ConcurrentBag<string> _requests = new();

            public int RequestCount => _requests.Count;

            public HttpResponseMessage Respond(HttpRequestMessage request)
            {
                _requests.Add(request.RequestUri!.ToString());

                var content = new ByteArrayContent(ImageData);
                content.Headers.ContentType = new MediaTypeHeaderValue(MediaTypes.Png);

                return new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
            }
        }
    }
}
