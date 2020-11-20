using Kentico.Kontent.Delivery.Abstractions;
using Statiq.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kontent.Statiq
{
    public sealed class KontentTaxonomy : Module
    {
        private readonly IDeliveryClient _client;

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

        /// <inheritdoc />
        protected override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            var items = await _client.GetTaxonomiesAsync(QueryParameters);

            var documentTasks = items.Taxonomies.Select(item => KontentTaxonomyHelpers.CreateDocument(context, item)).ToArray();

            return await Task.WhenAll(documentTasks);
        }
    }
}