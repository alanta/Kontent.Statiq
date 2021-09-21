using FakeItEasy;
using FluentAssertions;
using Kentico.Kontent.Delivery.Abstractions;
using Kontent.Statiq.Tests.Models;
using Kontent.Statiq.Tests.Tools;
using Microsoft.Extensions.DependencyInjection;
using Statiq.Common;
using Statiq.Core;
using Statiq.Razor;
using Statiq.Testing;
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
            var mockClient = A.Fake<IDeliveryClient>().WithFakeContent(new[] {new Article{ Title = "Test"}});

            
            var context = GetExecutionContext();

            context.FileSystem.FileProvider = new TestFileProvider
            {
                {
                    "/input/Article.cshtml", @"@model Kontent.Statiq.Tests.Models.Article

<h3>@Model.Title</h3>

@Model.BodyCopy"
                },
                {
                    "/input/_Layout.cshtml", @"@model Kontent.Statiq.Tests.Models.Article
LAYOUT!

@RenderBody()"
                },
                {
                    "/input/_ViewStart.cshtml", @"@model Kontent.Statiq.Tests.Models.Article
@{
    Layout = ""_Layout"";
                }"
                }
            };

            var modules = new IModule[]
            {
                // Load the content
                new Kontent<Article>(mockClient),
                // Put the Razor view in the body
                new MergeContent(modules: new ReadFiles(patterns: "/input/Article.cshtml")),
                // Render the view
                new RenderRazor()
                    .WithModel(KontentConfig.As<Article>()),
            };

            // Act
            var docs = await context.ExecuteModulesAsync(modules);

            // Assert
            var content = await docs.First().GetContentStringAsync();
            content.Should().Contain("LAYOUT!", "It should pickup the layout via viewstart");
            content.Should().Contain("<h3>Test</h3>", "It should pickup data from the model");
        }

        private TestExecutionContext GetExecutionContext()
        {
            // see https://github.com/statiqdev/Statiq.Framework/blob/main/tests/extensions/Statiq.Razor.Tests/RenderRazorFixture.cs
            TestExecutionContext context = new TestExecutionContext()
            {
               FileSystem = GetFileSystem(),
            };
            context.Services.AddSingleton<IReadOnlyFileSystem>(context.FileSystem);
            context.Services.AddSingleton<INamespacesCollection>(context.Namespaces);
            context.Services.AddRazor();
            new RazorEngineInitializer().Initialize(context.Engine);
            return context;
        }

        private TestFileSystem GetFileSystem() => new TestFileSystem()
        {
            RootPath = NormalizedPath.AbsoluteRoot,
            InputPaths = new PathCollection("input")
        };
    }
}
