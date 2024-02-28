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
    public class KontentDownloadImages : Module
    {
        readonly Dictionary<string, CachedImage> _cached = new(StringComparer.OrdinalIgnoreCase);

        internal record CachedImage(byte[] Data, string MediaType);

        /// <inheritdoc />
        protected override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            var assets = context.Inputs
                .SelectMany(doc => doc.GetKontentImageDownloads())
                .DistinctBy(a => a.LocalPath) // filter duplicates
                .ToArray();

            // optimize for re-rendering on preview - skip files already in cache
            var newAssets = assets.Where(asset => !_cached.ContainsKey(asset.LocalPath.ToString())).ToArray();
            var downloadsWithDestination = assets.Except(newAssets).Select(asset => context.CreateDocument(
                destination: asset.LocalPath.ToString().ToLower().TrimStart('/'),
                context.GetContentProvider(_cached[asset.LocalPath.ToString()].Data, _cached[asset.LocalPath.ToString()].MediaType)
            )).ToList();

            if( newAssets.Length == 0 )
            {
                context.LogInformation(null, $"Skipping image download because there are no new images.");
            }
            else if( newAssets.Length != assets.Length ) 
            {
                context.LogInformation(null, $"Downloading {newAssets.Length} files, skipping {assets.Length-newAssets.Length} already downloaded.");
            }

            var childModules = newAssets.Select(a => a.OriginalUrl).Chunk(20).Select( x => new ReadWeb(x.ToArray()) );


            var downloads = new List<IDocument>();

            // Workaround for unlimited concurrency in ReadWeb. By fetching chunks of 20 images we prevent timeouts 
            // caused by flooding the Kontent Delivery API with 100s of concurrent requests.
            foreach (var module in childModules)
            {
                var documents = await module!.ExecuteAsync(context);
                downloads.AddRange(documents);
            }

            foreach (var download in downloads)
            {
                var downloadedUrl = download.Get<string>(Keys.SourceUri);
                var asset = assets.FirstOrDefault(a => a.OriginalUrl == downloadedUrl);
                if (asset != null)
                {
                    _cached[asset.LocalPath.ToString()]=new CachedImage(Data: await download.GetContentBytesAsync(), MediaType: download.ContentProvider.MediaType);
                    downloadsWithDestination.Add(download.Clone(destination: asset.LocalPath.ToString().ToLower().TrimStart('/')));
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