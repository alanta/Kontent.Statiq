using FakeItEasy;
using FluentAssertions;
using Kentico.Kontent.Delivery.Abstractions;
using Kontent.Statiq.Tests.Models;
using Kontent.Statiq.Tests.Tools;
using Statiq.Common;
using Statiq.Core;
using Statiq.Razor;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Kontent.Statiq.Tests
{
    public class When_rendering_a_Razor_view
    {
        [Fact]
        public async Task It_should_pickup_Layout_and_view_start()
        {
            // Arrange 
            var mockClient = A.Fake<IDeliveryClient>().WithFakeContent(new[] {new Article()});

            // Act
            var engine = new Engine();

            var pipeline = new Pipeline()
            {
                InputModules =
                {
                    // Load the content
                    //PipelineExecutionHelpers.SetupKontentModule(),
                    new Kontent<Article>( mockClient ),

                    // Put the Razor view in the body
                    new MergeContent(modules: new ReadFiles(patterns: Path.GetFullPath(".\\input\\Article.cshtml"))),
                    // Render the view
                    new RenderRazor()
                        .WithModel( KontentConfig.As<Article>() ),
                    new TestModule(async documents => (await documents.First().GetContentStringAsync()).Should().Contain("LAYOUT"))
                }
            };

            engine.Pipelines.Add("test", pipeline);

            await engine.ExecuteAsync();
        }
    }
}
