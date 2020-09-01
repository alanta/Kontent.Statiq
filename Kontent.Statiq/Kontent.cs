using Kentico.Kontent.Delivery.Abstractions;
using Statiq.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Module = Statiq.Common.Module;

namespace Kontent.Statiq
{
    /// <summary>
    /// Retrieves content items from Kontent.
    /// </summary>
    public sealed class Kontent<TContentModel> : Module where TContentModel : class
    {
        /// <summary>
        /// The code name of the field uses to fill the main Content field on the Statiq document. This is mostly useful for untyped content.
        /// </summary>
        public Func<TContentModel, string>? GetContent { get; set; }

        private readonly Lazy<IDeliveryClient> _client;

        internal List<IQueryParameter> QueryParameters { get; } = new List<IQueryParameter>();

        /// <summary>
        /// Create a new instance of the Kontent module for Statiq using an existing Kontent client.
        /// </summary>
        /// <param name="client">The Kontent client to use.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="client"/> is null.</exception>
        public Kontent(IDeliveryClient client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client), $"{nameof(client)} must not be null");

            _client = new Lazy<IDeliveryClient>(() => client);
        }

        /// <inheritdoc />
        protected override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            var items = await _client.Value.GetItemsAsync<TContentModel>(QueryParameters);

            var documentTasks = items.Items.Select(item => CreateDocument(context, item)).ToArray();

            await Task.WhenAll(documentTasks);

            return documentTasks.Select(t => t.Result);
        }

        private async Task<IDocument> CreateDocument(IExecutionContext context, TContentModel item)
        {
            var props = typeof(TContentModel).GetProperties(BindingFlags.Instance | BindingFlags.FlattenHierarchy |
                                                            BindingFlags.GetProperty | BindingFlags.Public);
            var metadata = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>(TypedContentExtensions.KontentItemKey, item),
            };

            if (props.FirstOrDefault(prop => typeof(IContentItemSystemAttributes).IsAssignableFrom(prop.PropertyType))
                ?.GetValue(item) is IContentItemSystemAttributes systemProp)
            {
                metadata.AddRange(new[]
                {
                    new KeyValuePair<string, object>(KontentKeys.System.Name, systemProp.Name),
                    new KeyValuePair<string, object>(KontentKeys.System.CodeName, systemProp.Codename),
                    new KeyValuePair<string, object>(KontentKeys.System.Language, systemProp.Language),
                    new KeyValuePair<string, object>(KontentKeys.System.Id, systemProp.Id),
                    new KeyValuePair<string, object>(KontentKeys.System.Type, systemProp.Type),
                    new KeyValuePair<string, object>(KontentKeys.System.LastModified, systemProp.LastModified)
                });
            }

            var content = GetContent?.Invoke(item) ?? "";

            return await context.CreateDocumentAsync(metadata, content, "text/html");
        }

    }
}
