using FakeItEasy;
using FluentAssertions;
using Kentico.Kontent.Delivery.Abstractions;
using Kontent.Statiq.Tests.Models;
using Kontent.Statiq.Tests.Tools;
using Statiq.Common;
using Statiq.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            var engine = SetupExecution( sut, docs =>
            {
                docs.Should().NotBeNull();
                return Task.CompletedTask;
            });
            await engine.ExecuteAsync();

            // Assert
            var outputs = engine.Outputs.FromPipeline("test");
            outputs.Should().HaveCount(1);
            var metadata = outputs.First();

            metadata.Get<string>(KontentKeys.System.Type).Should().Be("article");
            metadata.Get<string>(KontentKeys.System.Id).Should().Be("cf106f4e-30a4-42ef-b313-b8ea3fd3e5c5");
            metadata.Get<string>(KontentKeys.System.Language).Should().Be("en-US");
            metadata.Get<string>(KontentKeys.System.CodeName).Should().Be("coffee_beverages_explained");
            metadata.Get<string>(KontentKeys.System.Name).Should().Be("Coffee Beverages Explained");
            metadata.Get<DateTime>(KontentKeys.System.LastModified).Should().BeCloseTo(new DateTime(2019, 09, 18, 10, 58, 38, 917));
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
            var engine = SetupExecution(sut, 
            
            // Assert    
                async docs => (await docs.FirstOrDefault().GetContentStringAsync()).Should().Contain("Coffee")
                );
            await engine.ExecuteAsync();
        }

        [Fact]
        public async Task It_should_correctly_set_the_default_content_from_richtext()
        {
            // Arrange
            var body = new TestRichTextContent();
            body.Add("Coffee");
            
            var content = new Article { BodyCopy = body };

            var deliveryClient = A.Fake<IDeliveryClient>().WithFakeContent(content);

            var sut = new Kontent<Article>(deliveryClient)
                .WithContent(item => item.BodyCopy.ToString() ?? "" );

            // Act
            var engine = SetupExecution(sut,

                // Assert    
                async docs => (await docs.FirstOrDefault().GetContentStringAsync()).Should().Contain("Coffee")
            );
            await engine.ExecuteAsync();
        }

        private static Engine SetupExecution<TContent>(Kontent<TContent> kontentModule, Func<IReadOnlyList<IDocument>, Task> test ) where TContent : class
        {
            var engine = new Engine();
            var pipeline = new Pipeline()
            {
                InputModules = { kontentModule, new TestModule(test) }
            };

            engine.Pipelines.Add("test", pipeline);
            return engine;
        }
    }
}
