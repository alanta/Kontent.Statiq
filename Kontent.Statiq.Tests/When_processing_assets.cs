using FluentAssertions;
using Kentico.Kontent.Delivery.Abstractions;
using Statiq.Common;
using Statiq.Core;
using Statiq.Testing;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Kontent.Statiq.Tests
{
    public class When_processing_assets : BaseFixture
    {
        [Fact]
        public async Task It_should_extract_asset_urls()
        {
            // Given 
            const string url = "https://the.cms/assets/image1.jpg";
            string content = $"<img src='{url}'/>";

            var metadata = new[]
            {
                new KeyValuePair<string, object>("image",
                    new[] { CreateAsset( url ) } ),
            };

            var document = new TestDocument(metadata, content);
            
            // When
            var assetParser = new KontentImageProcessor();
            var output = await ExecuteAsync(new[]{document}, assetParser);

            // Then
            output.Length.Should().Be(1);
            var outputDoc = output[0];

            var urls = outputDoc.GetKontentAssetDownloads();

            urls.Should().Contain( dl => dl.OriginalUrl == url );
        }

        [Theory,
        InlineData("https://the.cms/images/image1.jpg", "https://the.cms/images/image1.jpg", "/assets/img/image1.jpg"),
        InlineData("https://the.cms/images/image1.jpg", "https://the.cms/images/image1.jpg?w=110&h=340", "/assets/img/2cddd89b6d47fa06efe3e14ceada65c7-image1.jpg"),
        InlineData("https://the.cms/images/image1.jpg", "https://the.cms/images/image1.jpg?h=340&w=110", "/assets/img/2cddd89b6d47fa06efe3e14ceada65c7-image1.jpg")]
        public async Task It_should_replace_url_with_local_path(string assetUrl, string sourceUrl, string localUrl)
        {
            // Given 
            string content = $"<img src='{sourceUrl}'/>";

            var metadata = new[]
            {
                new KeyValuePair<string, object>("image",
                    new[] { CreateAsset(assetUrl) } ),
            };

            var document = new TestDocument(metadata, content);

            // When
            var assetParser = new KontentImageProcessor().WithLocalPath("/assets/img");
            var output = await ExecuteAsync(new[] { document }, assetParser);

            // Then
            output.Length.Should().Be(1);
            var outputDoc = output[0];

            outputDoc.Content.Should().Contain($"src=\"{localUrl}\"");
        }


        private Asset CreateAsset(string url)
        {
            var constructor = typeof(Asset).GetConstructors(BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.CreateInstance|BindingFlags.DeclaredOnly)
                .FirstOrDefault(ctor => ctor.GetParameters().Length == 7);

            constructor.Should().NotBeNull("API change detected: not constructor for Asset available with 7 params");

            var asset = constructor.Invoke(new object?[] {"image1.jpg", "", 8192, url, "Sample image", 100, 100});

            return asset as Asset;
        }
    }

    public class DownloadAssetsPipeline : Pipeline
    {
        public DownloadAssetsPipeline()
        {
            Dependencies.Add("Posts");

            InputModules = new ModuleList
            {
                // extract asset urls

            };

            ProcessModules = new ModuleList
            {
                // download the assets
                // update the documents with local urls
            };
        }
    }
}
