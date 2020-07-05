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
        public async Task It_should_correctly_copy_all_system_fields_into_the_document()
        {
            // Arrange

            // Act
            var sut = PipelineExecutionHelpers.SetupExecution(docs =>
            {
                docs.Should().NotBeNull();
                return Task.CompletedTask;
            });
            await sut.ExecuteAsync();

            // Assert
            var outputs = sut.Outputs.FromPipeline("test");
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

            // Act
            var sut = PipelineExecutionHelpers.SetupExecution(
                // Assert    
                async docs => (await docs.FirstOrDefault().GetContentStringAsync()).Should().Contain("Coffee")
                );
            await sut.ExecuteAsync();
        }
    }
}
