using Kentico.Kontent.Delivery.Abstractions;
using Statiq.Common;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace Kontent.Statiq
{
    internal static class KontentTaxonomyHelpers
    {
        internal static async Task<IEnumerable<IDocument>> CreateDocument(IExecutionContext context, ITaxonomyGroup item, bool collapseRoot)
        {
            string[] treePath = collapseRoot ? Array.Empty<string>() : new[] {item.System.Codename};
            IEnumerable<IDocument>? terms = Array.Empty<IDocument>();
            
            if (item.Terms != null && item.Terms.Count > 0)
            {
                terms = await item.Terms.ParallelSelectAsync(async t => await CreateDocument(context, t, treePath));
            }

            if (collapseRoot)
            {
                return terms;
            }
            
            var metadata = BuildRootNode(item, treePath, terms);
            var root = await context.CreateDocumentAsync(metadata, "", "text/html");
            return root.Yield();
        }

        private static IList<KeyValuePair<string, object>> BuildRootNode(ITaxonomyGroup item, string[] treePath, IEnumerable<IDocument> terms)
        {
            var metadata = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>(Keys.Title, item.System.Name),
                new KeyValuePair<string, object>(Keys.GroupKey, item.System.Codename),
                new KeyValuePair<string, object>(Keys.TreePath, treePath),
                new KeyValuePair<string, object>(KontentKeys.Taxonomy.Group, item),
            };
            
            if (item.Terms != null && item.Terms.Count > 0)
            {
                metadata.AddRange( 
                    new KeyValuePair<string, object>(Keys.Children, terms),
                    new KeyValuePair<string, object>(KontentKeys.Taxonomy.Terms, item.Terms?.ToArray() ?? Array.Empty<ITaxonomyTermDetails>())
                );
            }

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

            return metadata;
        }

        private static async Task<IDocument> CreateDocument(IExecutionContext context, ITaxonomyTermDetails item, string[] parentPath)
        {
            var treePath = parentPath.Concat(item.Codename).ToArray();
            var metadata = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>(Keys.Title, item.Name),
                new KeyValuePair<string, object>(Keys.GroupKey, item.Codename),
                new KeyValuePair<string, object>(Keys.TreePath, treePath),
                new KeyValuePair<string, object>(KontentKeys.System.Name, item.Name),
                new KeyValuePair<string, object>(KontentKeys.System.CodeName, item.Codename),
                new KeyValuePair<string, object>(KontentKeys.Taxonomy.Term, item)
            };

            if (item.Terms != null && item.Terms.Count > 0)
            {
                var terms = await item.Terms.ParallelSelectAsync(async t => await CreateDocument(context, t, treePath));
                metadata.Add(new KeyValuePair<string, object>(Keys.Children, terms));
                metadata.Add(new KeyValuePair<string, object>(KontentKeys.Taxonomy.Terms, item.Terms.ToArray()));
            }

            return await context.CreateDocumentAsync(metadata, "", "text/html");
        }
    }
}