using FluentAssertions;
using Statiq.Common;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Kontent.Statiq.Tests
{
    public class When_generating_local_asset_paths
    {
        [Fact]
        public void It_should_use_the_file_name_from_the_url()
        {
            // Given
            const string url = "https://the.cms/assets/image1.jpg";

            // When
            var localPath = KontentAssetHelper.GetLocalFileName(url, new NormalizedPath("img"));

            // Then
            localPath.FullPath.Should().Be("img/image1.jpg");
        }

        [Fact]
        public void It_should_hash_the_query_string_into_the_file_name()
        {
            // Given
            const string url = "https://the.cms/assets/image1.jpg?w=100&h=200";

            // When
            var localPath = KontentAssetHelper.GetLocalFileName(url, new NormalizedPath("img"));

            // Then
            // The hash keeps the applied transformations out of the public url. It must stay
            // stable over time, otherwise every existing site regenerates all of its assets.
            localPath.FullPath.Should().Be("img/e3c8446142ee184e3db3c09aabaf9b82-image1.jpg");
        }

        [Fact]
        public void It_should_ignore_the_order_of_the_query_parameters()
        {
            // Given
            const string url = "https://the.cms/assets/image1.jpg?w=100&h=200";
            const string sameUrlReordered = "https://the.cms/assets/image1.jpg?h=200&w=100";

            // When
            var localPath = KontentAssetHelper.GetLocalFileName(url, new NormalizedPath("img"));
            var otherLocalPath = KontentAssetHelper.GetLocalFileName(sameUrlReordered, new NormalizedPath("img"));

            // Then
            otherLocalPath.FullPath.Should().Be(localPath.FullPath);
        }

        [Fact]
        public async Task It_should_be_safe_to_call_from_multiple_threads()
        {
            // Given
            // Statiq processes documents concurrently, so KontentImageProcessor ends up calling
            // this helper from many threads at once. See issue #44.
            var urls = Enumerable.Range(0, 500)
                .Select(i => $"https://the.cms/assets/image{i}.jpg?w={i}&h={i * 2}")
                .ToArray();

            // When
            var work = urls.Select(url => Task.Run(
                () => KontentAssetHelper.GetLocalFileName(url, new NormalizedPath("img")).FullPath));

            Func<Task> act = async () => await Task.WhenAll(work);

            // Then
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public void It_should_produce_the_same_hash_from_multiple_threads()
        {
            // Given
            const string url = "https://the.cms/assets/image1.jpg?w=100&h=200";
            var expected = KontentAssetHelper.GetLocalFileName(url, new NormalizedPath("img")).FullPath;

            // When
            var results = Enumerable.Range(0, 500)
                .AsParallel()
                .WithDegreeOfParallelism(Math.Min(8, Environment.ProcessorCount * 2))
                .Select(_ => KontentAssetHelper.GetLocalFileName(url, new NormalizedPath("img")).FullPath)
                .ToArray();

            // Then
            results.Should().AllBe(expected);
        }
    }
}
