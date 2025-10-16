## Init
Creates `vc-package.json` boilerplate with the latest version number of the platform. The version number can be specified by the `PlatformVersion` parameter.

For example:
```console
vc-build Init
vc-build Init -PlatformVersion 3.52.0
```
---
## Install
Downloads and installs the platform or modules into the current folder. Versions can be specified as command parameters or defined in `vc-package.json`.

`vc-package.json` maintains the list of installed modules with their versions, allowing `vc-build` to restore the platform and modules on different machines.

### Parameters
- `PackageManifestPath`, `DiscoveryPath`, `ProbingPath`: Override paths.
- `SkipDependencySolving`: Skip dependency solving.
- `GithubToken`, `AzureToken`, `GitLabToken`: Pass tokens for authorization.
- `-Edge`: Install the latest available versions.

Since version 3.17.0, stable versions of modules are installed by default.

### Usage
```console
vc-build install
vc-build install -GitLabToken $GITLAB_TOKEN
vc-build install -platform -version <version>
vc-build install -platform -PlatformAssetUrl https://github.com/VirtoCommerce/vc-platform/releases/download/3.216.13/VirtoCommerce.Platform.3.216.13.zip
vc-build install -module <module> -version <version>
vc-build install -module <module>:<version>
vc-build install -PackageManifestPath some_directory/vc-package.json -DiscoveryPath ../modules -ProbingPath platform_dir/app_data/modules -SkipDependencySolving
```
---
## Update
Updates platform and modules to the latest stable bundle or specified versions. Before proceeding with the update, it shows a diff of version changes and asks for user confirmation.

### Parameters
- `-Edge`: Update to the latest available versions instead of stable bundle
- `-v`: Specify bundle name (default is "latest")

### Usage
```console
vc-build Update
vc-build Update -v 3
vc-build Update -edge
```
---
## ShowDiff
Shows the differences between current and target versions of the platform and modules. This target is automatically triggered before the Update target and requires user confirmation to proceed.

### Parameters
- `-Edge`: Show differences for the latest available versions instead of stable bundle
- `-v`: Specify bundle name (default is "latest")

### Usage
```console
vc-build ShowDiff
vc-build ShowDiff -v 3
vc-build ShowDiff -edge
```
---
## InstallModules
Installs modules according to `vc-package.json` and solves dependencies.

### Usage
```console
vc-build InstallModules
vc-build InstallModules -DiscoveryPath ../modules
```
---
## ValidateDependencies
Validates module dependencies against the installed platform version and other modules. Checks for version conflicts and missing dependencies. This target is automatically triggered by the Install and Update targets.

### Usage
```console
vc-build ValidateDependencies
vc-build ValidateDependencies -DiscoveryPath ../modules
```
---
## InstallPlatform
Installs the platform according to `vc-package.json`.

### Usage
```console
vc-build InstallPlatform
```
---
## Uninstall
Removes specified modules.

### Usage
```console
vc-build uninstall -Module VirtoCommerce.Cart VirtoCommerce.Catalog
```
---
## Clean
Cleans `bin`, `obj`, and `artifacts` directories.

### Usage
```console
vc-build clean
```
---
## Restore
Executes `dotnet restore`.

### Usage
```console
vc-build restore
vc-build restore -NugetConfig <path to nuget config>
```
---
## Compile
Executes `dotnet build`.

### Usage
```console
vc-build compile
```
---
## Pack
Creates NuGet packages by executing `dotnet pack`.

### Usage
```console
vc-build pack
```
---
## Test
Executes unit tests and generates a coverage report. Can filter tests with `-TestsFilter`.

### Usage
```console
vc-build Test
vc-build Test -TestsFilter "Category!=IntegrationTest"
```
---
## PublishPackages
Publishes NuGet packages in the `./artifacts` directory with `-Source` and `-ApiKey` parameters.

### Usage
```console
vc-build PublishPackages -ApiKey %SomeApiKey%
```
---
## QuickRelease
Creates a release branch from `dev`, merges it into `master`, increments the version in the `dev` branch, and removes the `release/*` branch.

