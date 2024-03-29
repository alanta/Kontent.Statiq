﻿using FakeItEasy;
using FluentAssertions;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Statiq.Tests.Models;
using Kontent.Statiq.Tests.Tools;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Kontent.Statiq.Tests
{
    public class When_executing_a_Statiq_pipeline
    {
        [Fact]
        public async Task It_should_correctly_copy_all_system_fields_into_the_document()
        {
            // Arrange
            var content = new Article
            {
                System = new TestContentItemSystemAttributes
                {
                    Type = "article",
                    Id = "cf106f4e-30a4-42ef-b313-b8ea3fd3e5c5",
                    Language = "en-US",
                    Codename = "coffee_beverages_explained",
                    Name = "Coffee Beverages Explained",
                    LastModified = new DateTime(2019, 09, 18, 10, 58, 38, 917)
                }
            };

            var deliveryClient = A.Fake<IDeliveryClient>()
                .WithFakeContent(content);
            var sut = new Kontent<Article>(deliveryClient);

            // Act
            var outputs = await Execute(sut);
            
            // Assert
            outputs.Should().HaveCount(1);
            var metadata = outputs.First();

            metadata.Get<string>(KontentKeys.System.Type).Should().Be("article");
            metadata.Get<string>(KontentKeys.System.Id).Should().Be("cf106f4e-30a4-42ef-b313-b8ea3fd3e5c5");
            metadata.Get<string>(KontentKeys.System.Language).Should().Be("en-US");
            metadata.Get<string>(KontentKeys.System.CodeName).Should().Be("coffee_beverages_explained");
            metadata.Get<string>(KontentKeys.System.Name).Should().Be("Coffee Beverages Explained");
            metadata.Get<DateTime>(KontentKeys.System.LastModified).Should().BeCloseTo(new DateTime(2019, 09, 18, 10, 58, 38, 917), TimeSpan.FromMilliseconds(100));
        }

        [Fact]
        public async Task It_should_handle_API_errors()
        {
            // Arrange
            var deliveryClient = A.Fake<IDeliveryClient>()
                .WithFakeContentError<Article>();

            var sut = new Kontent<Article>(deliveryClient);

            // Act
            Func<Task> act = async () => await sut.ExecuteAsync(A.Fake<IExecutionContext>());

            // Assert
            await act.ShouldThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task It_should_warn_on_empty_result()
        {
            // Arrange
            var deliveryClient = A.Fake<IDeliveryClient>()
                .WithFakeContent<Article>();

            var context = A.Fake<IExecutionContext>();
            var logger = A.Fake<ILogger>();
            A.CallTo(() => context.Logger).Returns(logger);
            
            var sut = new Kontent<Article>(deliveryClient);

            // Act
            var result = await sut.ExecuteAsync(context);

            // Assert
            result.Should().HaveCount(0);
            logger.VerifyLogged(LogLevel.Warning, "Query for Article returned no results");
        }

        [Fact]
        public async Task It_should_correctly_set_the_default_content()
        {
            // Arrange
            var content = new Article {Title = "Coffee is essential"};

            var deliveryClient = A.Fake<IDeliveryClient>().WithFakeContent(content);

            var sut = new Kontent<Article>(deliveryClient)
                .WithContent(item => item.Title);

            // Act
            var output = await Execute(sut);

            // Assert    
            (await output.FirstOrDefault().GetContentStringAsync()).Should().Contain("Coffee");
        }

        [Fact]
        public async Task It_should_correctly_set_the_default_content_from_rich_text()
        {
            // Arrange
            var body = new TestRichTextContent {"Coffee"};

            var content = new Article { BodyCopy = body };

            var deliveryClient = A.Fake<IDeliveryClient>().WithFakeContent(content);

            var sut = new Kontent<Article>(deliveryClient)
                .WithContent(item => item.BodyCopy.ToString() ?? "" );

            // Act
            var output = await Execute(sut);

            // Assert
            (await output.FirstOrDefault().GetContentStringAsync()).Should().Contain("Coffee");

        }

        private static Task<ImmutableArray<IDocument>> Execute<TContent>(Kontent<TContent> kontentModule ) where TContent : class
        {
            TestExecutionContext context = new TestExecutionContext();
            return context.ExecuteModulesAsync(new IModule[]{ kontentModule });
        }
    }
}
