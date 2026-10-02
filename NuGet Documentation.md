# NuGet releases

The version 2.x package targets .NET Standard 2.1, .NET 10, and .NET 11. .NET 8 and 9 use the .NET Standard 2.1 assembly. .NET Framework is not supported.

Builds use the SDK specified in [global.json](global.json), currently .NET 11 RC1. The test suite runs on .NET 8, 10, and 11 through [nuget-publish.yml](.github/workflows/nuget-publish.yml).

## Configuration

Set the repository secret `NUGET_API_KEY` to a NuGet.org API key permitted to publish `Tedd.WildcardMatch`. The publishing job uses the `nuget` GitHub environment; configure its reviewers or restrictions if release approval is required.

Only pushes that change the package version and manual workflow runs on the `deploy` branch publish NuGet packages. Creating `deploy` also publishes its initial version. Documentation and benchmark updates with an unchanged version build and test without publishing NuGet. Pushes to `main`, pull requests, and manual runs on other branches build, test, validate the benchmark corpus, and verify the package without publishing. Tags do not trigger publication.

The [Pages workflow](.github/workflows/pages.yml) also requires `deploy`. Enable GitHub Pages with **GitHub Actions** as its build source.

## Release procedure

1. Set a previously unpublished `MAJOR.MINOR.PATCH` version in `src/Tedd.WildcardMatch/Tedd.WildcardMatch.csproj`.
2. Commit and push that change to `main`.
3. Wait for the .NET 8, 10, and 11 tests, benchmark correctness checks, and package verification to pass.
4. Promote the validated commit to `deploy` and push that branch:

```sh
git switch deploy
git merge --ff-only main
git push origin deploy
```

If `deploy` does not exist, create it from the validated `main` commit:

```sh
git switch -c deploy main
git push -u origin deploy
```

Pushing a new version to `deploy` publishes the verified NuGet package and its adjacent `.snupkg` symbol package. Site changes deploy to GitHub Pages. Manual NuGet and Pages runs must also select `deploy`; a manual NuGet run can retry an unchanged version after a failed release. `--skip-duplicate` permits NuGet reruns after a successful upload.

## Package contents

The package contains one assembly for each target, the README, and the LGPL-2.1 license. Symbols use the `snupkg` format. Publishing does not change the version or create commits or tags.