### Usage
```console
vc-build QuickRelease
vc-build QuickRelease -Force
```
---
## QuickHotfix
Creates a hotfix branch from the main branch, increments the patch version, merges the current branch into the created hotfix branch, and merges it back into the main branch.
### Usage
```console
vc-build QuickHotfix
vc-build QuickHotfix -MainBranch main
vc-build QuickHotfix -CustomVersionPrefix 3.200.2
```
---
## Publish
Executes `dotnet publish`.

### Usage
```console
vc-build publish
```
---
## WebPackBuild
Executes `npm ci` and then `npm run webpack:build`.

### Usage
```console
vc-build WebPackBuild
```
---
## Compress
Compresses an artifact to an archive and filters excess files.
Behavior changes since 3.817
- Automatically excludes binary files (.dll, .so) originating from module dependencies:
- Downloads dependency module zips (stable or prerelease) and aggregates binary filenames to ignore.
- Merges ignore sources: global `module.ignore`, local `.moduleignore`, and dependency-derived entries; trims, de-duplicates, and sorts.
- Improved stability when ignore inputs are empty or missing.

### Parameters
- `ModulesCachePath`: Path for caching downloaded dependency zips. Defaults to `%USERPROFILE%/.vc-build/cache` or `VCBUILD_CACHE` env var if set.
- `PrereleasesBlobContainer`: Base URL to download prerelease module packages (used to derive dependency ignore lists). Default: `https://vc3prerelease.blob.core.windows.net/packages/`.
- `DisableIgnoreDependencyFiles`: When true, disables automatic exclusion of dependency binary files from the archive.
-

### Usage
```console
vc-build compress
vc-build compress -configuration Release
vc-build Compress -NugetConfig <path to nuget config>
```
---
## MatchVersions
Validates version consistency of all VirtoCommerce dependencies across all projects in the solution.
Checks performed:
- All module dependencies listed in the manifest are present in project references.
- All `VirtoCommerce.Platform.*` dependencies in all projects match the version specified in the manifest.
- All module dependencies in all projects match the version specified in the manifest.
- All `VirtoCommerce.Platform.*` dependencies across all projects have the same version.
- All module dependencies (`VirtoCommerce.*Module`) across all projects have the same version.

If any inconsistencies are found, the build will fail and detailed errors will be logged.
### Usage
```console
vc-build MatchVersions
```
---
## PublishModuleManifest
Updates `modules_v3.json` with information from the current artifact's `module.manifest`.

### Usage
```console
vc-build PublishModuleManifest
```
---
## SonarQubeStart
Starts the Sonar scanner by executing `dotnet sonarscanner begin`. Accepts parameters like `SonarBranchName`, `SonarPRBase`, `SonarPRBranch`, `SonarPRNumber`, `SonarGithubRepo`, `SonarPRProvider`, `SonarAuthToken`.

### Usage
```console
vc-build SonarQubeStart -SonarBranchName dev -SonarAuthToken *** -RepoName vc-module-marketing
```
---
## SonarQubeEnd
Executes `dotnet sonarscanner end`. Accepts `SonarAuthToken` parameter.

### Usage
```console
vc-build SonarQubeEnd -SonarAuthToken %SonarToken%
```
---
## Release
Creates a GitHub release. Accepts parameters like `GitHubUser`, `GitHubToken`, `ReleaseBranch`.

### Usage
```console
vc-build release -GitHubUser VirtoCommerce -GitHubToken %token%
```
---
## ClearTemp
Removes the `.tmp` directory.

### Usage
```console
vc-build ClearTemp
```
---
## DockerLogin
Executes `docker login`. Accepts parameters like `DockerRegistryUrl`, `DockerUsername`, `DockerPassword`.

### Usage
```console
vc-build dockerlogin -DockerRegistryUrl https://myregistry.com -DockerUsername user -DockerPassword 12345
```
---
## BuildImage
Builds a Docker image. Accepts parameters like `DockerfilePath`, `DockerImageFullName`.

