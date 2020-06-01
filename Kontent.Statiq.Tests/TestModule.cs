using Statiq.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kontent.Statiq.Tests
{
    internal class TestModule : Module
    {
        private readonly Action<IReadOnlyList<IDocument>> _test;

        public TestModule(Action<IReadOnlyList<IDocument>> test)
        {
            _test = test;
        }

        protected override Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            _test(context.Inputs);
            return Task.FromResult((IEnumerable<IDocument>)context.Inputs);
        }
    }
}