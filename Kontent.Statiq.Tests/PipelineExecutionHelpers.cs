using Kontent.Statiq.Tests.Models;
using Kontent.Statiq.Tests.Tools;
using Statiq.Common;
using Statiq.Core;
using System;
using System.Collections.Generic;
using System.IO;

namespace Kontent.Statiq.Tests
{
    internal static class PipelineExecutionHelpers
    {
        public static Engine SetupExecution(Action<IReadOnlyList<IDocument>> test)
        {
            var engine = new Engine();
            var pipeline = new Pipeline()
            {
                InputModules = {SetupKontentModule(), new TestModule(test)}
            };

            engine.Pipelines.Add("test", pipeline);
            return engine;
        }


        internal static Statiq.Kontent SetupKontentModule()
        {
            var responseJsonPath = Path.Combine(Environment.CurrentDirectory, $"response{Path.DirectorySeparatorChar}getitems.json");
            var responseJson = File.ReadAllText(responseJsonPath);
            return new Kontent(MockDeliveryClient.Create(responseJson, cfg => cfg.WithTypeProvider(new CustomTypeProvider())))
                .WithContentField(Article.BodyCopyCodename);
        }
    }
}