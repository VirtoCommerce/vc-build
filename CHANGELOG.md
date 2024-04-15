# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
## [3.800.0] - 2024-04-15
### Features
- Updated to Net 8.

### Added
- Added Telemetry.
- Added MakeLatest parameter.
- Added versions of vc-build and os to the telemetry.
- Increased default http timeout to 180 seconds.
- Added switch for user-friendly choose auth provider.
- Added local modules source.

### Bug Fixes
- Replaced deprecated sonar.login parameter.
- Implemented minor fixes for CompleteRelease target.
- Fixed relative to absolute path conversion.
- Corrected path escaping in CompleteRelease target.
- Implemented minor fix.
- Fixed path escaping in CompleteRelease target.
- Skipped node_modules in Backup target.
- Adjusted message about appsettings backup warning level.
- Removed obsolete target.
- Checked if module is in symlink on update.

### Other
- Added license, readme, icon, and fixed copyright.
- Removed obsolete targets.
- Improved comments for skipped targets.
- Updated targets.md.


## [3.16.0] - 2023-08-24
### Added
- Organization parameter for SetEnvParameter target
### Fixed
- Ignoring of PackageManifestPath
- Trigger of Rollback target
- Directory names for modules downloaded from Azure blobs
- failure of backup target when directory is empty

## [3.15.0] - 2023-08-08
### Added
- Support of bundles in the Update target
- Log message for modules from github private repos
### Fixed
- Ignoring of failures in modules installation
- Failure if modules directory doesn't exists for AzureBlob source
### Changed
- Update targe updates to the latest stable bundle by default

## [3.14.0] - 2023-04-17
### Changed
- Updated dependecies

## [3.13.0] - 2023-04-03
### Added
- PlatformAssetUrl to the package manifest
- non-zero exit code if WaitForStatus didn't obtain the expected status
### Fixed
- Use PackageOutputPath instead of SetOutputDirectory to fix error NETSDK1194: The "--output" option isn't supported when building a solution.
- Minor fixes

## [3.12.0] - 2023-02-16
### Added
- Support for modules installation from GitLab Job Artifacts
- Targets for custom app build
### Fixed
- Failures of Test target when there is spaces in the path of test project

## [3.11.0] - 2023-01-19
### Added
- Error message when WebProject is not found
- PlatformAssetUrl parameter
- Delay in WaitForStatus target
### Fixed
- Condition to run WebPackBuild target
- Condition to run Backup target
- Decreased the Cyclomatic Complexity of ChangeProjectVersion
- Failure when there is no package.json
- stderr messages on docker login

## [3.10.0] - 2022-12-20
### Added
- SetEnvParameter and WaitForStatus targets
- Log output for module sources
- Rollback if modules installation failed
### Fixed
- CSC warning CS8785: Generator 'RazorSourceGenerator' failed to generate source.

## [3.5.0] - 2022-07-21
### Added
- HttpTimeout parameter for overriding default http timeout
### Changed
- Default timeout of HttpTasks from 5 to 15 sec.

## [3.4.0] - 2022-05-26
### Added
- UpdateCloudEnvironment target

## [3.3.0] - 2022-04-26
### Added
- Support for traffic rules
- Ability to forcibly keep specified files
- Support for Azure Blob modules source
### Fixed
- IsTheme parameter

## [3.2.0] - 2022-03-31
### Added
- Bundles support
- WaitFor target
### Fixed
- indents for module.manifest serialization

## [3.1.0] - 2022-02-25
### Added
- SetHelmParameter Target
- Support for protected parameters
- Support for advanced section

## [3.0.0] - 2022-01-28
### Changed
- Target framework is net6
- Help command is the default command
### Fixed
- Help command
- Minor fixes

## [2.5.0] - 2022-01-21
### Added
- Support for GitHub private repositories
- Ingress hostnames support
- Support for additional argo configurations

## [2.2.0] - 2021-12-14
### Added
- Ability to install modules from Azure Pipeline Artifacts
- Ability to install modules from Azure Universal Packages
- Targets for build and publish Docker Images
- Target for update specs of Argo Applications
### Changed
- Target Framework updated to .net6
### Fixed
- Removing of temporary files after modules and platform installation
- Minor fixes

## [2.1.0] - 2021-10-01
### Added
- MatchVersions target
- DefaultProject parameter
- Ability to update only platform
- help target
### Changed
- appsettings.json doesn't reset on update
### Fixed
- User input in QuickRelease target
- Building of sample projects

## [2.0.0] - 2021-08-05
### Changed
- vc-build was updated to .Net 5
- Documentation was moved from vc-platform repo
- Updated dependencies
### Added
- Force parameter
- Case insensitivity for module installation
- ClearTemp Target
- Removing of zip files after modules installation
- _build project search
### Fixed
- Modules installation
- FileNotFoundException in ClearTemp target
- Properties of nugets of modules
- Check if ModuleManifestPath is null
- QuickRelease fails when the release branch is already exists

## [1.7.5] - 2021-05-27
### Added
- -SkipDependencySolving parameter

