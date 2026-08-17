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
        // Not resettable: the whole point of this cache is to survive between executions so that
        // re-rendering in preview mode doesn't download every image again.
        readonly ConcurrentCache<string, CachedImage> _cached = new(false, StringComparer.OrdinalIgnoreCase);

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

            var childModules = newAssets.Select(a => a.OriginalUrl).Chunk(20).Select( x => new ReadWebAssets(x.ToArray()) );


            var downloads = new List<IDocument>();

            // ReadWebAssets retries throttled requests, but it still fetches its whole batch in parallel.
            // Fetching chunks of 20 keeps us from flooding the Kontent Delivery API with 100s of
            // concurrent requests and backing off on all of them at once.
            foreach (var module in childModules)
            {
                var documents = await module.ExecuteAsync(context);
                downloads.AddRange(documents);
            }

            foreach (var download in downloads)
            {
                var downloadedUrl = download.Get<string>(Keys.SourceUri);
                var asset = Array.Find(assets, a => a.OriginalUrl == downloadedUrl);
                if (asset != null)
                {
                    var data = new CachedImage(Data: await download.GetContentBytesAsync(),
                        MediaType: download.ContentProvider.MediaType);
                    _cached.AddOrUpdate(asset.LocalPath.ToString(), data, (_, _) => data);
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