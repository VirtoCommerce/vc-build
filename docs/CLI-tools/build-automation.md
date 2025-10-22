The `vc-build` contains a set of targets that are designed to automate the various build scenarios for solutions based on VirtoCommerce and moreover, thanks to builder-server agnostic nature and cross-platform support, it can be integrated into any existing CI/CD pipeline with different build services, or used for local development scenarios.

![vc-build CLI](../media/cli-tools-2.png)


## Compile
```console
vc-build compile -configuration <Debug|Release>
```
Compile .NET Core solution.

### Parameters:

  - `-configuration <Debug|Release|CONFIGURATION>` - Defines the build configuration. The default configuration for build on build server is `Release`, on the local machine is `Debug`, but you can override the build configuration settings in your project.


## Test
```console
vc-build test (with no args)
```
Is used to compile the solution and runs all unit tests are discovered from the projects that are located in the solution folder and satisfied by this mask `*.Tests|*.Testing`.

Also, this target produces the test coverage and overall stats report.

### Example:
```console
vc-build test

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.
Passed!  - Failed:     0, Passed:    48, Skipped:     0, Total:    48, Duration: 4 s - VirtoCommerce.CatalogCsvImportModule.Tests.dll (netcoreapp3.1)

Calculating coverage result...
  Generating report 'c:\Projects\VirtoCommerce\V3\Modules\vc-module-catalog-csv-import\.tmp\coverage.xml'
+-------------------------------------------+--------+--------+--------+
| Module                                    | Line   | Branch | Method |
+-------------------------------------------+--------+--------+--------+
| VirtoCommerce.CatalogCsvImportModule.Core | 77,17% | 73,38% | 72,83% |
+-------------------------------------------+--------+--------+--------+
| VirtoCommerce.CatalogCsvImportModule.Data | 56,47% | 52,15% | 56,86% |
+-------------------------------------------+--------+--------+--------+

+---------+--------+--------+--------+
|         | Line   | Branch | Method |
+---------+--------+--------+--------+
| Total   | 65,23% | 60,64% | 66,66% |
+---------+--------+--------+--------+
| Average | 66,81% | 62,76% | 64,84% |
+---------+--------+--------+--------+


═══════════════════════════════════════
Target             Status      Duration
───────────────────────────────────────
Restore            Executed        0:04
Compile            Executed        0:08
Test               Executed        1:09
───────────────────────────────────────
Total                              1:22
═══════════════════════════════════════
```

## Pack

```console
vc-build pack -configuration <Debug|Release>
```

Builds the module solution and creates NuGet packages for all projects that have the `<IsPackable>` is set to `True` in the `.csproj` file.

The result of this target are NuGet packages that are stored by `artifacts` path of the module's root folder.
If execute this target for the platform solution the version is taken from `Directory.Build.props`.
If executes this target for a module solution, the version is taken from the module manifest file.

## PublishPackages

```console
vc-build publishPackages -source <SOURCE> -apiKey <API_KEY>
```
Pushes the NuGet packages are discovered in the `artifacts` folder to the server specified by `-source` parameter and publishes it.

### Example:
```console
vc-build publishPackages -source C:\local-nuget
```

### Parameters:

  - `-source <SOURCE>` - Specifies the server URL. NuGet identifies a UNC or local folder source and simply copies the file there instead of pushing it using HTTP. If `-source` is not set the default NuGet server `https://api.nuget.org/v3/index.json` will be used.
  - `-apiKey <API_KEY>` - The API key for the server.


## Compress

```console
vc-build compress -configuration <Debug|Release>
```
Pack the build artifacts into a distribution bundle zip that is ready to be transferred to a developer platform instance or published. Executing this command in a module root folder will put the resulting zip into the `artifact` folder.

This target excludes from the resulting zip all files whose names are enumerated from multiple sources:

