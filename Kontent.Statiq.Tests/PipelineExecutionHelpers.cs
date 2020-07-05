using Kontent.Statiq.Tests.Models;
using Kontent.Statiq.Tests.Tools;
using Statiq.Common;
using Statiq.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Kontent.Statiq.Tests
{
    internal static class PipelineExecutionHelpers
    {
        public static Engine SetupExecution(Func<IReadOnlyList<IDocument>, Task> test)
        {
            var engine = new Engine();
            var pipeline = new Pipeline()
            {
                InputModules = { SetupKontentModule(), new TestModule(test) }
            };

            engine.Pipelines.Add("test", pipeline);
            return engine;
        }


        internal static Statiq.Kontent<Article> SetupKontentModule()
        {
            var responseJsonPath = Path.Combine(Environment.CurrentDirectory, $"response{Path.DirectorySeparatorChar}getitems.json");
            var responseJson = File.ReadAllText(responseJsonPath);
            return new Kontent<Article>(MockDeliveryClient.Create(responseJson, cfg => cfg.WithTypeProvider(new CustomTypeProvider())))
                .WithContent(item => item.Title); // TODO : see if we can map rich content into a flat string
        }
    }
}