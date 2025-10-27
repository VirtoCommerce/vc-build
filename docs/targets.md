## Init
Creates `vc-package.json` boilerplate with the latest version number of the platform. The version number can be specified by the `PlatformVersion` parameter.

### Parameters
- `PlatformVersion`: Specify the platform version (optional, defaults to latest)

### Usage
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

### Parameters
- `DiscoveryPath`: Override modules discovery path

### Usage
```console
vc-build InstallModules
vc-build InstallModules -DiscoveryPath ../modules
```
---
## ValidateDependencies
Validates module dependencies against the installed platform version and other modules. Checks for version conflicts and missing dependencies. This target is automatically triggered by the Install and Update targets.

### Parameters
- `DiscoveryPath`: Override modules discovery path

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

### Parameters
- `Module`: Array of module names to remove (required)

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

### Parameters
- `NugetConfig`: Path to NuGet configuration file (optional)

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

### Parameters
- `TestsFilter`: Filter expression for test execution (optional, defaults to "Category!=IntegrationTest")

### Usage
```console
vc-build Test
vc-build Test -TestsFilter "Category!=IntegrationTest"
```
---
## PublishPackages
Publishes NuGet packages in the `./artifacts` directory with `-Source` and `-ApiKey` parameters.

### Parameters
- `ApiKey`: API key for the specified source (required)
- `Source`: NuGet source URL (optional, defaults to "https://api.nuget.org/v3/index.json")

### Usage
```console
vc-build PublishPackages -ApiKey %SomeApiKey%
```
---
## QuickRelease
Creates a release branch from `dev`, merges it into `master`, increments the version in the `dev` branch, and removes the `release/*` branch.

### Parameters
- `Force`: Force operation without confirmation prompts

### Usage
```console
vc-build QuickRelease
vc-build QuickRelease -Force
```
---
## QuickHotfix
Creates a hotfix branch from the main branch, increments the patch version, merges the current branch into the created hotfix branch, and merges it back into the main branch.

### Parameters
- `MainBranch`: Main branch name (optional, defaults to "master")
- `CustomVersionPrefix`: Custom version prefix to use

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
- `Configuration`: Build configuration (optional, defaults to Release)
- `NugetConfig`: Path to NuGet configuration file (optional)

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

### Parameters
- `SonarBranchName`: Branch name for SonarQube analysis
- `SonarPRBase`: Pull request base branch for SonarQube
- `SonarPRBranch`: Pull request branch for SonarQube
- `SonarPRNumber`: Pull request number for SonarQube
- `SonarGithubRepo`: GitHub repository for SonarQube
- `SonarPRProvider`: Pull request provider for SonarQube
- `SonarAuthToken`: Authentication token for SonarQube (required)
- `RepoName`: Repository name

### Usage
```console
vc-build SonarQubeStart -SonarBranchName dev -SonarAuthToken *** -RepoName vc-module-marketing
```
---
## SonarQubeEnd
Executes `dotnet sonarscanner end`. Accepts `SonarAuthToken` parameter.

### Parameters
- `SonarAuthToken`: Authentication token for SonarQube (required)

### Usage
```console
vc-build SonarQubeEnd -SonarAuthToken %SonarToken%
```
---
## Release
Creates a GitHub release. Accepts parameters like `GitHubUser`, `GitHubToken`, `ReleaseBranch`.

### Parameters
- `GitHubUser`: GitHub username (required)
- `GitHubToken`: GitHub authentication token (required)
- `ReleaseBranch`: Release branch name (optional)

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

### Parameters
- `DockerRegistryUrl`: Docker registry URL (optional, defaults to Docker Hub)
- `DockerUsername`: Docker username (required)
- `DockerPassword`: Docker password (required)

### Usage
```console
vc-build dockerlogin -DockerRegistryUrl https://myregistry.com -DockerUsername user -DockerPassword 12345
```
---
## PrepareDockerContext
Prepares the Docker build context for containerizing VirtoCommerce applications. This target creates a complete Docker build environment by:

1. **Creating Docker build context directory**: Sets up `artifacts/docker/` directory structure
2. **Downloading Docker assets**: Downloads Dockerfile and wait-for-it.sh script from VirtoCommerce Docker repository
3. **Publishing platform**: Compiles and publishes the VirtoCommerce platform to `artifacts/docker/publish/`
4. **Copying modules**: Builds and copies all discovered modules to the Docker context
5. **Setting Docker parameters**: Auto-configures Docker image names, tags, and build context paths

This target is automatically triggered by cloud deployment targets and prepares everything needed for Docker image creation.

