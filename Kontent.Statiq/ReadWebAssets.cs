using Microsoft.Extensions.Logging;
using Statiq.Common;
using Statiq.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Kontent.Statiq
{
    /// <summary>
    /// Downloads the given URIs and outputs the responses as new documents.
    /// </summary>
    /// <remarks>
    /// This is a narrowed stand-in for <c>Statiq.Core.ReadWeb</c>, which builds and disposes a fresh
    /// <see cref="HttpClientHandler"/> for every single request and never retries. Downloading a few
    /// hundred assets that way runs out of sockets, and the first 429 the Delivery API returns ends up
    /// written to disk as if it were image content.
    ///
    /// This module instead takes one client from <see cref="IExecutionState.CreateHttpClient()"/> per
    /// execution — that overload reuses the engine's shared message handler — and sends through
    /// <c>SendWithRetryAsync</c>, which retries 429s and transient socket failures with exponential
    /// back-off. <c>IExecutionState.SendHttpRequestWithRetryAsync</c> would do the same, but its
    /// <c>TestEngine</c> implementation skips the retry policy, so the behaviour could not be covered
    /// by tests.
    ///
    /// Only anonymous GETs are supported: request headers and credentials are out of reach because
    /// <c>WebRequestHeaders.ApplyTo</c> is internal to Statiq.Core. Use <c>Statiq.Core.ReadWeb</c> when
    /// you need those.
    /// </remarks>
    internal sealed class ReadWebAssets : Module
    {
        private readonly string[] _uris;

        public ReadWebAssets(params string[] uris)
        {
            _uris = uris.ThrowIfNull(nameof(uris));
        }

        /// <inheritdoc />
        protected override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            using var client = context.CreateHttpClient();
            var responses = await _uris.ParallelSelectAsync(
                uri => GetResponseAsync(uri, client, context), context.CancellationToken);

            return responses
                .OfType<WebResponse>()
                .Select(response => context.CreateDocument(
                    new MetadataItems
                    {
                        { Keys.SourceUri, response.Uri },
                        { Keys.SourceHeaders, response.Headers }
                    },
                    context.GetContentProvider(response.Data, response.MediaType)))
                .ToArray();
        }

        private static async Task<WebResponse?> GetResponseAsync(string uri, HttpClient client, IExecutionContext context)
        {
            try
            {
                using var response = await client.SendWithRetryAsync(
                    () => new HttpRequestMessage(HttpMethod.Get, uri), context.CancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    // Passing the response body on would write the error page to disk under the
                    // asset's name, so drop it and let the build fail on the logged error instead.
                    context.LogError("Failed to download {Uri} : {StatusCode} {StatusName}",
                        uri, (int)response.StatusCode, response.StatusCode);
                    return null;
                }

                using var content = response.Content;

                // The response is disposed on the way out of this method while the content provider is
                // only read later on, so the body has to be buffered here either way.
                var data = await content.ReadAsByteArrayAsync(context.CancellationToken);

                var headers = content.Headers.ToDictionary(
                    x => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(x.Key),
                    x => string.Join(",", x.Value));

                return new WebResponse(new Uri(uri), data, content.Headers.ContentType?.MediaType, headers);
            }
            catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
            {
                // The execution is being torn down, not a download failure.
                throw;
            }
            catch (Exception ex)
            {
                // A failure that outlived the retries is handled the same way as a non-success
                // status: drop the document and let the build fail on the logged error.
                context.LogError(ex, "Failed to download {Uri}", uri);
                return null;
            }
        }

        private sealed record WebResponse(Uri Uri, byte[] Data, string? MediaType, Dictionary<string, string> Headers);
    }
}
