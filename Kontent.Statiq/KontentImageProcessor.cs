using AngleSharp.Extensions;
using Kentico.Kontent.Delivery.Abstractions;
using Microsoft.Extensions.Logging;
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
    /// Parses HTML to find images used in the document. The urls are replaced with local urls and the
    /// asset download urls are added to the output document.
    /// </summary>
    public class KontentImageProcessor : Module
    {
        internal const string KontentAssetDownloadKey = "KONTENT-ASSET-DOWNLOADS";
        private NormalizedPath _localBasePath = "img";

        public KontentImageProcessor WithLocalPath(string path)
        {
            _localBasePath = path;
            return this;
        }
        
        /// <inheritdoc/>
        protected override async Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context)
        {
            var html = await input.ParseHtmlAsync(context);

            var downloadUrls = new List<KontentAssetDownload>();

            foreach (var image in html.Images)
            {
                // TODO : support source sets
                if (string.IsNullOrEmpty(image.Source))
                {
                    if (!string.IsNullOrWhiteSpace(image.SourceSet))
                    {
                        context.LogWarning("Images with source sets are not yet supported.");
                    }

                    continue;
                }

                var imageSource = image.Source;
                var localPath = KontentAssetHelper.GetLocalFileName(imageSource, _localBasePath);

                context.LogDebug("Found image {0} => {1}", image.Source, localPath);

                // update the content
                image.Source = localPath.IsRelative ? "/"+localPath : localPath.ToString();

                // add the url for downloading
                downloadUrls.Add(new KontentAssetDownload(imageSource, localPath));
            }

            return input.Clone( 
                new []{ new KeyValuePair<string, object>(KontentAssetDownloadKey, downloadUrls.ToArray()) },
                await context.GetContentProviderAsync(
                    html.ToHtml(), // Note that AngleSharp always injects <html> and <body> tags so can't use this module with HTML fragments
                    MediaTypes.Html)).Yield();
        }
    }
}
