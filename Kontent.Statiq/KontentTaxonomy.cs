using Kentico.Kontent.Delivery.Abstractions;
using Statiq.Common;
using Statiq.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kontent.Statiq
{
    /// <summary>
    /// Module that retrieves Kontent taxonomy groups as a Statiq document structure.
    /// <para>The output is a page structure for each group that matches the query. Child documents are child terms.</para>
    /// </summary>
    public sealed class KontentTaxonomy : Module
    {
        private readonly IDeliveryClient _client;
        private bool _nesting = false;
        private bool _collapseRoot = true;

        internal List<IQueryParameter> QueryParameters { get; } = new List<IQueryParameter>();

        /// <summary>
        /// Create a new instance of the KontentTaxonomy module for Statiq using an existing Kontent client.
        /// </summary>
        /// <param name="client">The Kontent client to use.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="client"/> is null.</exception>
        public KontentTaxonomy(IDeliveryClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client), $"{nameof(client)} must not be null");
        }

        /// <summary>
        /// Indicates that the module should only output root nodes (instead of all
        /// nodes which is the default behavior).
        /// </summary>
        /// <param name="nesting"><c>true</c> to enable nesting and only output the root nodes.</param>
        /// <param name="collapseRoot">
        /// Indicates that the root of the tree should be collapsed and the module
        /// should output first-level documents as if they were root documents. This setting
        /// has no effect if not nesting.
        /// </param>
        /// <returns>The current module instance.</returns>
        public KontentTaxonomy WithNesting(bool nesting = true, bool collapseRoot = false)
        {
            _nesting = nesting;
            _collapseRoot = collapseRoot;
            return this;
        }
        
        /// <inheritdoc />
        protected override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            var items = await _client.GetTaxonomiesAsync(QueryParameters);

            var documentTasks = items.Taxonomies.Select(item => KontentTaxonomyHelpers.CreateDocument(context, item, _collapseRoot)).ToArray();

            var results = await Task.WhenAll(documentTasks);
            var documents = results.SelectMany(d => d).ToArray();

            if (!_nesting)
            {
                // flatten the hierarchy
                documents = documents.Flatten(Keys.TreePlaceholder, Keys.Children).ToArray();
            }

            return documents;
        }
    }
}