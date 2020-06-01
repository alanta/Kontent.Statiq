using Statiq.Common;
using Statiq.Core;
using System.Collections.Generic;

namespace Kontent.Statiq
{
    /// <summary>
    /// Downloads all Kontent assets found in input documents.
    /// The downloaded assets can then be processed with modules such as <see cref="WriteFiles"/>.
    /// URLs are supplied with the <see cref="WithUris"/> method.
    /// </summary>
    /*public class KontentAssetDownload : Download, IModule
    {
        private DocumentConfig UriDocConfig;

        public KontentAssetDownload WithUris(DocumentConfig uris)
        {
            UriDocConfig = uris;
            return this;
        }

        public new IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            var uris = new List<string>();
            foreach (var input in inputs)
            {
                uris.AddRange((string[])UriDocConfig(input, context));
            }

            WithUris(uris.ToArray());

            return base.Execute(inputs, context);
        }
    }*/
}
