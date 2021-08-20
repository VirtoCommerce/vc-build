
The VirtoCommerce Global Tool (vc-build) is the official CLI [.NET Core GlobalTool](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools) that helps you build, test and deploy releases, create and push NuGet packages, provide package management for projects based on VirtoCommerce, and automate common DevOps tasks. It is powered by [nuke.build](https://nuke.build/) - a cross-platform build automation system with C# DSL, that provides an approach to embrace existing IDE tooling and state where everyone in a team can manage and change the build scenarios. This allows writing build scenarios in C# and debugging them in Visual Studio. Also, along with cross-platform support, it was the best choice for us to build our own build automation solution on top of this project.

Check out [the project source code](https://github.com/VirtoCommerce/vc-build) for the implementation details.

![vc-build CLI](../media/cli-tools-1.png)

## The key features:

[Build automation](./build-automation.md) 

- build and bundle a module project (both managed and scripted parts)
- discover and run all the unit tests from the solution
- create and publish NuGet packages for projects from your solution, this can be helpful if you intend to re-use a module's logic in another project, you will be able to quickly publish the needed version as a NuGet package. In the private or public NuGet registry
- include targets that allow performing various additional quality checks such as static code analysis (currently we support out-of-the-box integration with SonarCloud)

[Packages management](./package-management.md) 

- install, update, uninstall modules 
- install and update a platform application
- prepare backend package with specific versions of the platform and modules from the manifest file
  
[The platform cold start optimization and data migration (WIP)](./cold-start-and-data-migration.md)

- platform start optimization (slow run on Azure case)
- get idempotent SQL scripts for all modules EF migrations with the ability to apply them in a specific order without installed platform and source code (helpful for migration from VirtoCommerce platform version 2 (latest) to version 3)

## Before you start
Before you start using `VirtoCommerce.GlobalTool`, install the following in order to use all its functionality:

- .NET SDK 5.x
- Node.js 12.x
- Git SCM

## Installation
Run this command to install `VirtoCommerce.GlobalTool` on your machine:
```console

dotnet tool install VirtoCommerce.GlobalTool  -g

```

## Updating 
Run this command to update `VirtoCommerce.GlobalTool` to the latest version:

```console

dotnet tool update VirtoCommerce.GlobalTool -g

```

## Getting started
To use `VirtoCommerce.GlobalTool` by invoke the tool run the following command: `vc-build`

To get the list of all targets:
```console

vc-build help

```
Command output:

```console
NUKE Execution Engine version 5.0.2 (Windows,.NETCoreApp,Version=v2.1)

Target with name 'help' does not exist. Available targets are:
  - ChangeVersion
  - Clean
  - ClearTemp
  - Compile
  - CompleteHotfix
  - CompleteRelease
  - Compress
  - GetManifestGit
  - GrabMigrator
  - IncrementMinor
  - IncrementPatch
  - Init
  - InitPlatform
  - Install
  - InstallModules
  - InstallPlatform
  - MassPullAndBuild
  - Pack
  - Publish
  - PublishManifestGit
  - PublishModuleManifest
  - PublishPackages
  - QuickRelease
  - Release
  - Restore
  - SonarQubeEnd
  - SonarQubeStart
  - StartAnalyzer
  - StartHotfix
  - StartRelease
  - SwaggerValidation
  - Test
  - Uninstall
  - Update
  - UpdateManifest
  - ValidateSwaggerSchema
  - WebPackBuild
```
