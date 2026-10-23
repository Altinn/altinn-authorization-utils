# Altinn.Authorization.RepoCtl

`repoctl` is the command-line repository manager for Altinn Authorization repositories.
It discovers projects and their dependencies, checks repository conventions, keeps
solution and Release Please files up to date, and provides helpers for testing,
packaging, containers, and GitHub Actions.

A **vertical** is a group of related projects with its own directory, solution,
version, and configuration. Verticals can be applications (`app`), packages (`pkg`),
tools (`tool`), or libraries (`lib`). Commands that operate on one vertical accept
an ID such as `pkg:Altinn.Urn` or `tool:Altinn.Authorization.RepoCtl`.

For commands that operate on a single vertical, specify exactly one vertical
selector: the positional ID or directory path, `--vertical-id` for strict ID
lookup, or `--vertical-tag` for a release tag:

```sh
repoctl vertical test pkg:Altinn.Urn
repoctl vertical test ./src/pkgs/Altinn.Urn
repoctl vertical test --vertical-id pkg:Altinn.Urn
repoctl vertical test --vertical-tag pkg/Urn-v1.2.3
```

Explicit ID and tag selectors never fall back to directory lookup. Tag matching
uses each vertical's canonical tag prefix and a nonempty suffix after `-v`, and
requires exactly one match. It does not validate release versions, resolve Git
commits, or check deployment manifests.

## Install and run

Install the .NET tool:

```sh
dotnet tool install --global Altinn.Authorization.RepoCtl.Cli
repoctl --help
```

To use the source in this repository, run these commands from the repository root
with the .NET 10 SDK installed:

```sh
dotnet build src/tools/Altinn.Authorization.RepoCtl/Altinn.Authorization.RepoCtl.slnx
./src/tools/Altinn.Authorization.RepoCtl/src/RepoCtl.Cli/bin/Debug/net10.0/repoctl --help
```

The examples below use the installed `repoctl` command. You can substitute the
path to the built executable above.

RepoCtl searches upwards from the current directory for `.repo.json` or
`.repo.jsonc`, then loads the repository configuration and evaluates its projects
using MSBuild. Restore the target repository before using commands that inspect
projects:

```sh
dotnet restore
repoctl verticals list
```

Use `--working-directory` (also `--cwd` or `-w`) to select another repository.
Add `--help` to any command for its options. Logging supports `-v`, `-vv`, `-q`,
or an explicit `--verbosity` level.

## Discover verticals

List verticals, restrict them by kind or directory, or get JSON containing IDs,
versions, dependencies, and project types:

```sh
repoctl verticals list
repoctl verticals list --kind pkg
repoctl verticals list --dir src/tools
repoctl verticals list --format json
repoctl verticals list --working-directory /path/to/repository
```

## Check and update repository files

Run the repository's pre-commit/pre-merge checks, or check only generated
solutions or Release Please configuration:

```sh
repoctl check
repoctl check --format json
repoctl solutions check
repoctl release-please check
```

Regenerate the root and vertical `.slnx` files, including build dependencies in
vertical solutions, and update the existing `release-please-config.json`:

```sh
repoctl solutions update
repoctl release-please update
repoctl check
```

The `update` commands write repository files. Release Please updates configure
packages for packable verticals and their `Version.props` version updates; if no
Release Please configuration exists, that command does nothing.

## Test and package a vertical

`vertical test` invokes `dotnet test` on the vertical's solution.
`vertical pack` invokes `dotnet pack` for each packable project in the vertical.
Arguments after `--` are forwarded to the underlying .NET command:

```sh
repoctl vertical test pkg:Altinn.Urn
repoctl vertical test tool:Altinn.Authorization.RepoCtl -- --configuration Release
repoctl vertical pack pkg:Altinn.Urn -- --configuration Release
repoctl vertical pack tool:Altinn.Authorization.RepoCtl -- --configuration Release
```

Verticals with no tests or no packable projects are skipped with a message.

## Build application containers

For an application vertical with a `docker-bake.hcl`, RepoCtl runs
`docker buildx bake` from the repository root. This requires Docker with Buildx.
Replace `app:MyApplication` with the application's vertical ID:

```sh
repoctl vertical containers build app:MyApplication
repoctl vertical containers build app:MyApplication --tag dev
repoctl vertical containers build app:MyApplication --tag v1.2.3 --push
```

`--tag` sets the bake variable `TAG`; `--push` pushes the built images to their
configured registries.

## Promote application manifests

Promote already-built manifest images to one or more environment tags:

```sh
repoctl ci promote-app \
  --verticals 'app:MyApplication app:OtherApplication' \
  --environments 'test prod' \
  --sha "$SOURCE_SHA" \
  --repo Altinn/altinn-authorization
```

Both selection options accept quoted whitespace-separated lists or repeated
options. Each option occurrence consumes one token. The CLI library binds these
inputs as `StringValues`. Environment names may contain only lowercase ASCII
letters and digits.
RepoCtl resolves every application ID before starting and skips applications
without a `manifests` directory. For each application it creates pending GitHub
deployments named `<short-slug>/<environment>`, then runs
`flux tag artifact` to promote
`<registry>/<short-slug>-manifests:<sha>` to the selected environment
tags. Supply the registry repository prefix with `--registry` or `REPOCTL_OCI_REGISTRY`.

