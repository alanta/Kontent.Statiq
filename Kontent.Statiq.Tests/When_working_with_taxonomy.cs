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
    public class When_working_with_taxonomy
    {
        [Fact]
        public async Task It_should_load_taxonomy()
        {
            // Arrange
            var group = setupTaxonomyGroup("Test", "Test1", "Test2");
            var deliveryClient = A.Fake<IDeliveryClient>().WithFakeTaxonomy( group );

            var sut = new KontentTaxonomy(deliveryClient);

            // Act
            var engine = SetupExecution(sut,
                new IModule[]{
                    
                },
                // Assert    
                async docs => docs.FirstOrDefault().GetChildren().Should().HaveCount(2)
            );
            await engine.ExecuteAsync();
        }

        private ITaxonomyGroup setupTaxonomyGroup(string name, params string[] terms)
        {
            return new TestTaxonomyGroup
            {
                System = A.Fake<ITaxonomyGroupSystemAttributes>(),
                Terms = terms.Select( t => new TestTaxonomyTerm{ Codename = t, Name = t }).Cast<ITaxonomyTermDetails>().ToList()
            };
        }

        private static Engine SetupExecution(KontentTaxonomy kontentModule, IModule[] processModules, Func<IReadOnlyList<IDocument>, Task> test)
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

    internal class TestTaxonomyGroup : ITaxonomyGroup
    {
        public ITaxonomyGroupSystemAttributes System { get; set; }
        public IList<ITaxonomyTermDetails> Terms { get; set; }
    }

    internal class TestTaxonomyTerm : ITaxonomyTermDetails
    {
        public string Codename { get; set; }
        public string Name { get; set; }
        public IList<ITaxonomyTermDetails> Terms { get; set; }
    }
}
