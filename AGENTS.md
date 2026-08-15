# AGENTS.md

Guidance for AI agents working on **Kontent.Statiq** — an OSS NuGet package that
provides [Statiq](https://statiq.dev) modules for pulling content and assets from the
[Kontent.ai](https://kontent.ai) headless CMS.

Repo: https://github.com/alanta/Kontent.Statiq · License: MIT · Package: `Kontent.Statiq`

## Layout

| Path | What it is |
| --- | --- |
| `Kontent.Statiq/` | The library. This is the shipped NuGet package. |
| `Kontent.Statiq.Tests/` | xUnit test project. |
| `.github/workflows/` | `ci.yml` (build+test on push), `release.yml` (tag `v*` → draft GitHub release), `publish.yml` (release published → push to nuget.org). |
| `GitVersion.yml` | Version is derived from git history by GitVersion — never hard-code a version in a csproj. |
| `README.md` | The user-facing docs *and* the NuGet package readme (`PackageReadmeFile`). Update it when public API changes. |

Ignore `obj/`, `bin/`, `.vs/`, `_NCrunch_Kontent.Statiq/` — build/IDE leftovers, already gitignored.

### Main types in the library

- `Kontent<TContent>` / `KontentTaxonomy<TTaxonomy>` — the input modules that query the Delivery API.
- `KontentImageProcessor` — rewrites asset URLs in rendered HTML to local paths and records the
  needed downloads in document metadata under `KontentKeys.Images.Downloads`.
- `KontentDownloadImages` — reads that metadata and fetches the assets (wraps Statiq's `ReadWeb`).
- `KontentAssetHelper` — url → local filename mapping (query string is hashed into the name).
- `TypedContentExtensions`, `KontentDocumentHelpers`, `Html/HtmlHelpers` — helpers for Razor views.

`InternalsVisibleTo` is set for the test assembly, so internal types are testable.

## Build and test

Target framework is **net8.0** for both projects. If only a newer SDK is installed, the build
still works (targeting packs come from NuGet) but the **test host needs a roll-forward**:

```bash
dotnet build Kontent.Statiq.sln -c Debug
DOTNET_ROLL_FORWARD=LatestMajor dotnet test Kontent.Statiq.sln -c Debug --no-build
```

Expected: build succeeds with warnings only; all tests pass (34 as of the last check).

Notes:
- `GeneratePackageOnBuild` is on, so every build packs a `.nupkg` — the `NU5104` warning
  (stable package with prerelease Statiq dependency) is expected and harmless.
- SonarAnalyzer runs as part of the build. Existing `S1133`/`S6602` warnings are known noise;
  don't "fix" deprecated-code warnings on public API without an explicit deprecation decision.
- Statiq packages are referenced as `1.0.0-*` (floating prerelease). Statiq 1.0 has been in
  beta for years; `beta.72` is the newest and it targets netcoreapp3.1, so it does not constrain
  which .NET version this library targets.

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
