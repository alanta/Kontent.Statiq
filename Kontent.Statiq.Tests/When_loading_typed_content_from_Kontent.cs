using FakeItEasy;
using FluentAssertions;
using Kentico.Kontent.Delivery.Abstractions;
using Kontent.Statiq.Tests.Models;
using Kontent.Statiq.Tests.Tools;
using Statiq.Common;
using Statiq.Testing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Kontent.Statiq.Tests
{
    public class When_loading_typed_content_from_Kontent
    {
        [Fact]
        public async Task It_should_correctly_materialize_the_document()
        {
            // Arrange
            var responseJsonPath = Path.Combine(Environment.CurrentDirectory, $"response{Path.DirectorySeparatorChar}getitems.json");
            var responseJson = File.ReadAllText(responseJsonPath);

            var sut = new Statiq.Kontent(MockDeliveryClient.Create(responseJson, cfg => cfg
                    .WithTypeProvider(new CustomTypeProvider())))
                    .WithContentField(Article.BodyCopyCodename);

            var context = Setup_ExecutionContext();

            // Act
            var result = (await sut.ExecuteAsync(context)).ToArray();

            // Assert
            result.Should().NotBeEmpty();
            result[0].Should().BeOfType<TestDocument>();
            var article = result[0].AsKontent<Article>();

            article.Title.Should().StartWith("Coffee");
            article.System.Codename.Should().Be("coffee_beverages_explained");
            article.System.Type.Should().Be(Article.Codename);
        }

        [Fact]
        public async Task It_should_resolve_inline_content_types()
        {
            // Arrange
            var responseJsonPath = Path.Combine(Environment.CurrentDirectory, $"response{Path.DirectorySeparatorChar}getitems.json");
            var responseJson = File.ReadAllText(responseJsonPath);

            var sut = new Statiq.Kontent(MockDeliveryClient.Create(responseJson, cfg => cfg
                    .WithTypeProvider(new CustomTypeProvider())))
                    .WithContentField(Article.BodyCopyCodename);

            var context = Setup_ExecutionContext();

            // Act
            var result = (await sut.ExecuteAsync(context)).ToArray();

            // Assert
            result.Should().NotBeEmpty();
            result[0].Should().BeOfType<TestDocument>();
            var article = result[0].AsKontent<Article>();

            article.Title.Should().StartWith("Coffee");
            article.BodyCopy.Blocks.Should().Contain(block => block is IInlineContentItem && ((IInlineContentItem)block).ContentItem is Tweet, "Inline content must be resolved");
        }

        private static IExecutionContext Setup_ExecutionContext()
        {
            var context = A.Fake<IExecutionContext>();
            A.CallTo(() => context.CreateDocument(A<NormalizedPath>.Ignored, A<NormalizedPath>.Ignored, A<IEnumerable<KeyValuePair<string, object>>>.Ignored, A<IContentProvider>.Ignored))
                .ReturnsLazily((NormalizedPath path, NormalizedPath destination, IEnumerable <KeyValuePair<string, object>> metadata, IContentProvider content ) =>
                    new TestDocument(metadata, content)); 
            return context;
        }
    }
}