### Usage
```console
vc-build buildimage -DockerfilePath ./dockerfile -DockerImageFullName myimage:dev
```
---
## PushImage
Pushes a Docker image to the remote registry. Accepts `DockerImageFullName` parameter.

### Usage
```console
vc-build PushImage -DockerImageFullName myimage:dev
```
---
## BuildAndPush
Builds and pushes a Docker image. Accepts parameters like `DockerRegistryUrl`, `DockerUsername`, `DockerPassword`, `DockerfilePath`, `DockerImageFullName`.

### Usage
```console
vc-build BuildAndPush -DockerRegistryUrl https://myregistry.com -DockerUsername user -DockerPassword 12345 -DockerfilePath ./dockerfile -DockerImageFullName myimage:dev
```
---
## Configure
Validates and updates a connection string in `appsettings.json`. Accepts parameters like `Sql`, `Redis`, `AzureBlob`, `AppsettingsPath` (default is `./appsettings.json`).

### Usage
```console
vc-build Configure -Sql "MsSql connection string" -Redis "Redis connection string" -AzureBlob "Container connection string"
```
---
## CloudEnvUpdate
Updates applications in the cloud. Accepts the following parameters: `CloudToken`, `Manifest`, `RoutesFile` (optional).

### Usage
```console
vc-build CloudEnvUpdate -CloudToken <your token> -Manifest <path to application manifest>
vc-build CloudEnvUpdate -CloudToken <your token> -Manifest <path to application manifest> -RoutesFile <path to routes file>
```
---
## CloudEnvSetParameter
Updates parameters of the cloud environment. Accepts parameters like `CloudToken`, `EnvironmentName`, `HelmParameters` (Array), `Organization` (optional).

### Usage
```console
vc-build CloudEnvSetParameter -CloudToken <your token> -EnvironmentName <environment name> -HelmParameters platform.config.paramname=somevalue123
```
---
## CloudEnvStatus
Waits for health and/or sync statuses of the environment. Accepts parameters like `CloudToken`, `EnvironmentName`, `HealthStatus`, `SyncStatus`.

### Usage
```console
vc-build CloudEnvStatus -CloudToken <your token> -EnvironmentName <environment name> -HealthStatus Healthy -SyncStatus Progressing
```
---
## CloudAuth
Saves a token for accessing the VirtoCloud portal, eliminating the need to use the `CloudToken` parameter with every call to targets in the Cloud group. Accepts parameters like `AzureAD` (optional), `CloudToken` (optional).

### Usage
```console
vc-build CloudAuth
vc-build CloudAuth -AzureAD
vc-build CloudAuth -CloudToken <token>
```
---
## CloudInit
Creates a new environment. Accepts the `ServicePlan` parameter to specify the service plan (default value is `F1`).

### Usage
```console
vc-build CloudInit -EnvironmentName <EnvName>
vc-build CloudInit -EnvironmentName <EnvName> -ServicePlan F1
```
---
## CloudEnvList
Lists environments with statuses.

### Usage
```console
vc-build CloudEnvList
```
---
## CloudEnvRestart
Restarts the environment.

### Usage
```console
vc-build CloudEnvRestart -EnvironmentName <EnvName>
```
---
## CloudEnvLogs
Shows environment logs.

### Usage
```console
vc-build CloudEnvLogs -EnvironmentName <EnvName>
```
---
## CloudDown
Deletes the environment. Accepts parameters like `Organization` (optional), `EnvironmentName` (required).

### Usage
```console
vc-build CloudDown -EnvironmentName <EnvName>
vc-build CloudDown -Organization <OrgName> -EnvironmentName <EnvName>
```
---
## CloudDeploy
Deploys a custom image to the existing environment.

### Usage
```console
vc-build CloudDeploy -EnvironmentName <EnvName> -DockerUsername <username of docker hub>
```
---
## CloudUp
Deploys a custom image to a new environment.

### Usage
```console
vc-build CloudUp -EnvironmentName <EnvName> -DockerUsername <username of docker hub>
```
