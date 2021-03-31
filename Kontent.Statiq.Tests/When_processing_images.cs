using FluentAssertions;
using Statiq.Common;
using Statiq.Testing;
using System.Threading.Tasks;
using Xunit;

namespace Kontent.Statiq.Tests
{
    public class When_processing_images : BaseFixture
    {
        [Fact]
        public async Task It_should_extract_asset_urls()
        {
            // Given 
            const string url = "https://the.cms/assets/image1.jpg";
            string content = $"<img src='{url}'/>";

            var document = new TestDocument(content);

            // When
            var assetParser = new KontentImageProcessor();
            var output = await ExecuteAsync(new[] { document }, assetParser);

            // Then
            output.Length.Should().Be(1);
            var outputDoc = output[0];

            var urls = outputDoc.GetKontentImageDownloads();

            urls.Should().Contain(dl => dl.OriginalUrl == url);
        }

        [Theory,
        InlineData("https://the.cms/images/image1.jpg", "/assets/img/image1.jpg"),
        InlineData("https://the.cms/images/image1.jpg?w=110&h=340", "/assets/img/2cddd89b6d47fa06efe3e14ceada65c7-image1.jpg"),
        InlineData("https://the.cms/images/image1.jpg?h=340&w=110", "/assets/img/2cddd89b6d47fa06efe3e14ceada65c7-image1.jpg")]
        public async Task It_should_replace_url_with_local_path(string sourceUrl, string localUrl)
        {
            // Given 
            string content = $"<img src='{sourceUrl}'/>";

            var document = new TestDocument(content);

            // When
            
            var assetParser = new KontentImageProcessor().WithLocalPath("/assets/img");
            var output = await ExecuteAsync(new[] { document }, assetParser);

            // Then
            output.Length.Should().Be(1);
            var outputDoc = output[0];

            outputDoc.Content.Should().Contain($"src=\"{localUrl}\"");
        }

        [Theory,
         InlineData("https://the.cms/images/image1.jpg", "/images", "/images/image1.jpg"),
         InlineData("https://the.cms/images/image2.jpg", "unrootedPath/test", "/unrootedPath/test/image2.jpg"),
         InlineData("https://the.cms/images/image1.jpg?w=110&h=340", "/thumbnails/", "/thumbnails/2cddd89b6d47fa06efe3e14ceada65c7-image1.jpg")]
        public async Task It_should_apply_config_for_local_path(string sourceUrl, string path, string localUrl)
        {
            // Given 
            string content = $"<img src='{sourceUrl}'/>";

            var document = new TestDocument(content);

            // When

            var assetParser = new KontentImageProcessor().WithLocalPath(Config.FromValue(new NormalizedPath(path)));
            var output = await ExecuteAsync(new[] { document }, assetParser);

            // Then
            output.Length.Should().Be(1);
            var outputDoc = output[0];

            outputDoc.Content.Should().Contain($"src=\"{localUrl}\"");
        }

        [Theory,
         InlineData("<meta property=\"og:image\" content=\"https://the.cms/images/image1.jpg\"/>", "", "/assets/img/image1.jpg"),
         InlineData("<meta property=\"og:image\" content=\"https://the.cms/images/image1.jpg\"/>", "test.alanta.nl", "https://test.alanta.nl/assets/img/image1.jpg")
         ]
        public async Task It_should_replace_meta_url_with_local_path(string tag, string host, string localUrl)
        {
            // Given 
            string content = $"<html><head>{tag}</head><body></body>";

            var document = new TestDocument(content);

            var context = new TestExecutionContext();
            context.Settings[Keys.Host] = host;
            context.Settings[Keys.LinksUseHttps] = true;

            // When
            var assetParser = new KontentImageProcessor().WithLocalPath("/assets/img");
            var output = await ExecuteAsync(new[] { document }, context, assetParser);

            // Then
            output.Length.Should().Be(1);
            var outputDoc = output[0];

            outputDoc.Content.Should().Contain($"content=\"{localUrl}\"");
        }
        
        [Theory,
         InlineData("<div style=\"background-image:url('https://the.cms/images/image1.jpg')\">", "", "background-image:url('/assets/img/image1.jpg')"),
         InlineData("<div style=\"background:url('https://the.cms/images/image1.jpg')\">", "", "url('/assets/img/image1.jpg')"),
         InlineData("<div style=\"background: no-repeat center/80% url('https://the.cms/images/image1.jpg')\">", "", "background: no-repeat center/80% url('/assets/img/image1.jpg')"),
         InlineData("<div style=\"background-image: url('../dont-replace-me.jpg'), url('https://the.cms/media/examples/lizard.png')\">", "", "background-image: url('../dont-replace-me.jpg'), url('/assets/img/lizard.png')"),
         InlineData("<div style=\"background-image:url(https://the.cms/images/image1.jpg)\">", "test.alanta.nl", "https://test.alanta.nl/assets/img/image1.jpg")
        ]
        public async Task It_should_replace_backgroundimage_url_with_local_path(string tag, string host, string localUrl)
        {
            // Given 
            string content = $"<html><body>{tag}</body></html>";

            var document = new TestDocument(content);

            var context = new TestExecutionContext();
            context.Settings[Keys.Host] = host;
            context.Settings[Keys.LinksUseHttps] = true;

            // When
            var assetParser = new KontentImageProcessor().WithLocalPath("/assets/img");
            var output = await ExecuteAsync(new[] { document }, context, assetParser);

            // Then
            output.Length.Should().Be(1);
            var outputDoc = output[0];

            outputDoc.Content.Should().Contain(localUrl);
        }
    }
}
