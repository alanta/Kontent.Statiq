using AngleSharp.Extensions;
using Microsoft.Extensions.Logging;
using Statiq.Common;
using Statiq.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using IDocument = Statiq.Common.IDocument;

namespace Kontent.Statiq
{
    /// <summary>
    /// Parses HTML to find images used in the document. The urls are replaced with local urls and the
    /// asset download urls are added to the output document.
    /// </summary>
    public class KontentImageProcessor : Module
    {
        internal const string KontentAssetDownloadKey = "KONTENT-ASSET-DOWNLOADS";
        private NormalizedPath _localBasePath = "img";
        private Func<string, bool>? _urlFilter;

        /// <summary>
        /// Set the local path for downloaded images. Default is <em>/img</em>.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public KontentImageProcessor WithLocalPath(string path)
        {
            _localBasePath = path;
            return this;
        }

        /// <summary>
        /// Specify a filter. Return true if an item should be skipped. You can add only one filter.
        /// </summary>
        /// <param name="filter">The filter function. Return <c>true</c> to skip a url.</param>
        /// <returns>The module for further chaining.</returns>
        public KontentImageProcessor Skip(Func<string, bool> filter)
        {
            _urlFilter = filter;
            return this;
        }

        /// <inheritdoc/>
        protected override async Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context)
        {
            var html = await input.ParseHtmlAsync(context);

            var downloadUrls = new List<KontentImageDownload>();

            foreach (var image in html.Images)
            {
                var imageSource = image.Source;
                if (SkipImage(imageSource))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(image.SourceSet))
                {
                    var (sourceSet, downloads) = ProcessSourceSet(image.SourceSet, context);
                    image.SourceSet = sourceSet;
                    if (downloads.Any())
                    {
                        downloadUrls.AddRange(downloads);
                    }
                }

                var localPath = KontentAssetHelper.GetLocalFileName(imageSource, _localBasePath);

                context.LogDebug("Replacing image {0} => {1}", image.Source, localPath);

                // update the content
                image.Source = localPath.IsRelative ? "/" + localPath : localPath.ToString();

                // add the url for downloading
                downloadUrls.Add(new KontentImageDownload(imageSource, localPath));
            }

            return input.Clone(
                new[] { new KeyValuePair<string, object>(KontentAssetDownloadKey, downloadUrls.ToArray()) },
                await context.GetContentProviderAsync(
                    html.ToHtml(), // Note that AngleSharp always injects <html> and <body> tags so can't use this module with HTML fragments
                    MediaTypes.Html)).Yield();

        }

        private bool SkipImage(string uri)
        {
            return (!IsRemoteUrl(uri) || !(_urlFilter?.Invoke(uri) ?? true));
        }

        private (string, KontentImageDownload[]) ProcessSourceSet(string sourceSet, IExecutionContext context)
        {
            var downloads = new List<KontentImageDownload>();
            var newSourceSet = new List<string>();

            foreach (var match in _sourceSetRegex.Matches(sourceSet).Cast<Match>())
            {
                var url = match.Groups["url"].Value;
                var size = match.Groups["size"]?.Value ?? "";

                if (SkipImage(url))
                {
                    newSourceSet.Add(match.Value);
                }
                else
                {
                    var localPath = KontentAssetHelper.GetLocalFileName(url, _localBasePath);
                    context.LogDebug("Replacing srcset image {0} => {1}", url, localPath);
                    newSourceSet.Add($"{localPath} {size}".Trim());
                    downloads.Add(new KontentImageDownload(url, localPath));
                }
            }

            if (!downloads.Any())
            {
                // leave the source set untouched
                return (sourceSet, Array.Empty<KontentImageDownload>());
            }

            return (string.Join(",", newSourceSet), downloads.ToArray());
        }

        private static readonly Regex _sourceSetRegex = new Regex("(?:(?<url>.*)\\s+(?<size>[0-9]+[w|x]))", RegexOptions.IgnoreCase | RegexOptions.Multiline, TimeSpan.FromSeconds(5));
        private static readonly Regex _remoteUrlRegex = new Regex("^https?://", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(5));

        private static bool IsRemoteUrl(string? url)
        {
            return url != null && _remoteUrlRegex.IsMatch(url);
        }
    }
}
