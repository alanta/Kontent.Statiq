# AGENTS.md

Guidance for AI agents working on **Kontent.Statiq** — an OSS NuGet package that
provides [Statiq](https://statiq.dev) modules for pulling content and assets from the
[Kontent.ai](https://kontent.ai) headless CMS.

Repo: https://github.com/alanta/Kontent.Statiq · License: MIT · Package: `Kontent.Statiq`

> **Read [SECURITY_GUIDELINES.md](SECURITY_GUIDELINES.md) before changing anything under
> `.github/workflows/` or touching dependencies.** Those rules are binding, they include a
> review checklist to run your diff against, and a deviation has to be stated in the PR
> description rather than left for the reviewer to spot.

## Layout

| Path | What it is |
| --- | --- |
| `Kontent.Statiq/` | The library. This is the shipped NuGet package. |
| `Kontent.Statiq.Tests/` | xUnit test project. |
| `docs/` | Design notes explaining *why* something is built the way it is, for decisions that look like they could be simplified away. Not user-facing docs — those go in `README.md`. |
| `.github/workflows/` | `ci.yml` (build+test on push), `release.yml` (tag `v*` → draft GitHub release), `publish.yml` (release published → push to nuget.org). |
| `GitVersion.yml` | Version is derived from git history by GitVersion — never hard-code a version in a csproj. |
| `Directory.Packages.props` | Every package version, centrally. A `PackageReference` must not carry a `Version`. |
| `Directory.Build.props` | `RestorePackagesWithLockFile` and the local `VersionSuffix`. |
| `nuget.config` | Sources are cleared and mapped to nuget.org only — don't add a feed without review. |
| `README.md` | The user-facing docs *and* the NuGet package readme (`PackageReadmeFile`). Update it when public API changes. |

Ignore `obj/`, `bin/`, `.vs/`, `_NCrunch_Kontent.Statiq/` — build/IDE leftovers, already gitignored.

### Main types in the library

- `Kontent<TContent>` / `KontentTaxonomy<TTaxonomy>` — the input modules that query the Delivery API.
- `KontentImageProcessor` — rewrites asset URLs in rendered HTML to local paths and records the
  needed downloads in document metadata under `KontentKeys.Images.Downloads`.
- `KontentDownloadImages` — reads that metadata and fetches the assets, in batches of 20, caching
  results so a preview re-render doesn't download everything again.
- `ReadWebAssets` — internal; the actual downloader. Deliberately *not* Statiq's `ReadWeb`, which
  leaks sockets and writes throttling error pages to disk as image content. It only does anonymous
  GETs and can never do more, because `WebRequestHeaders.ApplyTo` is internal to `Statiq.Core`.
  Read [docs/asset-downloads.md](docs/asset-downloads.md) before changing either module.
- `KontentAssetHelper` — url → local filename mapping (query string is hashed into the name).
- `TypedContentExtensions`, `KontentDocumentHelpers`, `Html/HtmlHelpers` — helpers for Razor views.

`InternalsVisibleTo` is set for the test assembly, so internal types are testable.

## Build and test

Target framework is **net8.0** for both projects.

```bash
dotnet build Kontent.Statiq.sln -c Debug
dotnet test Kontent.Statiq.sln -c Debug --no-build
```

Expected: build succeeds **warning free**; all tests pass (43 as of the last check).

If only a newer SDK/runtime is installed, the build still works (targeting packs come from NuGet)
but the test host will not start. Either install the .NET 8 runtime or run the tests with
`DOTNET_ROLL_FORWARD=LatestMajor dotnet test ...`.

Notes:
- `GeneratePackageOnBuild` is on, so every build packs a `.nupkg`. `Directory.Build.props` sets
  `VersionSuffix=local` so that package is a prerelease like every version this repo publishes —
  otherwise the version defaults to a stable `1.0.0` and packing warns `NU5104` (stable package
  with a prerelease Statiq dependency). Released builds pass `-p:Version=`, which overrides it.
- Statiq packages are pinned to `1.0.0-beta.72`. Statiq 1.0 has been in beta for years and has
  published nothing since January 2024; beta.72 targets netcoreapp3.1, so it does not constrain
  which .NET version this library targets.

### SonarAnalyzer

SonarAnalyzer runs as part of the build and **the build is warning free — keep it that way.**
SonarCloud also analyses pull requests; its rules come from the quality profile on Sonar's side,
not from anything in this repo.

Judge a finding before acting on it. If it describes a real defect, fix it. If it is a false
positive, **suppress it at the site with a `#pragma warning disable Sxxxx` and a comment saying
why** — do not reshape working code to satisfy a rule that is wrong. Two examples to copy:

- `KontentAssetHelper.GetLocalFileName` — `S4790` (weak hash). The MD5 is a filename
  discriminator, not a security control, and changing it would rename every published asset.
- `KontentConfig.GetChildren` — `S2955` (null check on an unconstrained type parameter). The
  alternatives are a comparison to `default` that reads worse, or a `where T : class` constraint
  that changes a public signature.

Reserve `.editorconfig` (`dotnet_diagnostic.Sxxxx.severity = none`) for a decision that genuinely
applies repo-wide, with the reasoning in a comment above it. `S1133` ("remove this deprecated code
someday") is off there because this library deliberately marks public API `[Obsolete]` before
removing it in a major version. Turning a rule off globally to fix one call site gives away every
place the rule would have been right.

### Dependencies

Versions live in `Directory.Packages.props` (central package management) and both projects commit a
`packages.lock.json`. CI restores with `--locked-mode`, so **a package change and its regenerated
lock files belong in the same commit**:

```bash
dotnet restore Kontent.Statiq.sln --force-evaluate
```

Skipping that fails CI with `NU1004`. Don't "fix" it by dropping `--locked-mode`. `nuget.config`
pins the only package source to nuget.org. See SECURITY_GUIDELINES.md section 2.

## Line endings — read this before committing

The repository content is stored with **LF**. There is no `.gitattributes`, so a checkout on
Windows (or a work area copied from Windows) easily produces a working tree full of CRLF files
whose entire content then shows up as modified.

- Before judging the diff, use `git diff --ignore-cr-at-eol` to see the *real* changes.
- Never commit a whole-file CRLF/LF flip mixed with a behavioural change — it hides the change.
- If a file is CRLF and you only need to edit a few lines, normalize the file in its own commit,
  or leave the endings alone and edit in place.

## Conventions

- Test classes are named `When_<situation>` with `[Fact]` methods named `It_should_<expectation>`,
  in `Kontent.Statiq.Tests/`, laid out Arrange / Act / Assert with comment markers.
- Assertions use **FluentAssertions**; mocks use **FakeItEasy** (`A.Fake<IDeliveryClient>()`, with
  the extension helpers in `Tools/KontentSetupHelpers.cs`); HTTP is faked with
  **RichardSzalay.MockHttp**; pipelines are driven with **Statiq.Testing** (`Engine`, `TestDocument`).
- Prefer extending `KontentSetupHelpers` over hand-rolling new Delivery API fakes.
- Tests never hit the network. `TestExecutionContext.HttpResponseFunc` intercepts every HTTP call
  (its default returns an empty `200`); see the `FakeAssetServer` in `When_downloading_images.cs`
  for the pattern of serving content *and* counting requests.
- `TestExecutionContext` is a test double, not the real `Engine`, so engine-level behaviour is not
  exercised — notably it never calls `IConcurrentCache.ResetCaches()`. A cache registered as
  `resettable` therefore looks like it survives between executions in tests when it would not in a
  real build. `TestEngine.SendHttpRequestWithRetryAsync` is a second instance of the same trap: it
  is a plain `SendAsync` with the retry policy stripped out, so a test written against it passes
  whether or not retry works. Check such assumptions against the Statiq source, not just a green
  test run.
- A test that expects a failure has to raise `TestExecutionContext.TestLoggerProvider.ThrowLogLevel`
  first — anything logged at `Warning` or above otherwise becomes an exception before the assertions
  run. See `When_downloading_images.cs`.
- `Nullable` is enabled in both projects — keep annotations correct rather than sprinkling `!`.
- Public API needs XML doc comments (`GenerateDocumentationFile` is on; missing docs warn).
- Modules can be executed concurrently by Statiq across documents — **any shared state in a module
  or static helper must be thread-safe**. Use `ConcurrentCache`/`ConcurrentDictionary`, and prefer
  the static one-shot crypto APIs (`MD5.HashData`) over shared `HashAlgorithm` instances.

## Working with GitHub

`gh issue view` / `gh pr view` currently fail on this repo with a GraphQL `projectCards`
deprecation error. Use the REST API instead:

```bash
gh api repos/alanta/Kontent.Statiq/issues/44 --jq '.title, .body'
gh api repos/alanta/Kontent.Statiq/pulls --jq '.[] | "\(.number) \(.title)"'
```

`gh issue list` / `gh pr list` do work.

## Ground rules

- This is a public OSS repo. Don't push branches, open/close issues or PRs, or publish packages
  without being asked to.
- The default branch is `main`; work happens on `feature/*` branches merged via PR.
- Releases are cut by tagging `v*` — that triggers `release.yml`, and publishing to nuget.org only
  happens when the resulting draft release is published manually.
