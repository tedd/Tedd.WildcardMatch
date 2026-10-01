# NuGet releases

The library targets .NET Standard 2.0. Build, tests, benchmark validation, and package verification run on .NET 10 through [nuget-publish.yml](.github/workflows/nuget-publish.yml).

## Configuration

Set the repository secret `NUGET_API_KEY` to a NuGet.org API key permitted to publish `Tedd.WildcardMatch`. The publishing job uses the `nuget` GitHub environment; configure its reviewers or restrictions if release approval is required.

Pull requests, manual workflow runs, and ordinary pushes to `main` verify the package. Only a `vMAJOR.MINOR.PATCH` tag publishes it. The tag must match the project's `Version` exactly and point to a commit belonging to `main`.

## Release procedure

1. Set a previously unpublished version in `src/Tedd.WildcardMatch/Tedd.WildcardMatch.csproj`.
2. Commit and push that change to `main`.
3. Wait for the build, tests, benchmark correctness checks, and package verification to pass.
4. Create and push the corresponding version tag. For example, a project version of `1.0.4` requires:

```sh
git tag v1.0.4
git push origin v1.0.4
```

The workflow publishes the verified package artifact and its adjacent `.snupkg` symbol package. `--skip-duplicate` permits reruns after a successful upload. An invalid version tag or a commit outside `main` fails before publication.

## Package contents

The package contains the library, README, and LGPL-2.1 license. Symbols use the `snupkg` format. Publishing does not change the version, create commits, or create tags.
