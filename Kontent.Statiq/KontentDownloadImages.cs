using Statiq.Common;
using Statiq.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kontent.Statiq
{
    /// <summary>
    /// Downloads all images found in input documents processed with <see cref="KontentImageProcessor"/>
    /// The downloaded assets can then be processed with modules such as <see cref="WriteFiles"/>.
    /// </summary>
    public class KontentDownloadImages : ReadWeb
    {
        /// <inheritdoc />
        protected override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            var assets = context.Inputs
                .SelectMany(doc => doc.GetKontentImageDownloads())
                .ToArray();

            var assetUrls = assets.Select(a => a.OriginalUrl).ToArray(); // TODO: distinct by local path

            WithUris(assetUrls.ToArray());

            var downloads =  await base.ExecuteContextAsync(context);
            var downloadsWithDestination = new List<IDocument>();

            foreach (var download in downloads)
            {
                var downloadedUrl = download.Get<string>(Keys.SourceUri);
                var asset = assets.FirstOrDefault(a => a.OriginalUrl == downloadedUrl);
                if (asset != null)
                {
                    downloadsWithDestination.Add(download.Clone(asset.LocalPath.ToString().TrimStart('/')));
                }
                else
                {
                    throw new InvalidOperationException($"No asset found for url {downloadedUrl}");
                }
            }

            return downloadsWithDestination;
        }

    }
}