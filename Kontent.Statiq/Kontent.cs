using Kentico.Kontent.Delivery;
using Kentico.Kontent.Delivery.Abstractions;
using Kentico.Kontent.Delivery.Builders.DeliveryClient;
using Microsoft.Extensions.Logging;
using Statiq.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kontent.Statiq
{
    /// <summary>
    /// Retrieves content items from Kontent.
    /// </summary>
    public sealed class Kontent : Module
    {
        /// <summary>
        /// The preview API key to use. Set this if you want to use preview (unpublished) content./>.
        /// </summary>
        public string PreviewApiKey { get; set; }
        
        /// <summary>
        /// The production API key to use. Set either this key if you don't want preview content and have secured your API (paid subscribtion required)./>.
        /// </summary>
        public string ProductionApiKey { get; set; }
        
        /// <summary>
        /// The Kontent project id. This is required.
        /// </summary>
        public string ProjectId { get; }
        
        /// <summary>
        /// The code name of the field uses to fill the main Content field on the Statiq document. This is mostly useful for untyped content.
        /// </summary>
        public string ContentField { get; set; }

        private readonly Lazy<IDeliveryClient> _client;
        
        internal List<Action<IOptionalClientSetup>> ConfigureClientActions = new List<Action<IOptionalClientSetup>>();
        internal List<IQueryParameter> QueryParameters { get; } = new List<IQueryParameter>();

        /// <summary>
        /// Create a new instance of the Kontent module for Statiq using an existing Kontent client.
        /// </summary>
        /// <param name="client">The Kontent client to use.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="client"/> is null.</exception>
        public Kontent(IDeliveryClient client)
        {
            if( client == null )
                throw new ArgumentNullException(nameof(client), $"{nameof(client)} must not be null");

            _client = new Lazy<IDeliveryClient>(() => client);
        }

        /// <summary>
        /// Create a new instance of the Kontent module for Statiq.
        /// </summary>
        /// <param name="projectId">Kontent project ID</param>
        /// <param name="previewApiKey">The preview API key (optional)</param>
        public Kontent(string projectId, string previewApiKey = null)
        {
            if (string.IsNullOrWhiteSpace(projectId))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(projectId));
            ProjectId = projectId;
            PreviewApiKey = previewApiKey;
            _client = new Lazy<IDeliveryClient>(CreateClient);
        }

        private IDeliveryClient CreateClient()
        {
            var builder = DeliveryClientBuilder
                .WithOptions(options =>
                {
                    var opt2 = options.WithProjectId(ProjectId);

                    if (!string.IsNullOrWhiteSpace(PreviewApiKey))
                    {
                        return opt2.UsePreviewApi(PreviewApiKey).Build();
                    }

                    if (!string.IsNullOrEmpty(ProductionApiKey))
                    {
                        return opt2.UseProductionApi(ProductionApiKey).Build();
                    }

                    return opt2.UseProductionApi().Build();

                });

            foreach (var action in ConfigureClientActions)
            {
                action(builder);
            }

            return builder.Build();
        }

        /// <inheritdoc />
        protected override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            var items = await _client.Value.GetItemsAsync(QueryParameters);

            var documentTasks = items.Items.Select(item => CreateDocument(context, item)).ToArray();

            await Task.WhenAll( documentTasks );

            return documentTasks.Select(t => t.Result);
        }

        private async Task<IDocument> CreateDocument(IExecutionContext context, ContentItem item)
        {
            var metadata = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>(TypedContentExtensions.KontentItemKey, item),
                new KeyValuePair<string, object>("name", item.System.Name),
                new KeyValuePair<string, object>("codename", item.System.Codename),
                new KeyValuePair<string, object>("language", item.System.Language),
                new KeyValuePair<string, object>("id", item.System.Id),
                new KeyValuePair<string, object>("type", item.System.Type)
            };

            foreach (var element in item.Elements)
            {
                string type = element.Value.type;

                switch (type)
                {
                    case "asset":
                        metadata.Add(new KeyValuePair<string, object>(element.Name, item.GetAssets(element.Name)));
                        break;
                    case "date_time":
                        metadata.Add(new KeyValuePair<string, object>(element.Name, item.GetDateTime(element.Name)));
                        break;
                    case "number":
                        metadata.Add(new KeyValuePair<string, object>(element.Name, item.GetNumber(element.Name)));
                        break;
                    case "multiple_choice":
                        metadata.Add(new KeyValuePair<string, object>(element.Name, item.GetOptions(element.Name)));
                        break;
                    case "modular_content":
                        metadata.Add(new KeyValuePair<string, object>(element.Name, item.GetLinkedItems(element.Name)));
                        break;
                    case "taxonomy":
                        metadata.Add(new KeyValuePair<string, object>(element.Name, item.GetTaxonomyTerms(element.Name)));
                        break;
                    case "url_slug":
                    case "text":
                    case "rich_text":
                        metadata.Add( new KeyValuePair<string, object>(element.Name, item.GetString(element.Name)));
                        break;
                    default:
                        context.LogDebug($"Unknown type '{type}' for content element '{element.Name}'" );
                        break;
                }
            }

            var content = string.IsNullOrWhiteSpace(ContentField) ? "" : item.GetString(ContentField);
            
            return await context.CreateDocumentAsync( metadata, content, "text/html" );
        }

    }
}