- [global module.ignore](https://raw.githubusercontent.com/VirtoCommerce/vc-platform/dev/module.ignore) file that is managed by the VirtoCommerce team
- local `.moduleignore` file that is taken from the root folder of the module
- dependency-derived binary file names (.dll, .so) collected from dependency module zips

### Parameters (since 3.817)
- `ModulesCachePath`: Path for caching downloaded dependency zips. Defaults to `%USERPROFILE%/.vc-build/cache` or `VCBUILD_CACHE` env var if set.
- `PrereleasesBlobContainer`: Base URL to download prerelease module packages used to derive dependency ignore lists. Default: `https://vc3prerelease.blob.core.windows.net/packages/`.
- `DisableIgnoreDependencyFiles`: When true, disables automatic exclusion of dependency binary files from the archive.

### Example:

```console
vc-build compress -configuration Release
```

## Container Create (Docker Containerization)

```console
vc-build CloudUp -EnvironmentName <environment-name> -DockerUsername <docker-username> -DockerPassword <docker-password>
vc-build CloudDeploy -EnvironmentName <environment-name> -DockerUsername <docker-username> -DockerPassword <docker-password>
vc-build PrepareDockerContext -DockerUsername <docker-username> -EnvironmentName <environment-name>
```

The vc-build tool provides comprehensive Docker containerization capabilities for VirtoCommerce applications. The container creation functionality automates the entire process of building, configuring, and deploying Docker containers for VirtoCommerce platforms and modules.

### What Container Create Does

The container creation process performs these key operations:

1. **Prepares Docker Build Context** (`PrepareDockerContext` target):
   - Creates a clean Docker build directory structure in `artifacts/docker/`
   - Downloads the official VirtoCommerce Dockerfile and helper scripts
   - Compiles and publishes the VirtoCommerce platform to the Docker context
   - Builds and copies all discovered modules to the container
   - Configures Docker image names, tags, and build parameters

2. **Builds Docker Image** (`BuildImage` target):
   - Uses Docker to build a containerized image of the VirtoCommerce application
   - Includes the platform, all modules, and proper configuration
   - Tags the image with specified or auto-generated tags

3. **Pushes to Registry** (`PushImage` target):
   - Authenticates with the Docker registry
   - Pushes the built image to the registry for deployment

4. **Deploys to Cloud** (`CloudUp`/`CloudDeploy` targets):
   - Creates or updates VirtoCloud environments
   - Configures the environment to use the custom Docker image
   - Handles all cloud deployment automation

### Container Creation Workflows

**Full Container Create and Deploy (New Environment):**
```console
vc-build CloudUp -EnvironmentName mystore -DockerUsername mycompany -DockerPassword mypass
```

**Container Create and Deploy (Existing Environment):**
```console
vc-build CloudDeploy -EnvironmentName mystore -DockerUsername mycompany -DockerPassword mypass
```

**Manual Container Creation Steps:**
```console
# 1. Prepare Docker context
vc-build PrepareDockerContext -DockerUsername mycompany -EnvironmentName mystore

# 2. Build Docker image  
vc-build BuildImage -DockerImageName mycompany/mystore:latest

# 3. Push to registry
vc-build PushImage -DockerImageName mycompany/mystore:latest
```

### Parameters:

- **`EnvironmentName`** - Name of the target environment (required for cloud deployments)
- **`DockerUsername`** - Docker registry username (required)
- **`DockerPassword`** - Docker registry password (required for pushing images)
- **`DockerRegistryUrl`** - Custom Docker registry URL (optional, defaults to Docker Hub)
- **`DockerImageName`** - Custom Docker image name (optional, auto-generated if not provided)
- **`DockerImageTag`** - Docker image tags (optional, timestamp-based if not provided)
- **`DockerfilePath`** - Path to custom Dockerfile (optional)
- **`DockerBuildContextPath`** - Custom Docker build context path (optional)
- **`CloudToken`** - VirtoCloud authentication token (required for cloud deployments)
- **`ServicePlan`** - Cloud service plan (optional, defaults to F1)

### Example:

```console
# Create and deploy a containerized store to a new cloud environment
vc-build CloudUp -EnvironmentName mystore -DockerUsername mycompany -DockerPassword mytoken

# Deploy updated container to existing environment  
vc-build CloudDeploy -EnvironmentName mystore -DockerUsername mycompany -DockerPassword mytoken

# Build custom container with specific image name
vc-build PrepareDockerContext -DockerUsername mycompany -EnvironmentName mystore
vc-build BuildImage -DockerImageName myregistry.com/mycompany/mystore:v1.0.0
```

Console output:

```console
═══════════════════════════════════════
Target             Status      Duration
───────────────────────────────────────
Clean              Executed        0:00
Restore            Executed        0:07
Compile            Executed        0:06
WebPackBuild       Executed        0:00
Test               Executed        0:05
Publish            Executed        0:01
Compress           Executed        0:01
───────────────────────────────────────
Total                              0:23
═══════════════════════════════════════
```
