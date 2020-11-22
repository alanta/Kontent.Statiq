using FakeItEasy;
using FluentAssertions;
using Kentico.Kontent.Delivery.Abstractions;
using Kontent.Statiq.Tests.Models;
using Kontent.Statiq.Tests.Tools;
using Statiq.Common;
using Statiq.Core;
using Statiq.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Kontent.Statiq.Tests
{
    public class When_working_with_taxonomy
    {
        [Fact]
        public async Task It_should_load_taxonomy()
        {
            // Arrange
            var group = SetupTaxonomyGroup("Test", "Test1", "Test2");
            var deliveryClient = A.Fake<IDeliveryClient>().WithFakeTaxonomy( group );

            // Act
            var pipeline = new Pipeline
            {
                InputModules =
                {
                    new KontentTaxonomy(deliveryClient)
                }
            };

            var results = await Execute(pipeline);

            // Assert
            results.Should().HaveCount(1);
            results.FirstOrDefault().GetChildren().Should().HaveCount(2);
        }

        [Fact]
        public async Task It_should_flatten_taxonomy()
        {
            // Arrange
            var group = SetupTaxonomyGroup("Test", "Test1", "Test2");
            var deliveryClient = A.Fake<IDeliveryClient>().WithFakeTaxonomy(group);

            // Act
            var pipeline = new Pipeline
            {
                InputModules =
                {
                    new KontentTaxonomy(deliveryClient),
                    new FlattenTree(),
                    new SetDestination(Config.FromDocument((doc,ctx)=>
                        new NormalizedPath( doc.Get<string>(KontentKeys.System.Name)))),
                }
            };

            var results = await Execute(pipeline);

            // Assert
            results.Should().HaveCount(3);
            results.Get(new NormalizedPath("Test1")).Get<string>(Keys.TreePath).Should().Be("/Test/Test1");
            results.Get(new NormalizedPath("Test2")).Get<string>(Keys.TreePath).Should().Be("/Test/Test2");
        }

        [Fact]
        public async Task It_should_work_with_lookup()
        {
            // Arrange
            var group = SetupTaxonomyGroup("Sitemap", "Recipe", "Product");
            var articles = new[]
            {
                new Article
                {
                    System = new TestContentItemSystemAttributes(){ Name = "make-cookies" },
                    Title = "How to make cookies",
                    Sitemap = new ITaxonomyTerm[]
                    {
                        new TestTaxonomyTerm {Codename = "Recipe", Name = "Recipe"},
                        new TestTaxonomyTerm {Codename = "Featured", Name = "Featured"}
                    }
                },
                new Article
                {
                    System = new TestContentItemSystemAttributes(){ Name = "make-a-cake" },
                    Title = "How to make a cake",
                    Sitemap = new ITaxonomyTerm[]
                    {
                        new TestTaxonomyTerm {Codename = "Recipe", Name = "Recipe"}
                    }
                },
                new Article
                {
                    System = new TestContentItemSystemAttributes(){ Name = "buy-cookies"},
                    Title = "Chocolate chip cookies",
                    Sitemap = new ITaxonomyTerm[]
                    {
                        new TestTaxonomyTerm {Codename = "Product", Name = "Product"},
                        new TestTaxonomyTerm {Codename = "Featured", Name = "Featured"}
                    }
                }
            };

            var deliveryClient = A.Fake<IDeliveryClient>()
                .WithFakeTaxonomy(group)
                .WithFakeContent(articles);

            // Act
            var pipeline = new Pipeline
            {
                InputModules =
                {
                    new Kontent<Article>(deliveryClient),
                    new SetMetadata("Tags", KontentConfig.Get((Article art) => art.Sitemap.Select( t => t.Codename ).ToArray())),
                }
            };

            var results = await Execute(pipeline);

            // Assert
            var documentsByTag = results.ToLookupMany<string>("Tags");
            documentsByTag["Recipe"].Should().HaveCount(2);
            documentsByTag["Product"].Should().HaveCount(1);
            documentsByTag["Featured"].Should().HaveCount(2); // this fails
        }

        [Fact]
        public void Test()
        {
            // Arrange
            var docs = new[]
            {
                new TestDocument()
                {
                    {"Tags", new[] {"Tag1", "Tag2"}}
                },
                new TestDocument()
                {
                    {"Tags", new[] {"Tag1"}}
                }
            };

            // Act
            var docsByTag = docs.ToLookupMany<string>("Tags");

            // Assert
            docsByTag["Tag1"].Should().HaveCount(2);
            docsByTag["Tag2"].Should().HaveCount(1);

        }

        private ITaxonomyGroup SetupTaxonomyGroup(string name, params string[] terms)
        {
            return new TestTaxonomyGroup
            {
                System = new TestTaxonomyGroupSystemAttributes
                {
                    Name = name,
                    Codename = name,
                    Id = Guid.NewGuid().ToString("N"),
                    LastModified = DateTime.Now
                },
                Terms = terms.Select( t => new TestTaxonomyTermDetails{ Codename = t, Name = t }).Cast<ITaxonomyTermDetails>().ToList()
            };
        }

        private static Task<IPipelineOutputs> Execute(Pipeline pipeline)
        {
            var engine = new Engine();
            engine.Pipelines.Add("test", pipeline);
            return engine.ExecuteAsync();
        }
    }

    internal class TestTaxonomyGroupSystemAttributes : ITaxonomyGroupSystemAttributes
    {
        public string Codename { get; set; }
        public string Id { get; set; }
        public DateTime LastModified { get; set; }
        public string Name { get; set; }
    }

    internal class TestTaxonomyGroup : ITaxonomyGroup
    {
        public ITaxonomyGroupSystemAttributes System { get; set; }
        public IList<ITaxonomyTermDetails> Terms { get; set; }
    }

    internal class TestTaxonomyTermDetails : ITaxonomyTermDetails
    {
        public string Codename { get; set; }
        public string Name { get; set; }
        public IList<ITaxonomyTermDetails> Terms { get; set; }
    }

    internal class TestTaxonomyTerm : ITaxonomyTerm
    {
        public string Codename { get; set; }
        public string Name { get; set; }
    }
}
