using FakeItEasy;
using FluentAssertions;
using Kontent.Statiq.Tests.Models;
using Kontent.Statiq.Tests.Tools;
using Statiq.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Kontent.Statiq.Tests
{
    public class When_loading_untyped_content_from_Kontent
    {
        [Fact]
        public async Task It_should_correctly_copy_all_fields_into_the_document()
        {
            // Arrange
            var responseJsonPath = Path.Combine(Environment.CurrentDirectory, $"response{Path.DirectorySeparatorChar}getitems.json");
            var responseJson = File.ReadAllText(responseJsonPath);

            var sut = new Statiq.Kontent<Article>(MockDeliveryClient.Create(responseJson));

            var context = A.Fake<IExecutionContext>();

            // Act
            var result = (await sut.ExecuteAsync(context)).ToArray();

            // Assert
            result.Should().NotBeEmpty();
            // TODO : check all the fields!
        }

        [Fact]
        public async Task It_should_correctly_set_the_default_content()
        {
            // Arrange
            var responseJsonPath = Path.Combine(Environment.CurrentDirectory, $"response{Path.DirectorySeparatorChar}getitems.json");
            var responseJson = File.ReadAllText(responseJsonPath);

            var sut = new Statiq.Kontent<Article>(MockDeliveryClient.Create(responseJson));

            var context = A.Fake<IExecutionContext>();
            // Act
            var result = (await sut.ExecuteAsync(context)).ToArray();

            // Assert
            A.CallTo(() => context.CreateDocument(A<NormalizedPath>.Ignored, A<NormalizedPath>.Ignored, A<IEnumerable<KeyValuePair<string, object>>>.Ignored, A<IContentProvider>.Ignored))
                .MustHaveHappened();

            // TODO : verify the document!
        }


    }
}