## [1.7.4] - 2021-05-26
### Fixed
- sonar.coverageReportPaths wasn't passing

## [1.7.3] - 2021-05-26
### Changed
- Updated help
- Extended install target
- Removed coverlet.console dependency
### Fixed
- Coverage report generation method

## [1.7.0] - 2021-04-21
### Added
- Targets: Init, Install, InstallPlatform, InstallModules, Uninstall
- -? parameter which shows help

## [1.6.0] - 2021-04-09
### Added
- InitPlatform target

## [1.5.2] - 2021-04-05
### Added
- Publication of package symbols
### Fixed
- Decreased the degree of parallelism on symbols publication
- Fixed typos
- Branch of sources for github release

## [1.5.1] - 2021-01-25
### Fixed
- Typo in name of target IncrementPatch

## [1.5.0] - 2021-01-22
## Added
- QuickRelease target works with themes now

## [1.4.19] - 2020-12-28
### Added
- VCBUILD_DISABLE_RELEASE_APPROVAL environment variable to disable release approval
### Changed
- Updated Nuke.Common dependency
- github-release tool replaced by Octokit

## [1.4.16] - 2020-10-12
### Added
- Targets: GetManifestGit, UpdateManifest, PublishManifestGit

## [1.4.15] - 2020-10-06
### Added
- PushChanges parameter
- ModulesJsonRepoUrl parameter
### Fixed
- Search of WebProject
- Git Log filter

## [1.4.7] - 2020-09-17
### Added
- Parameters for Sonar PullRequests Analysis
### Fixed
- Default value for SwaggerValidatorUri
- Parameters for Sonar PullRequests Decoration

## [1.4.3] - 2020-09-07
### Added
- SonarBranchName parameter
### Fixed
- Search of Web-projects
- sonar.branch.name parameter changed to sonar.branch
- NukeSpecificationFiles exclusions

## [1.4.0] - 2020-08-31
### Added
 - GrabMigrator Target

## [1.3.7] - 2020-08-25
### Fixed
- Directory.Build.props search
- Search of Web-projects
- Search of Tests Assemblies and Projects
- Swashbuckle.AspNetCore.Cli dependency resolving
- SonarQubeStart target for using it with SonarCloud

## [1.3.1] - 2020-07-31
### Fixed
- Repository index updating
- Parameters for SonarScanner to use it with new versions of SonaQube

## [1.3.0] - 2020-07-20
### Added
- Targets: StartRelease, CompleteRelease, QuickRelease, StartHotfix, CompleteHotfix, IncrementMinor, IncrementPatch, ChangeVersion
### Changed
- OnTargetStart event handler was replaced with ChangeVersion Target
### Fixed
- Hash and Sources of github releases (VP-3628)

## [1.2.0] - 2020-07-07
### Changed
- Opencover is replaced with Coverlet

## [1.1.1] - 2020-06-26
### Fixed
- An issue when there is no Directory.Build.Props file

## [1.1.0] - 2020-06-25
### Changed
- Parameters VersionTag and CustomTagSuffix were replaced with CustomVersionSuffix

## [1.0.2] - 2020-06-18
### Added
- Support for prereleases in modules manifest
### Fixed
- Updating of modules properties in the manifest

## [1.0.1] - 2020-06-11
### Fixed
- Artifact name of Storefront
- Name of artifacts directory of Modules
### Added
- .nuke file will be created if it doesn't exist and there is solution file in current directory
- ArtifactsDirectory Parameter to customize artifact directory

## [1.0.0] - 2020-06-05
### Fixed
- An issue with Storefront's project search
- Modules version is getting from Project properties now
### Added
- CustomTagSuffix parameter
### Changed
- Updated dependencies

## [3.0.0-beta0010] - 2020-04-24
### Fixed
- An issue with opencover that fails when it runs not on build server.
### Changed
- NUKE Execution Engine updated to 0.24.10
- Removed GitVersion dependency
- The Version is going to be got from Project properties now instead of GitVersion

## [3.0.0-beta0009] - 2020-04-01
### Added
- Custom logger for DotnetTasks
- ValidateSwaggerSchema Target
- Support for Pull Request in SonarQubeStart Target
### Fixed
- An issue with dependencies filter in Compress Target
- An issue with packaging vc-build with 3rd party tools https://github.com/nuke-build/nuke/issues/437
### Changed
- NUKE Execution Engine updated to 0.24.7
- GitVersion updated to 5.2.4
- Virtocommerce.Platform dependency changed from ProjectReference to PackageReference

## [3.0.0-beta0008] - 2020-01-28
### Added
- Code Coverage
### Changed
- Updated dependencies
### Fixed
- Fixed an issues with GitVersion and Nuke

## [3.0.0-beta0007] - 2020-01-13
### Added
- SwaggerValidationUrl parameter
- changelog
### Changed
- Target Framework updated to 3.1
- SwaggerValidation now uses validator.swagger.io
- Nuke.Common dependency updated to 0.23.6
