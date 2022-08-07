using Kontent.Ai.Delivery.Abstractions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Kontent.Statiq.Tests.Tools
{
    internal class TestRichTextContent : IRichTextContent
    {
        private readonly IList<IRichTextBlock> _blocks;

        public TestRichTextContent(params IRichTextBlock[] blocks)
        {
            _blocks = new List<IRichTextBlock>(blocks);
        }

        public void Add(string content)
        {
            _blocks.Add(new TestContentBlock(content));
        }

        public IEnumerator<IRichTextBlock> GetEnumerator()
        {
            return ((IEnumerable<IRichTextBlock>)_blocks).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerable<IRichTextBlock> Blocks => _blocks;

        public override string ToString()
        {
            return string.Join("", _blocks.Select(b => b.ToString()));
        }
    }
}