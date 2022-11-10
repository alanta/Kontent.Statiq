using FakeItEasy;
using FluentAssertions;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Statiq.Tests.Models;
using Kontent.Statiq.Tests.Tools;
using Statiq.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;
using System.Linq;
using Shouldly;
using Xunit.Sdk;

namespace Kontent.Statiq.Tests
{
    public class When_using_the_items_feed
    {
        [Fact]
        public async Task It_should_invoke_the_correct_endpoint()
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
                .WithFakeContentFeed(content);
            
            var sut = new Kontent<Article>(deliveryClient)
                .WithItemsFeed();

            // Act
            var docs = await sut.ExecuteAsync(A.Fake<IExecutionContext>());

            // Assert
            docs.Should().HaveCount(1);
            A.CallTo(() => deliveryClient.GetItemsFeed<Article>(A<IEnumerable<IQueryParameter>>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task It_should_handle_API_errors()
        {
            // Arrange
            var deliveryClient = A.Fake<IDeliveryClient>()
                .WithFakeContentFeedError<Article>();

            var sut = new Kontent<Article>(deliveryClient)
                .WithItemsFeed();

            // Act
            Func<Task> act = async () => await sut.ExecuteAsync(A.Fake<IExecutionContext>());

            // Assert
            await act.ShouldThrowAsync<InvalidOperationException>();
        }
    }
}
