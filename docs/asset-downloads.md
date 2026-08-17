# Why asset downloads don't use `Statiq.Core.ReadWeb`

`KontentDownloadImages` fetches assets through an internal module, [`ReadWebAssets`](../Kontent.Statiq/ReadWebAssets.cs),
rather than Statiq's own `ReadWeb`. This note records why, so the next person to read that code
doesn't "simplify" it back.

## The problem

A content-heavy site references hundreds of Kontent.ai assets. Downloading them through `ReadWeb`
was unreliable: builds intermittently failed, and worse, sometimes *succeeded* while writing broken
images.

Two independent causes, both in `Statiq.Core.ReadWeb`:

**A fresh handler per request.** `ReadWeb.GetResponseAsync` does this for every single URL:

```csharp
using HttpClientHandler clientHandler = new HttpClientHandler();
using HttpClient client = context.CreateHttpClient(clientHandler);
```

Each request gets its own connection pool, and disposal leaves the socket in `TIME_WAIT`. A few
hundred assets exhausts the ephemeral port range. Note that the *parameterless*
`IExecutionState.CreateHttpClient()` overload does the right thing already — it hands out a client
over the engine's shared message handler with `disposeHandler: false`. `ReadWeb` opts out of it only
so it can attach `request.Credentials` to a per-request handler.

**No retry, and no status check.** `ReadWeb` never inspects `IsSuccessStatusCode`. When the Delivery
API throttles and returns `429`, the response body — an error page — is handed on as document content
and written to disk under the asset's filename. You get a `.jpg` containing HTML, and a green build.

## What `ReadWebAssets` does instead

Statiq already ships the machinery for both halves; `ReadWeb` just doesn't use it.

- One client per module execution from `context.CreateHttpClient()` — the shared-handler overload.
- Requests go through `Statiq.Core.HttpClientExtensions.SendWithRetryAsync`, a Polly policy: 5
  attempts, exponential back-off, triggered by `429`, `HttpRequestException`, and `TaskCanceledException`
  wrapping an `IOException`/`SocketException`.
- A non-2xx response that survives the retries is logged as an error and the document is **dropped**,
  so a failed download can never be written out under the asset's name.
- `context.CancellationToken` is threaded through the parallel select, the send, and the body read.

`KontentDownloadImages` still chunks URLs into batches of 20 and awaits each batch. Retry limits the
damage from throttling but does nothing to limit concurrency, and backing off on 300 simultaneous
requests is far worse than not making them. The two mechanisms are complementary.

## Why it isn't a general-purpose `ReadWeb` replacement

`ReadWebAssets` only does anonymous `GET`s. It is `internal` and should stay that way.

`WebRequestHeaders.ApplyTo(HttpRequestHeaders)` — the only way to apply a `WebRequestHeaders` to a
request — is `internal` to `Statiq.Core`. **No out-of-tree copy of `ReadWeb` can support request
headers or credentials.** Anything needing those must keep using `Statiq.Core.ReadWeb`.

That constraint is also the argument for fixing this upstream rather than growing this module: the
general-purpose version can only live in `Statiq.Core`. If you pick that up, the change there is
small — use the parameterless `CreateHttpClient()` unless `Credentials` is set, send via
`SendWithRetryAsync`, and pass the cancellation token to `ParallelSelectAsync`/`ReadAsStreamAsync`/`CopyToAsync`.
Whether a non-2xx should fail the build is a separate and more contentious question, worth raising as
its own issue. As of this writing there is no open Statiq issue covering any of it.

## Testing note

The obvious seam, `IExecutionState.SendHttpRequestWithRetryAsync`, is **not testable**:
`Statiq.Testing.TestEngine` implements it as a plain `SendAsync` with no retry policy, so a test
against it passes whether or not retry works. Going through `CreateHttpClient()` +
`SendWithRetryAsync` exercises the real policy under `TestExecutionContext`, because only the message
handler is faked.

`When_downloading_images.cs` covers both behaviours: `It_should_retry_a_throttled_request` asserts the
fake server saw exactly 3 requests, and `It_should_not_write_an_error_response_to_the_asset` asserts a
persistent `404` produces no document. The second test has to raise
`TestLoggerProvider.ThrowLogLevel`, because the harness otherwise turns the logged error into an
exception before the assertions run.

## Buffering

`ReadWebAssets` reads the body with `ReadAsByteArrayAsync` and passes the array to
`context.GetContentProvider(byte[], mediaType)`, which wraps it directly in a `MemoryContent`.

Buffering is unavoidable — the `HttpResponseMessage` is disposed while the content provider is only
read later in the pipeline. But the `MemoryStreamFactory` + `CopyToAsync` dance that `ReadWeb` uses
(and which this module used at first) buys nothing here: the `Stream` overload of
`GetContentProvider` re-buffers anyway, and `KontentDownloadImages` immediately calls
`GetContentBytesAsync()` to populate its cache, so a `byte[]` is materialized regardless.
