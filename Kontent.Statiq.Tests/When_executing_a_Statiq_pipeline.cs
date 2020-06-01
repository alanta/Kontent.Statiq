using FluentAssertions;
using Statiq.Common;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Kontent.Statiq.Tests
{
    public class When_executing_a_Statiq_pipeline
    {
        [Fact]
        public async Task It_should_correctly_copy_all_fields_into_the_document()
        {
            // Arrange

            // Act
            var sut = PipelineExecutionHelpers.SetupExecution( docs => docs.Should().NotBeNull() );
            await sut.ExecuteAsync();

            // Assert
            var outputs = sut.Outputs.FromPipeline("test");
            outputs.Should().HaveCount(1);
            var metadata = outputs.First();

            metadata.Get("title").ToString().Should().Be("Coffee Beverages Explained");
            metadata.Get("body_copy").ToString().Should().Contain("Espresso");
        }

        [Fact]
        public async Task It_should_correctly_set_the_default_content()
        {
            // Arrange

            // Act
            var sut = PipelineExecutionHelpers.SetupExecution(
                // Assert    
                async docs => (await docs.FirstOrDefault().GetContentStringAsync()).Should().Contain("Espresso") 
                );
            await sut.ExecuteAsync();
        }
    }
}
