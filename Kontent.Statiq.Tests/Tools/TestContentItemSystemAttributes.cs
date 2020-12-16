using Kentico.Kontent.Delivery.Abstractions;
using System;
using System.Collections.Generic;

namespace Kontent.Statiq.Tests.Tools
{
    /// <summary>
    /// Implements the IContentItemSystemAttributes interface for testing.
    /// </summary>
    internal sealed class TestContentItemSystemAttributes : IContentItemSystemAttributes
    {
        public string Id { get; internal set; }
        public string Name { get; internal set; }
        public string Codename { get; internal set; }
        public string Type { get; internal set; }
        public string Collection { get; }
        public IList<string> SitemapLocation { get; internal set; }
        public DateTime LastModified { get; internal set; }
        public string Language { get; internal set; }
    }
}