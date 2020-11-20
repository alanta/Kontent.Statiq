using Kentico.Kontent.Delivery.Abstractions;
using Statiq.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kontent.Statiq
{
    internal class KontentTaxonomyHelpers
    {
        internal static async Task<IDocument> CreateDocument(IExecutionContext context, ITaxonomyGroup item)
        {
            var treePath = $"/{item.System.Codename}";

            var metadata = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>(Keys.Title, item.System.Name),
                new KeyValuePair<string, object>(Keys.GroupKey, item.System.Codename),
                new KeyValuePair<string, object>(Keys.TreePath, treePath),
                new KeyValuePair<string, object>(TypedContentExtensions.KontentTaxonomyGroupKey, item)
            };

            MapSystemMetadata(item, metadata);

            if (item.Terms != null)
            {
                var terms = await item.Terms.ParallelSelectAsync(async t => await CreateDocument(context, t, treePath));
                metadata.Add(new KeyValuePair<string, object>(Keys.Children, terms));
            }

            return await context.CreateDocumentAsync(metadata, "", "text/html");
        }

        internal static async Task<IDocument> CreateDocument(IExecutionContext context, ITaxonomyTermDetails item, string parentPath)
        {
            var treePath = $"{parentPath}/{item.Codename}";
            var metadata = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>(Keys.Title, item.Name),
                new KeyValuePair<string, object>(Keys.GroupKey, item.Codename),
                new KeyValuePair<string, object>(Keys.TreePath, treePath),
                new KeyValuePair<string, object>(KontentKeys.System.Name, item.Name),
                new KeyValuePair<string, object>(KontentKeys.System.CodeName, item.Codename),
            };

            if (item.Terms != null)
            {
                var terms = await item.Terms.ParallelSelectAsync(async t => await CreateDocument(context, t, treePath));
                metadata.Add(new KeyValuePair<string, object>(Keys.Children, terms));
            }

            return await context.CreateDocumentAsync(metadata, "", "text/html");
        }

        /// <summary>
        /// Map Kontent system properties directly into document metadata.
        /// </summary>
        /// <param name="item">The Kontent item.</param>
        /// <param name="props"></param>
        /// <param name="metadata"></param>
        private static void MapSystemMetadata(ITaxonomyGroup item, List<KeyValuePair<string, object>> metadata)
        {
            if( item.System != null )
            {
                metadata.AddRange(new[]
                {
                    new KeyValuePair<string, object>(KontentKeys.System.Name, item.System.Name),
                    new KeyValuePair<string, object>(KontentKeys.System.CodeName, item.System.Codename),
                    new KeyValuePair<string, object>(KontentKeys.System.Id, item.System.Id),
                    new KeyValuePair<string, object>(KontentKeys.System.LastModified, item.System.LastModified),
                });
            }
        }
    }
}