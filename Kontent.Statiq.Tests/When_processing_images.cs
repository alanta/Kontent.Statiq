using FluentAssertions;
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
            var output = await ExecuteAsync(new[]{document}, assetParser);

            // Then
            output.Length.Should().Be(1);
            var outputDoc = output[0];

            var urls = outputDoc.GetKontentImageDownloads();

            urls.Should().Contain( dl => dl.OriginalUrl == url );
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
    }
}
