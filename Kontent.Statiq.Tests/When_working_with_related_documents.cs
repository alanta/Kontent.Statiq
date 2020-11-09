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
    public class When_working_with_related_documents
    {
        [Fact]
        public async Task It_should_map_child_page_collections()
        {
            // Arrange
            var home = new Home();
            var sub1 = CreateArticle("Sub 1");
            var sub2 = CreateArticle("Sub 2");
            home.Articles = new[] { sub1, sub2 };

            var deliveryClient = A.Fake<IDeliveryClient>().WithFakeContent(home);

            var sut = new Kontent<Home>(deliveryClient);

            // Act
            var engine = SetupExecution(sut,
               new[]{
                new AddDocumentsToMetadata(Keys.Children, KontentConfig.GetChildren<Home>( page => page.Articles ) ),
                },
                // Assert    
                async docs => docs.FirstOrDefault().GetChildren().Should().HaveCount(2)
            );
            await engine.ExecuteAsync();
        }

        [Fact]
        public async Task It_should_allow_multiple_child_page_collections()
        {
            // Arrange
            var home = new Home();
            var sub1 = CreateArticle("Sub 1");
            var sub2 = CreateArticle("Sub 2");
            home.Articles = new[] { sub1, sub2 };
            home.Cafes = new[]
            {
                CreateCafe("Ok Café")
            };

            var deliveryClient = A.Fake<IDeliveryClient>().WithFakeContent(home);

            var sut = new Kontent<Home>(deliveryClient);

            // Act
            var engine = SetupExecution(sut,
                new[]{
                    new AddKontentDocumentsToMetadata<Home>(page => page.Articles),
                    new AddKontentDocumentsToMetadata<Home>("cafes", page => page.Cafes),
                },

                // Assert    
                async docs =>
                {
                    var articles = docs.FirstOrDefault().GetChildren();
                    articles.Should().HaveCount(2);
                    articles.Should().Contain(a => string.Equals(a[KontentKeys.System.Name], "Sub 1"));
                    articles.Should().Contain(a => string.Equals(a[KontentKeys.System.Name], "Sub 2"));

                    var cafes = docs.FirstOrDefault().GetChildren("cafes");
                    cafes.Should().HaveCount(1);
                    cafes.First()[KontentKeys.System.Name].Should().Be("Ok Café");

                });
            await engine.ExecuteAsync();
        }

        [Fact]
        public async Task It_should_not_throw_on_null_or_empty_collections()
        {
            // Arrange
            var home = new Home
            {
                Articles = null!, 
                Cafes = new Cafe[0]
            };

            var deliveryClient = A.Fake<IDeliveryClient>().WithFakeContent(home);

            var sut = new Kontent<Home>(deliveryClient);

            // Act
            var engine = SetupExecution(sut,
                new[]{
                    new AddKontentDocumentsToMetadata<Home>(page => page.Articles),
                    new AddKontentDocumentsToMetadata<Home>("cafes", page => page.Cafes),
                },

                // Assert    
                async docs =>
                {
                    var articles = docs.FirstOrDefault().GetChildren();
                    articles.Should().HaveCount(0);

                    var cafes = docs.FirstOrDefault().GetChildren("cafes");
                    cafes.Should().HaveCount(0);
                });
            await engine.ExecuteAsync();
        }

        private static Article CreateArticle(string content)
        {
            var body = new TestRichTextContent {content};

            return new Article { BodyCopy = body, System = new TestContentItemSystemAttributes{ Name = content } };
        }

        private static Cafe CreateCafe(string name)
        {
            return new Cafe { System = new TestContentItemSystemAttributes{ Name = name }};
        }

        private static Engine SetupExecution<TContent>(Kontent<TContent> kontentModule, IModule[] processModules, Func<IReadOnlyList<IDocument>, Task> test) where TContent : class
        {
            var engine = new Engine();
            var pipeline = new Pipeline()
            {
                InputModules = { kontentModule },
                OutputModules = { new TestModule(test) }
            };
            pipeline.ProcessModules.AddRange(processModules);

            engine.Pipelines.Add("test", pipeline);
            return engine;
        }
    }
}
