using FakeItEasy;
using FluentAssertions;
using Kentico.Kontent.Delivery.Abstractions;
using Kentico.Kontent.Delivery.Urls.QueryParameters.Filters;
using Kontent.Statiq.Tests.Models;
using Kontent.Statiq.Tests.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Statiq.Common;
using Statiq.Core;
using Statiq.Razor;
using Statiq.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Kontent.Statiq.Tests
{
    public class When_working_with_taxonomy
    {
        private readonly ITestOutputHelper _output;

        public When_working_with_taxonomy(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task It_should_load_taxonomy()
        {
            // Arrange
            var group = SetupTaxonomyGroup("Test", "Test1", "Test2");
            group.Terms.First().Terms.AddRange( CreateTaxonomyTerms("Test1a", "Test1b", "Test1c") );
            var deliveryClient = A.Fake<IDeliveryClient>().WithFakeTaxonomy( group );

            // Act
            var pipeline = new Pipeline
            {
                InputModules =
                {
                    new KontentTaxonomy(deliveryClient)
                        .WithNesting(),
                }
            };

            var results = await Execute(pipeline);

            // Assert
            results.Should().HaveCount(1);
            results.First().GetChildren().Should().HaveCount(2);
            results.First().GetChildren().First().GetChildren().Should().HaveCount(3);
        }

        [Fact]
        public async Task It_should_load_taxonomy_and_retrieve_it()
        {
            // Arrange
            var group = SetupTaxonomyGroup("Test", "Test1", "Test2");
            var deliveryClient = A.Fake<IDeliveryClient>().WithFakeTaxonomy(group);

            // Act
            var pipeline = new Pipeline
            {
                InputModules =
                {
                    new KontentTaxonomy(deliveryClient)
                        .WithNesting()
                }
            };

            var results = await Execute(pipeline);

            // Assert
            results.First().AsKontentTaxonomy().Should().BeEquivalentTo(group);
        }

        [Fact]
        public async Task It_should_load_taxonomy_and_retrieve_its_terms()
        {
            // Arrange
            var group = SetupTaxonomyGroup("Test", "Test1", "Test2");
            var deliveryClient = A.Fake<IDeliveryClient>().WithFakeTaxonomy(group);

            // Act
            var pipeline = new Pipeline
            {
                InputModules =
                {
                    new KontentTaxonomy(deliveryClient)
                        .WithNesting()
                }
            };

            var results = await Execute(pipeline);

            // Assert
            results.First().AsKontentTaxonomyTerms().Should().HaveCount(2);

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
                    new KontentTaxonomy(deliveryClient)
                        .WithNesting(),
                    new FlattenTree(),
                    new SetDestination(Config.FromDocument((doc,ctx)=>
                        new NormalizedPath( doc.Get<string>(KontentKeys.System.Name)))),
                }
            };

            var results = await Execute(pipeline);

            // Assert
            results.Should().HaveCount(3);
            results.Get(new NormalizedPath("Test1")).Get<string[]>(Keys.TreePath).Should().BeEquivalentTo("test", "test1");
            results.Get(new NormalizedPath("Test2")).Get<string[]>(Keys.TreePath).Should().BeEquivalentTo("test", "test2");
        }

        [Fact]
        public async Task It_should_work_with_lookup()
        {
            // Arrange
            var recipe = new TestTaxonomyTerm {Codename = "recipe-tag", Name = "Recipe"};
            var featured = new TestTaxonomyTerm { Codename = "featured-tag", Name = "Featured" };
            var product = new TestTaxonomyTerm {Codename = "product-tag", Name = "Product"};

            var articles = new[]
            {
                new Article
                {
                    System = new TestContentItemSystemAttributes(){ Name = "make-cookies" },
                    Title = "How to make cookies",
                    Sitemap = new ITaxonomyTerm[] {recipe, featured}
                },
                new Article
                {
                    System = new TestContentItemSystemAttributes(){ Name = "make-a-cake" },
                    Title = "How to make a cake",
                    Sitemap = new ITaxonomyTerm[] {recipe}
                },
                new Article
                {
                    System = new TestContentItemSystemAttributes(){ Name = "buy-cookies"},
                    Title = "Chocolate chip cookies",
                    Sitemap = new ITaxonomyTerm[] { product, featured }
                }
            };

            var deliveryClient = A.Fake<IDeliveryClient>()
                .WithFakeContent(articles);

            // Act
            var pipeline = new Pipeline
            {
                InputModules =
                {
                    new Kontent<Article>(deliveryClient),
                    // Set taxonomy terms as metadata
                    new SetMetadata("Tags", KontentConfig.Get((Article art) => art.Sitemap)),
                }
            };

            var results = await Execute(pipeline);
            var documentsByTag = results.ToLookupManyByTaxonomy("Tags");

            // Assert
            documentsByTag[recipe].Should().HaveCount(2);
            documentsByTag[new TestTaxonomyTerm{ Codename = product.Codename }].Should().HaveCount(1);
            documentsByTag[featured].Should().Contain( x => x.AsKontent<Article>().Title == articles[0].Title );
        }

        [Fact]
        public async Task It_should_enable_grouping_documents_into_trees()
        {
            // Arrange
            var group = SetupTaxonomyGroup("Menu", "Product", "Recipe", "Featured");
            group.Terms.First().Terms.AddRange(CreateTaxonomyTerms("Sugar-free","Vegan","Low-carb"));

            var recipe = new TestTaxonomyTerm { Codename = "recipe", Name = "Recipe" };
            var featured = new TestTaxonomyTerm { Codename = "featured", Name = "Featured" };
            var product = new TestTaxonomyTerm { Codename = "product", Name = "Product" };
            var lowCarb = new TestTaxonomyTerm {Codename = "low-carb", Name = "Low-carb"};

            var articles = new[]
            {
                new Article
                {
                    System = new TestContentItemSystemAttributes(){ Name = "make-cookies" },
                    Title = "How to make cookies",
                    Sitemap = new ITaxonomyTerm[] {recipe, featured}
                },
                new Article
                {
                    System = new TestContentItemSystemAttributes(){ Name = "make-a-cake" },
                    Title = "How to make a cake",
                    Sitemap = new ITaxonomyTerm[] {recipe}
                },
                new Article
                {
                    System = new TestContentItemSystemAttributes(){ Name = "buy-cookies"},
                    Title = "Chocolate chip cookies",
                    Sitemap = new ITaxonomyTerm[] { product, featured }
                },
                new Article
                {
                    System = new TestContentItemSystemAttributes(){ Name = "low-carb-cookies"},
                    Title = "Low-carb cookies",
                    Sitemap = new ITaxonomyTerm[] { product, lowCarb }
                }
            };

            var deliveryClient = A.Fake<IDeliveryClient>()
                .WithFakeTaxonomy(group)
                .WithFakeContent(articles);

            // Act
            var articlePipeline = new Pipeline
            {
                InputModules =
                {
                    new Kontent<Article>(deliveryClient),
                    // Set taxonomy terms as metadata
                    new SetMetadata("Tags", KontentConfig.Get((Article art) => art.Sitemap)),
                    new GroupDocuments( "Tags" ).WithComparer(new TaxonomyTermComparer())
                }
            };

            var menuPipeline = new Pipeline
            {
                Dependencies = { "Articles" },
                InputModules =
                {
                    new KontentTaxonomy(deliveryClient)
                        .WithQuery(new EqualsFilter("system.codename", "menu")),
                },
                ProcessModules = {
                    new SetMetadata( Keys.Children, Config.FromDocument((doc, ctx) =>
                    {
                        var taxonomyTerm = doc.AsKontentTaxonomyTerm()?.Codename;

                        return ctx.Outputs.FromPipeline("Articles")
                            .FirstOrDefault(x => x.AsKontentTaxonomyTerm(Keys.GroupKey)?.Codename == taxonomyTerm)
                            ?.GetChildren().ToArray();
                    }))
                }
            };

            var engine = new Engine(
                new ApplicationState(Array.Empty<string>(), "", ""),
                new TestServiceProvider(cfg =>
                {
                    cfg.AddRazor();
                    cfg.AddSingleton<ILoggerFactory>(new XUnitLoggerFactory(_output));
                }));
            
            engine.Pipelines.Add("Articles", articlePipeline);
            engine.Pipelines.Add("Menu", menuPipeline);
            var results = await engine.ExecuteAsync();

            // Assert
            var sitemap = results.FromPipeline("Menu");
            sitemap.Select(node => node.Get<string>(Keys.Title))
                .Should().BeEquivalentTo("Product", "Featured", "Vegan", "Low-carb", "Recipe", "Sugar-free");
            
            var productSection = sitemap.FirstOrDefault( p => p.Get<string>(Keys.Title) == "Product" );
            productSection.GetChildren().Should().HaveCount(2);
            var lowCarbSection = sitemap.FirstOrDefault( p => p.Get<string>(Keys.Title) == "Low-carb" );
            lowCarbSection.GetChildren().Should().HaveCount(1);
            lowCarbSection.Get<string[]>(Keys.TreePath).Should().Equal( "product", "low-carb");
            var featuredSection = sitemap.FirstOrDefault(p => p.Get<string>(Keys.Title) == "Featured");
            featuredSection.GetChildren().Should().HaveCount(2);
            
            _output.WriteLine( string.Join("\n", sitemap.Select( doc => string.Join( "/", doc.Get<string[]>(Keys.TreePath)) + $" ({doc.GetChildren().Count})")));
        }


        private ITaxonomyGroup SetupTaxonomyGroup(string name, params string[] terms)
        {
            var group = new TestTaxonomyGroup(new TestTaxonomyGroupSystemAttributes
            {
                Name = name,
                Codename = name.ToLower(),
                Id = Guid.NewGuid().ToString("N"),
                LastModified = DateTime.Now
            });
            group.Terms.AddRange(CreateTaxonomyTerms(terms));
            return group;
        }

        private IList<ITaxonomyTermDetails> CreateTaxonomyTerms(params string[] terms)
        {
            return terms.Select(t => new TestTaxonomyTermDetails {Codename = t.ToLower(), Name = t})
                .Cast<ITaxonomyTermDetails>().ToList();
        }

        private static Task<IPipelineOutputs> Execute(Pipeline pipeline)
        {
            var engine = new Engine(new ApplicationState(Array.Empty<string>(), "", ""),
                new TestServiceProvider(cfg => cfg.AddRazor()));
            
            engine.Pipelines.Add("test", pipeline);
            return engine.ExecuteAsync();
        }
    }

    internal class TestTaxonomyGroupSystemAttributes : ITaxonomyGroupSystemAttributes
    {
        public string Codename { get; set; } = "";
        public string Id { get; set; } = "";
        public DateTime LastModified { get; set; }
        public string Name { get; set; } = "";
    }

    internal class TestTaxonomyGroup : ITaxonomyGroup
    {
        public TestTaxonomyGroup(ITaxonomyGroupSystemAttributes system)
        {
            System = system;
        }
        public ITaxonomyGroupSystemAttributes System { get; }
        public IList<ITaxonomyTermDetails> Terms { get; } = new List<ITaxonomyTermDetails>();
    }

    internal class TestTaxonomyTermDetails : ITaxonomyTermDetails
    {
        public string Codename { get; set; } = "";
        public string Name { get; set; } = "";
        public IList<ITaxonomyTermDetails> Terms { get; } = new List<ITaxonomyTermDetails>();
    }

    internal class TestTaxonomyTerm : ITaxonomyTerm
    {
        public string Codename { get; set; } = "";
        public string Name { get; set; } = "";
    }
}