### Parameters
- `DockerUsername`: Username for Docker registry (used to generate image name if not specified)
- `EnvironmentName`: Environment name (used to generate image name if not specified)  
- `DockerImageName`: Custom Docker image name (optional, auto-generated if not provided)
- `DockerImageTag`: Custom Docker image tags (optional, timestamp-based tag generated if not provided)
- `DockerfileUrl`: URL to custom Dockerfile (optional, defaults to VirtoCommerce platform Dockerfile)
- `WaitScriptUrl`: URL to wait-for-it.sh script (optional, defaults to VirtoCommerce script)

### Usage
```console
vc-build PrepareDockerContext -DockerUsername myuser -EnvironmentName dev
vc-build PrepareDockerContext -DockerImageName myregistry/myapp -DockerImageTag latest
```
---
## BuildImage
Builds a Docker image from the prepared Docker context. Uses the Dockerfile and published application files to create a containerized VirtoCommerce application.

### Parameters
- `DockerImageName`: Docker image name (required)
- `DockerImageTag`: Docker image tags (optional, defaults to timestamp-based tag)
- `DockerfilePath`: Path to Dockerfile (optional, defaults to `artifacts/docker/Dockerfile`)
- `DockerBuildContext`: Docker build context path (optional, defaults to `artifacts/docker/`)

### Usage
```console
vc-build BuildImage -DockerImageName myuser/myapp
vc-build BuildImage -DockerImageName myregistry/myapp -DockerImageTag v1.0.0
vc-build BuildImage -DockerImageName myapp -DockerfilePath ./custom.dockerfile
```
---
## PushImage
Pushes a Docker image to the specified registry. Requires Docker login credentials for private registries.

### Parameters
- `DockerImageName`: Docker image name to push (required)
- `DockerImageTag`: Image tags to push (optional, pushes all tags if not specified)
- `DockerRegistryUrl`: Docker registry URL (optional, defaults to Docker Hub)
- `DockerUsername`: Registry username (required for authentication)
- `DockerPassword`: Registry password (required for authentication)

### Usage
```console
vc-build PushImage -DockerImageName myuser/myapp -DockerUsername myuser -DockerPassword mypass
vc-build PushImage -DockerImageName myregistry.com/myapp -DockerRegistryUrl myregistry.com
```
---
## BuildAndPush
Builds and pushes a Docker image. Accepts parameters like `DockerRegistryUrl`, `DockerUsername`, `DockerPassword`, `DockerfilePath`, `DockerImageFullName`.

### Parameters
- `DockerRegistryUrl`: Docker registry URL (optional, defaults to Docker Hub)
- `DockerUsername`: Docker username (required)
- `DockerPassword`: Docker password (required)
- `DockerfilePath`: Path to Dockerfile (required)
- `DockerImageFullName`: Full Docker image name with tag (required)

### Usage
```console
vc-build BuildAndPush -DockerRegistryUrl https://myregistry.com -DockerUsername user -DockerPassword 12345 -DockerfilePath ./dockerfile -DockerImageFullName myimage:dev
```
---
## Configure
Validates and updates a connection string in `appsettings.json`. Accepts parameters like `Sql`, `Redis`, `AzureBlob`, `AppsettingsPath` (default is `./appsettings.json`).

### Parameters
- `Sql`: SQL Server connection string
- `Redis`: Redis connection string
- `AzureBlob`: Azure Blob Storage connection string
- `AppsettingsPath`: Path to appsettings.json file (optional, defaults to "./appsettings.json")

### Usage
```console
vc-build Configure -Sql "MsSql connection string" -Redis "Redis connection string" -AzureBlob "Container connection string"
```
---
## CloudEnvUpdate
Updates applications in the cloud. Accepts the following parameters: `CloudToken`, `Manifest`, `RoutesFile` (optional).

### Parameters
- `CloudToken`: VirtoCloud authentication token (required)
- `Manifest`: Path to application manifest file (required)
- `RoutesFile`: Path to routes file (optional)

### Usage
```console
vc-build CloudEnvUpdate -CloudToken <your token> -Manifest <path to application manifest>
vc-build CloudEnvUpdate -CloudToken <your token> -Manifest <path to application manifest> -RoutesFile <path to routes file>
```
---
## CloudEnvSetParameter
Updates parameters of the cloud environment. Accepts parameters like `CloudToken`, `EnvironmentName`, `HelmParameters` (Array), `Organization` (optional).

### Parameters
- `CloudToken`: VirtoCloud authentication token (required)
- `EnvironmentName`: Environment name (required)
- `HelmParameters`: Array of Helm parameters in key=value format (required)
- `Organization`: Organization name (optional)

### Usage
```console
vc-build CloudEnvSetParameter -CloudToken <your token> -EnvironmentName <environment name> -HelmParameters platform.config.paramname=somevalue123
```
---
## CloudEnvStatus
Waits for health and/or sync statuses of the environment. Accepts parameters like `CloudToken`, `EnvironmentName`, `HealthStatus`, `SyncStatus`.

