using Kentico.Kontent.Delivery.Abstractions;
using Statiq.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        internal List<IQueryParameter> QueryParameters { get; } = new List<IQueryParameter>();

        /// <summary>
        /// Create a new instance of the Kontent module for Statiq using an existing Kontent client.
        /// </summary>
        /// <param name="client">The Kontent client to use.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="client"/> is null.</exception>
        public Kontent(IDeliveryClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client), $"{nameof(client)} must not be null");
        }

        /// <inheritdoc />
        protected override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            var items = await _client.GetItemsAsync<TContentModel>(QueryParameters);

            var documentTasks = items.Items.Select(item => KontentDocumentHelpers.CreateDocument(context, item, GetContent)).ToArray();

            return await Task.WhenAll(documentTasks);
        }
    }
}
