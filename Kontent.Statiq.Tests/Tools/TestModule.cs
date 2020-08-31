using Statiq.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kontent.Statiq.Tests.Tools
{
    internal class TestModule : Module
    {
        private readonly Func<IReadOnlyList<IDocument>, Task> _test;

        public TestModule(Func<IReadOnlyList<IDocument>, Task> test)
        {
            _test = test;
        }

        protected override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            await _test(context.Inputs);
            return context.Inputs;
        }
    }
}