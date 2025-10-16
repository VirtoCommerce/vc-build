# Container Create vc-build Command - Complete Documentation

## What is Container Create?

The "Container Create" functionality in vc-build refers to the comprehensive Docker containerization workflow that allows developers to build, configure, and deploy VirtoCommerce applications as Docker containers. This is not a single command, but rather a coordinated set of targets that work together to containerize VirtoCommerce platforms and modules.

## Key Container Creation Targets

### 1. `PrepareDockerContext`
**Purpose**: Prepares the complete Docker build environment for containerization.

**What it does**:
- Creates `artifacts/docker/` directory structure
- Downloads official VirtoCommerce Dockerfile and helper scripts
- Compiles and publishes the VirtoCommerce platform
- Builds and copies all discovered modules
- Configures Docker image names, tags, and build parameters

**Usage**:
```console
vc-build PrepareDockerContext -DockerUsername myuser -EnvironmentName myapp
```

### 2. `BuildImage` 
**Purpose**: Builds the Docker image using the prepared context.

**What it does**:
- Uses Docker to build a containerized image
- Includes platform, modules, and configuration
- Tags image with specified or auto-generated tags

**Usage**:
```console
vc-build BuildImage -DockerImageName myuser/myapp:latest
```

### 3. `PushImage`
**Purpose**: Pushes the built image to a Docker registry.

**What it does**:
- Authenticates with Docker registry
- Pushes the built image for deployment

**Usage**:
```console
vc-build PushImage -DockerImageName myuser/myapp:latest
```

### 4. `CloudUp` (Complete Container Create + Deploy - New Environment)
**Purpose**: Full container creation and deployment workflow for new environments.

**What it does**:
1. Prepares Docker context (`PrepareDockerContext`)
2. Builds Docker image (`BuildImage`)
3. Pushes to registry (`PushImage`)
4. Creates new cloud environment (`CloudInit`)
5. Configures environment with custom image

**Usage**:
```console
vc-build CloudUp -EnvironmentName mystore -DockerUsername mycompany -DockerPassword mypass
```

### 5. `CloudDeploy` (Complete Container Create + Deploy - Existing Environment)
**Purpose**: Full container creation and deployment workflow for existing environments.

**What it does**:
1. Prepares Docker context (`PrepareDockerContext`)
2. Builds Docker image (`BuildImage`)
3. Pushes to registry (`PushImage`)
4. Updates existing environment with new image

**Usage**:
```console
vc-build CloudDeploy -EnvironmentName mystore -DockerUsername mycompany -DockerPassword mypass
```

## Container Creation Workflows

### Quick Start - Most Common Use Cases

**1. Create and deploy containerized app to new environment:**
```console
vc-build CloudUp -EnvironmentName mystore -DockerUsername mycompany -DockerPassword mytoken
```

**2. Update existing environment with new container:**
```console
vc-build CloudDeploy -EnvironmentName mystore -DockerUsername mycompany -DockerPassword mytoken
```

### Advanced - Manual Container Building

**Step-by-step container creation:**
```console
# 1. Prepare the Docker build environment
vc-build PrepareDockerContext -DockerUsername mycompany -EnvironmentName mystore

# 2. Build the Docker image
vc-build BuildImage -DockerImageName mycompany/mystore:v1.0.0

# 3. Push to registry
vc-build PushImage -DockerImageName mycompany/mystore:v1.0.0
```

## Key Parameters

### Required Parameters
- **`DockerUsername`** - Docker registry username (required for image naming and registry access)
- **`DockerPassword`** - Docker registry password (required for pushing images)
- **`EnvironmentName`** - Target environment name (required for cloud deployments)

### Optional Parameters
- **`DockerRegistryUrl`** - Custom Docker registry URL (defaults to Docker Hub)
- **`DockerImageName`** - Custom Docker image name (auto-generated if not provided)
- **`DockerImageTag`** - Docker image tags (timestamp-based if not provided)
- **`DockerfilePath`** - Path to custom Dockerfile
- **`DockerBuildContextPath`** - Custom Docker build context path
- **`CloudToken`** - VirtoCloud authentication token (required for cloud deployments)
- **`ServicePlan`** - Cloud service plan (defaults to F1)

## What Gets Containerized

The container creation process includes:

1. **VirtoCommerce Platform**: The core e-commerce platform
2. **All Modules**: Discovered modules are built and included
3. **Configuration**: Proper appsettings and module configurations
4. **Dependencies**: All required runtime dependencies
5. **Scripts**: Helper scripts for container orchestration

## Technical Details

### Docker Context Structure
```
artifacts/docker/
├── Dockerfile          # Downloaded from VirtoCommerce repository
├── wait-for-it.sh      # Helper script for container orchestration
└── publish/            # Published platform and modules
    ├── appsettings.json
    ├── modules/         # All built modules
    └── app_data/
        └── modules/     # Module binaries
```

### Auto-Generated Names
- **Image Name**: `{DockerUsername}/{EnvironmentName.ToLowerCase()}`
- **Image Tag**: `MMddyyHHmmss` (timestamp-based)

### Dependencies
The container creation workflow automatically handles:
- Module dependency resolution
- Platform compilation and publishing
- Module building and packaging
- Docker image layering and optimization

## Examples

### Production Deployment
```console
# Deploy production-ready container with specific version tag
vc-build CloudUp -EnvironmentName production \
  -DockerUsername mycompany \
  -DockerPassword $DOCKER_TOKEN \
  -DockerImageTag v2.1.0 \
  -ServicePlan Standard
```

### Development Environment
```console
# Quick development environment setup
vc-build CloudDeploy -EnvironmentName dev-feature-x \
  -DockerUsername mycompany \
  -DockerPassword $DOCKER_TOKEN
```

### Custom Registry
```console
# Use private registry
vc-build CloudUp -EnvironmentName staging \
  -DockerUsername myuser \
  -DockerPassword $PRIVATE_TOKEN \
  -DockerRegistryUrl https://myregistry.company.com \
  -DockerImageName myregistry.company.com/virtocommerce/staging
```

## Summary

The Container Create functionality in vc-build provides a complete, automated solution for containerizing VirtoCommerce applications. It handles the complex process of building, configuring, and deploying Docker containers that include the VirtoCommerce platform, all modules, and proper configuration. The workflow is designed to work seamlessly with VirtoCloud for cloud deployments while also supporting custom registries and manual container management.