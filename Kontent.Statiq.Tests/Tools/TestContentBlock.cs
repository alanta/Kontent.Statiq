using Kontent.Ai.Delivery.Abstractions;

namespace Kontent.Statiq.Tests.Tools
{
    public class TestContentBlock : IRichTextBlock
    {
        private readonly string _content;

        public TestContentBlock( string content )
        {
            _content = content;
        }

        public override string ToString()
        {
            return _content;
        }
    }
}