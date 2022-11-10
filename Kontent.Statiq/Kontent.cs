using Kontent.Ai.Delivery.Abstractions;
using Statiq.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Module = Statiq.Common.Module;

namespace Kontent.Statiq
{
    /// <summary>
    /// Retrieves content items from Kontent.
    /// <para>Use <c>.WithContent()</c> to specify what property to load content from and <c>.WithQuery()</c> to specify query parameters.</para>
    /// </summary>
    /// <typeparam name="TContentModel">The content model.</typeparam>
    public sealed class Kontent<TContentModel> : Module where TContentModel : class
    {
        internal Func<TContentModel, string>? GetContent { get; set; }

        private readonly IDeliveryClient _client;
        private bool _useItemFeed;

        internal List<IQueryParameter> QueryParameters { get; } = new List<IQueryParameter>();

        /// <summary>
        /// Create a new instance of the Kontent module for Statiq using an existing Kontent client.
        /// </summary>
        /// <param name="client">The Kontent client to use.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="client"/> is null.</exception>
        public Kontent(IDeliveryClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client), $"{nameof(client)} must not be null");
            _useItemFeed = false;
        }

        /// <summary>
        /// Use the items-feed endpoint instead of the items endpoint.
        /// <para>Use this when you have a lot of content and get a bad request response.</para>
        /// </summary>
        /// <returns>The module.</returns>
        public Kontent<TContentModel> WithItemsFeed()
        {
            _useItemFeed = true;
            return this;
        }
        
        /// <inheritdoc />
        protected override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            if (_useItemFeed)
            {
                var feed = _client.GetItemsFeed<TContentModel>(QueryParameters);

                var documents = new List<IDocument>();
                
                while (feed.HasMoreResults)
                {
                    var nextBatch = await feed.FetchNextBatchAsync();
                    if (!nextBatch.ApiResponse.IsSuccess)
                    {
                        throw new InvalidOperationException($"Failed to load data from Kontent item feed for content type {typeof(TContentModel).Name} : {nextBatch.ApiResponse.Error.Message}");
                    }

                    var outputDocuments = nextBatch.Items.Select(item => KontentDocumentHelpers.CreateDocument(context, item, GetContent)).ToArray();
                    
                    documents.AddRange(outputDocuments);
                }

                return documents;
            }
            else
            {
                var items = await _client.GetItemsAsync<TContentModel>(QueryParameters);
                if (!items.ApiResponse.IsSuccess)
                {
                    throw new InvalidOperationException($"Failed to load data from Kontent for content type {typeof(TContentModel).Name} : {items.ApiResponse.Error.Message}");
                }

                if (items.Items == null || items.Items.Count == 0)
                {
                    context.Logger.LogWarning($"Query for {typeof(TContentModel).Name} returned no results.");
                }

                return items.Items?.Select(item => KontentDocumentHelpers.CreateDocument(context, item, GetContent)).ToArray() ?? Array.Empty<IDocument>();
            }
        }
    }
}
