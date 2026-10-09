# Altinn.Authorization.RepoCtl

`repoctl` is the command-line repository manager for Altinn Authorization repositories.
It discovers projects and their dependencies, checks repository conventions, keeps
solution and Release Please files up to date, and provides helpers for testing,
packaging, containers, and GitHub Actions.

A **vertical** is a group of related projects with its own directory, solution,
version, and configuration. Verticals can be applications (`app`), packages (`pkg`),
tools (`tool`), or libraries (`lib`). Commands that operate on one vertical accept
an ID such as `pkg:Altinn.Urn` or `tool:Altinn.Authorization.RepoCtl`.

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
`VERTICAL_SLUG`, `VERTICAL_SHORT_SLUG`, and `VERTICAL_VERSION` for later steps:

```sh
repoctl ci export-vertical pkg:Altinn.Urn
```

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
