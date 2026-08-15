# Security guidelines

Rules for writing and reviewing code in this repository.

Both humans and coding agents should treat these as binding. If you are an agent making
changes here, check your diff against the [review checklist](#review-checklist) before
handing work back.

Two framing notes:

- **These are classes, not incidents.** A rule exists because a whole category of mistake is
  cheap to prevent and expensive to find later — not because one line once got flagged.
- **Deviations are allowed, silently deviating is not.** If a rule genuinely does not fit,
  say so in the PR description and explain why. An unexplained exception reads as an
  oversight to the next reviewer.

---

## 1. GitHub Actions workflows

CI has credentials — OIDC federation to nuget.org (trusted publishing), `packages: write` to
GHCR. A workflow compromise is a supply-chain compromise, so workflows get the same scrutiny
as production code.

### 1.1 Never interpolate context values into a `run:` body

`${{ ... }}` is substituted as **raw text before the shell ever starts**. A value containing
shell metacharacters becomes executable code, not a string — quoting does not save you,
because the quotes themselves are part of the text being replaced.

Pass through `env:` and dereference as a shell variable. The value then arrives as
environment data that the shell never parses as syntax.

```yaml
# WRONG - inputs.location is pasted into the script text
- run: bash scripts/deploy.sh "${{ inputs.location }}"

# RIGHT - the value arrives as data
- env:
    LOCATION: ${{ inputs.location }}
  run: bash scripts/deploy.sh "$LOCATION"
```

This applies to **every** context — `inputs`, `vars`, `env`, `steps.*.outputs`, `needs.*`,
and all of `github.*`. Do not try to classify a context as trusted; the rule is uniform
because the exceptions are hard to reason about and change over time.

Highest-risk fields, where the content is attacker-controlled by design:
`github.event.issue.title`, `.body`, `.head_ref`, `.pull_request.*`, commit messages, and
author names.

Interpolation in `with:`, `if:`, and `env:` blocks is fine — those are structured YAML
inputs, not shell text. Only `run:` bodies are affected.

### 1.2 Pin third-party actions to a full commit SHA

A tag is a mutable pointer. Whoever controls the action repository can repoint `v4` at new
code, and it runs with your workflow's token on your next build.

```yaml
# WRONG - mutable
uses: dorny/paths-filter@v4

# RIGHT - immutable, with the human-readable version retained
uses: dorny/paths-filter@7b450fff21473bca461d4b92ce414b9d0420d706 # v4
```

- Pin **all** actions, including `actions/*`. GitHub's own namespace is lower risk, not zero
  risk, and a uniform rule is easier to enforce than a judgement call per action.
- Always keep the `# vN` trailer. Without it nobody can tell what a bare hash is.
- Use the full 40-character commit SHA. Short SHAs and tag-object SHAs are not acceptable.
- Get the SHA from the action's own repository, not from a search result:
  ```bash
  gh api repos/OWNER/REPO/git/ref/tags/vN --jq '.object.sha'
  ```
  If that returns an annotated tag object (`.object.type == "tag"`), dereference it with
  `gh api repos/OWNER/REPO/git/tags/SHA --jq '.object.sha'` to get the commit.

**Pinning without automation is a downgrade.** A pin freezes the action forever, including
past security fixes. `.github/dependabot.yml` carries a `github-actions` entry that keeps
pins moving; if you add a workflow, it is already covered — do not disable it.

When bumping a major version, confirm the inputs you use still exist at the new major before
merging. Reading `action.yml` at the pinned SHA is enough:

```bash
gh api repos/OWNER/REPO/contents/action.yml?ref=SHA --jq '.content' | base64 -d
```

### 1.3 Least-privilege tokens

- Declare `permissions:` explicitly in every workflow. The default is too broad.
- Start from `contents: read` and add only what a job proves it needs.
- Prefer job-level `permissions:` over workflow-level when only one job needs the extra
  scope — a release workflow should not grant `contents: write` to its build job.
- Use OIDC (`id-token: write` plus a trusted-publishing exchange, e.g. `NuGet/login`) for
  publishing credentials. Do not add long-lived API keys or tokens as repository secrets.

### 1.4 Untrusted triggers

- `pull_request` from a fork gets a read-only token and no secrets. Keep it that way.
- Do not add `pull_request_target` or `workflow_run` to run untrusted code. They execute in
  a trusted context with secrets available, and combined with a checkout of the PR head they
  are a direct path to credential theft. If you believe you need one, raise it for review
  first.
- Never `echo` a secret, and never pass one as a command-line argument — arguments are
  visible in process listings. Use `env:` or stdin.

---

## 2. Dependencies

### 2.1 Lock files are mandatory

Every .NET project writes a `packages.lock.json` (`RestorePackagesWithLockFile` in
`Directory.Build.props`), which pins the **full transitive closure** to exact versions and
content hashes. CI restores with `--locked-mode`, so a dependency that resolves differently
than what was committed fails the build instead of silently shipping.

This is what defends against a transitive dependency changing under you between the commit
that was reviewed and the build that ships.

Consequence for anyone changing packages: **a version change and its regenerated lock files
belong in the same commit.**

```bash
dotnet restore EasyAuthDevProxy.slnx --force-evaluate   # after any package change
```

Skipping this fails CI with `NU1004`. Do not "fix" that by dropping `--locked-mode` or
disabling the property — the failure is the control working.

### 2.2 One version per package

.NET package versions live only in `Directory.Packages.props` (Central Package Management).

- Do not put a `Version` attribute on a `PackageReference` in a project file.
- Add the `PackageVersion` entry centrally first, then reference the package.
- Avoid `VersionOverride`. It reintroduces exactly the per-project drift that CPM exists to
  eliminate. If one project truly cannot move, use it — and write down why and what would
  let it be removed.

Version drift is a security problem, not just a tidiness one: it means a patched version in
one project silently coexists with a vulnerable one in another, and neither an audit nor a
scanner report tells you which is deployed.

Aspire packages are the documented exception — they come implicitly from
`Aspire.AppHost.Sdk` and are versioned by the `Sdk` attribute in `AppHost.csproj`.

### 2.3 Responding to a vulnerability report

1. Prefer the smallest upgrade that clears the advisory. Take the patched version, not the
   newest available.
2. Bump **indirect** dependencies directly when the advisory names them — you do not have to
   wait for the intermediate package to update. Add a `PackageVersion` entry for the
   transitive package in `Directory.Packages.props` even though nothing references it
   directly yet; NuGet will pick it up once something in the graph needs it.
3. Rebuild and run the full test suite. A dependency bump is a code change.
4. Do not suppress or ignore a finding to make a scan pass. If a finding is genuinely not
   applicable, record the reasoning where the next person will find it.

Judge exploitability honestly, in both directions. A DoS in a parser this codebase never
reaches is lower priority than its CVSS implies — but "we probably don't call that path" is
an argument for scheduling, never for silence.

### 2.4 Adding a dependency

Before introducing one, consider whether the standard library covers it. Each new dependency
is permanent attack surface and someone else's release process. When you do add one:

- Check it is actively maintained and not deprecated.
- Add it centrally (see 2.2) and regenerate lock files (see 2.1).
- Prefer packages already in the closure over a new one doing a similar job.

NuGet sources are locked down in `nuget.config` — `<clear/>` plus `packageSourceMapping`, so
every package must come from nuget.org and nothing can be shadowed by a rogue feed. Do not
add package sources without review.

---

## 3. Review checklist

Before opening a PR, or before an agent reports work complete:

**If workflows changed**
- [ ] No `${{ }}` anywhere inside a `run:` body — context values go through `env:`
- [ ] Every action pinned to a 40-char commit SHA with a `# vN` trailer
- [ ] `permissions:` declared and no broader than the job needs
- [ ] No new secret echoed, logged, or passed as a command-line argument

**If dependencies changed**
- [ ] Versions changed only in `Directory.Packages.props` (no `Version` on a `PackageReference`)
- [ ] Lock files regenerated and committed in the same commit
- [ ] `dotnet restore --locked-mode` succeeds from clean
- [ ] Build and full test suite pass

**Always**
- [ ] No credentials, tokens, connection strings, or keys in source, config, or fixtures
- [ ] Nothing security-relevant weakened without saying so explicitly in the PR description

A verification you did not actually run does not count. Run the command, read the output,
and report what it said — including when it failed.
