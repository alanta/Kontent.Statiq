using FakeItEasy;
using FluentAssertions;
using Statiq.Common;
using Statiq.Core;
using Statiq.Razor;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Kontent.Statiq.Tests
{
    public class When_rendering_a_Razor_view
    {
        private readonly ITestOutputHelper _testOutput;

        public When_rendering_a_Razor_view(ITestOutputHelper testOutput)
        {
            _testOutput = testOutput;
        }

        [Fact]
        public async Task It_should_pickup_Layout_and_view_start()
        {
            // Arrange 

            // Act
            var engine = new Engine();

            var pipeline = new Pipeline()
            {
                InputModules =
                {
                    // Load the content
                    PipelineExecutionHelpers.SetupKontentModule(),
                    // Put the Razor view in the body
                    new MergeContent(modules: new ReadFiles(patterns: Path.GetFullPath(".\\input\\Article.cshtml"))),
                    // Render the view
                    new RenderRazor()
                        .WithModel( Config.FromDocument( (document, context) => document.AsKontent<Models.Article>() ) ),
                    new TestModule(async documents => (await documents.First().GetContentStringAsync()).Should().Contain("LAYOUT"))
                }
            };

            engine.Pipelines.Add("test", pipeline);

            await engine.ExecuteAsync();
        }
    }
}