### Parameters
- `CloudToken`: VirtoCloud authentication token (required)
- `EnvironmentName`: Environment name (required)
- `HealthStatus`: Expected health status to wait for
- `SyncStatus`: Expected sync status to wait for

### Usage
```console
vc-build CloudEnvStatus -CloudToken <your token> -EnvironmentName <environment name> -HealthStatus Healthy -SyncStatus Progressing
```
---
## CloudAuth
Saves a token for accessing the VirtoCloud portal, eliminating the need to use the `CloudToken` parameter with every call to targets in the Cloud group. Accepts parameters like `AzureAD` (optional), `CloudToken` (optional).

### Parameters
- `AzureAD`: Use Azure AD authentication (optional)
- `CloudToken`: Provide token directly (optional)

### Usage
```console
vc-build CloudAuth
vc-build CloudAuth -AzureAD
vc-build CloudAuth -CloudToken <token>
```
---
## CloudInit
Creates a new environment. Accepts the `ServicePlan` parameter to specify the service plan (default value is `F1`).

### Parameters
- `EnvironmentName`: New environment name (required)
- `ServicePlan`: Service plan name (optional, defaults to "F1")

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

### Parameters
- `EnvironmentName`: Environment name to restart (required)

### Usage
```console
vc-build CloudEnvRestart -EnvironmentName <EnvName>
```
---
## CloudEnvLogs
Shows environment logs.

### Parameters
- `EnvironmentName`: Environment name to show logs for (required)

### Usage
```console
vc-build CloudEnvLogs -EnvironmentName <EnvName>
```
---
## CloudDown
Deletes the environment. Accepts parameters like `Organization` (optional), `EnvironmentName` (required).

### Parameters
- `EnvironmentName`: Environment name to delete (required)
- `Organization`: Organization name (optional)

### Usage
```console
vc-build CloudDown -EnvironmentName <EnvName>
vc-build CloudDown -Organization <OrgName> -EnvironmentName <EnvName>
```
---
## CloudDeploy
Deploys a custom Docker image to an existing VirtoCommerce cloud environment. This target performs the complete container creation and deployment workflow:

1. **Prepares Docker context** (via `PrepareDockerContext` target)
2. **Builds Docker image** (via `BuildImage` target) 
3. **Pushes image to registry** (via `PushImage` target)
4. **Updates environment** with new Docker image coordinates

This is the complete "Container Create" workflow that builds and deploys a containerized VirtoCommerce application.

### Parameters
- `EnvironmentName`: Target environment name (required)
- `DockerUsername`: Docker Hub username (required for image naming and registry access)
- `DockerPassword`: Docker registry password (required for pushing images)
- `DockerRegistryUrl`: Docker registry URL (optional, defaults to Docker Hub)
- `DockerImageName`: Custom image name (optional, auto-generated from username/environment)
- `DockerImageTag`: Image tags (optional, timestamp-based if not provided)
- `CloudToken`: VirtoCloud authentication token (required)
- `Organization`: Organization name (optional)

### Usage
```console
vc-build CloudDeploy -EnvironmentName myenv -DockerUsername myuser -DockerPassword mypass
vc-build CloudDeploy -EnvironmentName prod -DockerUsername myorg -DockerPassword mypass -DockerImageTag v1.0.0
```
---
## CloudUp
Creates a new environment and deploys a custom Docker image to it. This target combines environment creation with the complete container creation and deployment workflow:

1. **Prepares Docker context** (via `PrepareDockerContext` target)
2. **Builds Docker image** (via `BuildImage` target)
3. **Pushes image to registry** (via `PushImage` target) 
4. **Creates new environment** (via `CloudInit` target)
5. **Configures environment** with the custom Docker image

This is the complete "Container Create and Deploy" workflow for new environments.

### Parameters
- `EnvironmentName`: New environment name (required)
- `DockerUsername`: Docker Hub username (required for image naming and registry access)
- `DockerPassword`: Docker registry password (required for pushing images)
- `DockerRegistryUrl`: Docker registry URL (optional, defaults to Docker Hub)
- `DockerImageName`: Custom image name (optional, auto-generated from username/environment)
- `DockerImageTag`: Image tags (optional, timestamp-based if not provided)
- `CloudToken`: VirtoCloud authentication token (required)
- `ServicePlan`: Cloud service plan (optional, defaults to F1)
- `ClusterName`: Target cluster name (optional)
- `DbProvider`: Database provider (optional)
- `DbName`: Database name (optional)

### Usage
```console
vc-build CloudUp -EnvironmentName newenv -DockerUsername myuser -DockerPassword mypass
vc-build CloudUp -EnvironmentName production -DockerUsername myorg -DockerPassword mypass -ServicePlan Standard -DockerImageTag v2.0.0
```
---
