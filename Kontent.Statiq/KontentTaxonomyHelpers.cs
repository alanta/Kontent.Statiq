using Kentico.Kontent.Delivery.Abstractions;
using Statiq.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kontent.Statiq
{
    internal static class KontentTaxonomyHelpers
    {
        internal static async Task<IDocument> CreateDocument(IExecutionContext context, ITaxonomyGroup item)
        {
            var treePath = new []{item.System.Codename};

            var metadata = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>(Keys.Title, item.System.Name),
                new KeyValuePair<string, object>(Keys.GroupKey, item.System.Codename),
                new KeyValuePair<string, object>(Keys.TreePath, treePath),
                new KeyValuePair<string, object>(TypedContentExtensions.KontentTaxonomyGroupKey, item)
            };

            MapSystemMetadata(item, metadata);

            if (item.Terms != null && item.Terms.Count > 0)
            {
                var terms = await item.Terms.ParallelSelectAsync(async t => await CreateDocument(context, t, treePath));
                metadata.Add(new KeyValuePair<string, object>(Keys.Children, terms));
                metadata.Add(new KeyValuePair<string, object>(TypedContentExtensions.KontentTaxonomyTermsKey, item.Terms.Select(TaxonomyTerm.CreateFrom).ToArray()));
            }

            return await context.CreateDocumentAsync(metadata, "", "text/html");
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
                new KeyValuePair<string, object>(TypedContentExtensions.KontentTaxonomyTermKey, TaxonomyTerm.CreateFrom(item))
            };

            if (item.Terms != null && item.Terms.Count > 0)
            {
                var terms = await item.Terms.ParallelSelectAsync(async t => await CreateDocument(context, t, treePath));
                metadata.Add(new KeyValuePair<string, object>(Keys.Children, terms));
                metadata.Add(new KeyValuePair<string, object>(TypedContentExtensions.KontentTaxonomyTermsKey, item.Terms.Select( TaxonomyTerm.CreateFrom ).ToArray()));
            }

            return await context.CreateDocumentAsync(metadata, "", "text/html");
        }

        /// <summary>
        /// Map Kontent system properties directly into document metadata.
        /// </summary>
        /// <param name="item">The taxonomy group.</param>
        /// <param name="metadata">The metadata collection.</param>
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

    internal class TaxonomyTerm : ITaxonomyTerm
    {
        private TaxonomyTerm(string codename, string name)
        {
            Codename = codename;
            Name = name;
        }

        public string Codename { get; }
        public string Name { get; }

        public static ITaxonomyTerm CreateFrom(ITaxonomyTermDetails details)
        {
            return new TaxonomyTerm(details.Codename, details.Name);
        }
    }
}