`--sha` defaults to `GITHUB_SHA`; `--repo` defaults to `GITHUB_REPOSITORY`.
Set `GITHUB_TOKEN` with deployments write permission. The Flux CLI must be
installed, with registry credentials allowing pushes. Flux uses the Docker
registry credentials, so keep the registry login step before invoking RepoCtl.
GitHub requests use the same Octokit client as release uploads.

Deployment creation disables auto-merge and required status contexts, and marks
only `prod` as production. A failed request or promotion stops the command.
Deployment records remain pending while promotion and Flux reconciliation take
place; creating a record does not confirm a successful rollout.

## CI helpers

`ci find-verticals` returns JSON describing the selected verticals. Within GitHub
Actions it also sets step outputs `matrix`, `verticals`, and `any` for subsequent
jobs. The default filter, `none`, includes all selected verticals:

```sh
repoctl ci find-verticals
repoctl ci find-verticals --kind pkg
```

Use changed-path filters in CI to select work based on changes:

```sh
repoctl ci find-verticals --filter full
repoctl ci find-verticals --kind app --filter infra
```

| Filter | Includes verticals affected by |
| --- | --- |
| `none` | All selected verticals, without querying changed paths. |
| `self` | Changes in the vertical or shared repository files. |
| `self-only` | Changes in the vertical itself. |
| `full` | Changes in the vertical, its dependencies, or shared repository files. |
| `infra` | Changes under an application's `infra` directory or shared repository files. |

`ci export-vertical` is intended for GitHub Actions. It exports `VERTICAL_DIR`,
`VERTICAL_KIND`, `VERTICAL_NAME`, `VERTICAL_ID`, `VERTICAL_DISPLAY_NAME`,
`VERTICAL_SLUG`, `VERTICAL_SHORT_SLUG`, `VERTICAL_TAG_PREFIX`, `VERTICAL_VERSION`,
and `VERTICAL_CHANGELOG_PATHS` for later steps:

```sh
repoctl ci export-vertical pkg:Altinn.Urn
```

It also writes step outputs named `dir`, `kind`, `name`, `id`, `displayName`,
`slug`, `shortSlug`, `tagPrefix`, `version`, and `changelogPaths`, with the same
values as the corresponding environment variables:

```yaml
- id: vertical
  run: repoctl ci export-vertical pkg:Altinn.Urn
- run: echo "${{ steps.vertical.outputs.dir }}"
```

Create an application draft release with one command:

```sh
repoctl ci draft-release app:MyApplication --version 2026.10.19 --sha "$COMMIT"
```

The version defaults to today's UTC date (`yyyy.MM.dd`). Overrides must have the
shape `yyyy.MM.dd` with an optional three-digit fourth component (001 to 999). The date
must be a valid calendar date, including leap-year rules. `--sha` defaults to
`GITHUB_SHA`, and `--repo` defaults to `GITHUB_REPOSITORY`.

The command resolves the commit through GitHub, checks that the tag does not
exist and that no draft already exists for this application, generates release
notes using `git cliff`, and creates a draft with `make_latest: false`. Tags and
titles use the vertical's tag prefix. Changelogs include the application's paths
and its transitive library dependencies, using the repository's `cliff.toml`.

Set `GITHUB_TOKEN` with contents write permission; both Octokit and git-cliff use
this variable. Install git-cliff and check out the source commit with full Git
history and tags before running the command. The command verifies that local
HEAD matches the resolved release commit.

In `release-app.yml`, after checkout and git-cliff installation, the five Bash
steps can be replaced with:

```yaml
- name: Create draft release
  env:
    GITHUB_TOKEN: ${{ github.token }}
    APP: ${{ inputs.app }}
    VERSION_OVERRIDE: ${{ inputs.version }}
  run: repoctl ci draft-release "app:${APP}" --version "$VERSION_OVERRIDE"
```

Update the workflow's installed RepoCtl version to a release containing this
command before switching its invocation.

## Publish artifacts

Publish matching packages using `dotnet nuget push`. Additional arguments are
forwarded to that command, which is invoked with `--skip-duplicate`:

```sh
repoctl nuget publish 'artifacts/**/*.nupkg' -- \
  --source https://api.nuget.org/v3/index.json --api-key "$NUGET_API_KEY"
```

Upload matching files to an existing GitHub release using its numeric release ID:

```sh
repoctl github upload 123456 'artifacts/**/*.nupkg' --repo Altinn/altinn-authorization-utils
```

These commands publish to external services and require the appropriate
credentials. Quote globs so RepoCtl expands them relative to the repository root.

## Tag application release images

After checking out the source commit, setting up Docker Buildx and the Flux CLI,
and logging in to GHCR, replace the image discovery and retagging script with:

```sh
repoctl ci tag-app-images --vertical-id "$VERTICAL" --sha "$SOURCE_SHA" --version "$VERSION"
```

The command resolves the default group from the application's `docker-bake.hcl`,
including nested groups and matrix targets. Every selected target must have an
image tagged with the source SHA. Container images are retagged as `v<version>`
using `docker buildx imagetools create`. The application's manifests artifact is
retagged as `v<version>` separately using `flux tag artifact`, preserving its OCI manifest.
Discovery completes before retagging begins; retagging stops on the first failure.
Supply the manifests repository prefix with `--registry` or `REPOCTL_OCI_REGISTRY`. Image repositories from Bake are retained as given.
The version follows the same calendar-version rules as `ci draft-release`, including date validation.
Update the workflow's installed RepoCtl version before switching its invocation.
