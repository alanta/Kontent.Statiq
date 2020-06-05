using Kontent.Statiq.Models;
using Statiq.Common;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kontent.Statiq
{
    /// <summary>
    /// Parses document content by replacing <c>!!local-assets/</c> paths with URLs to downloaded assets.
    /// URLs are matched by the file name of the asset.
    /// </summary>
    public class KontentLocalAssetParser : Module
    {
        private string _folderPath = string.Empty;

        /// <summary>
        /// Set the base folder for local assets.
        /// </summary>
        /// <param name="folderPath">The path to the local asset folder.</param>
        /// <returns>The module.</returns>
        public KontentLocalAssetParser WithFolderPath(string folderPath)
        {
            _folderPath = folderPath + "/";
            return this;
        }

        /// <inheritdoc/>
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
                        content = content.Replace($"!!local-assets/{image.Name}",
                            $"/{_folderPath}{KontentAssetHelper.GetAssetFileName(image.Url)}");
                    }
                }

                return input.Clone(await context.GetContentProviderAsync(content, MediaTypes.Html)).Yield();
            }

            return input.Yield();
        }
    }
}
