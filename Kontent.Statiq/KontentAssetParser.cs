using AngleSharp.Dom;
using Kontent.Statiq.Models;
using Statiq.Common;
using Statiq.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IDocument = Statiq.Common.IDocument;

namespace Kontent.Statiq
{
    /// <summary>
    /// Parses document content by replacing <c>!!assets/</c> paths with Kentico Cloud asset URLs.
    /// URLs are matched by the file name of the asset.
    /// </summary>
    public class KontentAssetParser : Module
    {
        protected override async Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context)
        {
            var content = await input.GetContentStringAsync();

            var assets = input.Where(x => x.Value is List<Asset>).ToList();
            if (assets.Any())
            {
                foreach (var metaAsset in assets)
                {
                    var asset = (List<Asset>) metaAsset.Value;

                    foreach (var image in asset)
                    {
                        content = content.Replace($"!!assets/{image.Name}", image.Url);
                    }
                }

                return input.Clone(await context.GetContentProviderAsync(content, MediaTypes.Html)).Yield();
            }

            return input.Yield();
        }

    }

}
