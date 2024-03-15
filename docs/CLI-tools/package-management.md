The `vc-build` tool provides a set of targets that allow you to easily install, uninstall, and update platform dependencies through running simple CLI commands.

## Install

```console
vc-build install (with no args)
vc-build install -platform -version <version>
vc-build install -module <module> -version <version>
vc-build install -module <module>:<version>
```

This command downloads and installs the platform or modules into the relevant folder with the versions transferred as command parameters or defined in `vc-package.json`.

### Example of the vc-package.json file:
```console
{
  "Sources": [
    {
      "Name": "GithubReleases",
      "ModuleSources": [
        "https://raw.githubusercontent.com/VirtoCommerce/vc-modules/master/modules_v3.json"
      ],
      "Modules": [
        {
          "Id": "VirtoCommerce.Assets",
          "Version": "3.200.0"
        }
      ]
    },
    {
      "Name": "GithubPrivateRepos",
      "Owner": "VirtoCommerce",
      "Modules":[
        {
          "Id": "vc-module-custom",
          "Version": "3.16.0"
        }
      ]
    },
    {
      "Name": "AzurePipelineArtifacts",
      "Organization": "<The name of the Azure DevOps organization.>",
      "Project": "<Project ID or project name>",
      "Modules": [
        {
          "Id": "vc-module-custom",
          "Version": "3.14.0",
          "Branch": "<Branch name>",
          "Definition": "<definition name with optional leading folder path, or the definition id>"
        }
      ]
    },
    {
      "Name": "AzureUniversalPackages",
      "Organization": "https://dev.azure.com/<YOUR_ORGANIZATION>",
      "Feed": "<FEED_NAME>",
      "Project": "<YOUR_PROJECT_NAME>",
      "Modules": [
        {
          "Id": "<PACKAGE_NAME>",
          "Version": "<PACKAGE_VERSION>"
        }
      ]
    },
    {
      "Name": "AzureBlob",
      "Modules": [
        {
          "BlobName": "CustomCompany.CustomModule1_3.200.0.zip"
        }
      ],
      "Container": "modules",
      "ServiceUri": "https://vcpsblob.blob.core.windows.net"
    },
    {
      "Name": "GitlabJobArtifacts",
      "Modules": [
        {
          "JobId": "3679907995",
          "ArtifactName": "artifacts/VirtoCommerce.Catalog_3.255.0.zip",
          "Id": "42920184"
        }
      ]
    },
    {
      "Name": "Local",
      "Modules": [
        {
          "Path": "C:/projects/vc/vc-module-saas/artifacts/VirtoCommerce.SaaS_3.214.0.zip",
          "Id": "OptionalForThisSource"
        },
        {
          "Path": "C:\\projects\\vc\\vc-module-catalog\\artifacts\\VirtoCommerce.Catalog"
        }
      ]
    }
  ],
  "ManifestVersion": "2.0",
  "PlatformVersion": "3.216.0"
}
```

The `vc-package.json` file is used to maintain the list of installed modules with their versions. This allows `vc-build` to easily restore the platform with modules on a different machine, such as a build server, without all those packages.

- `vc-build install (with no args)`

This target downloads and installs the platform and modules into the relevant folder with versions described in `vc-package.json`.
If `vc-package.json` is not found in the local folder, the command will by default download and install the latest stable bundle. If -Edge parameter has been used then this target will download the latest available platform and modules marked as `commerce`.

By default, the `install` target will install all modules listed as dependencies in `vc-package.json`.

### Examples:
```console
vc-build install
```

- `vc-build install -platform -version <version>`

This will fetch and install the platform with the specific version. If the platform with the specified version does not exist in the registry, the request will fail.
If no version is specified, the latest platform version will be installed.

### Examples:
```console
vc-build install -platform
vc-build install -platform -version 3.55.0
```

- `vc-build install -module -version <version>`

This will install the specified module version. The request will fail in case the version has not been published to the registry.
If no version is specified, the latest module version will be installed.
You can also install multiple modules with a single command by specifying multiple modules with their versions as arguments.

If the module to be installed has dependencies, their latest versions will be installed along with it.

This command also modifies the `vc-package.json` file with the installed dependencies after successful run.

### Examples:
```console
vc-build install -module VirtoCommerce.Cart
vc-build install -module VirtoCommerce.Cart -version 3.12.0
vc-build install -module VirtoCommerce.Cart:3.12.0 VirtoCommerce.Core:3.20.0
```

- `vc-build install -stable [-v <bundle name> -BundlesUrl <custom url to bundles json>]`

Install also supports working with bundles. A bundle file is a JSON object with key-value pairs that contains names of bundles and URLs to their manifests.

### Examples:
```console
vc-build install -stable # will install modules from the latest bundle
vc-build install -stable -v "2" # will install modules from the bundle named 2
vc-build install -stable -bundlesUrl https://somedomain.com/bundles.json # will use a custom URL for bundle search
```

## Update

```console
vc-build update (with no args)
vc-build update -edge
vc-build update -v 5
vc-build update -platform -version <version>
vc-build update -module <module> -version <version>
```
This command will update the platform and all modules linked to the version specified by `<version>`, respecting semver.
If `<version>` is not specified, the component will be updated to the latest version.
If no args are specified, the platform and all modules in the specified location will be updated.

This command also updates the installed dependency versions in the `vc-package.json` file.
Since the version 3.15.0 this target updates to stable bundles by default. If you want to update to the latest available versions you can add -Edge parameter.
You can specify the bundle to update your environment to specific versions using -v <bundle name> parameter.

### Examples:
```console
vc-build update
vc-build update -platform
vc-build update -platform -version 3.14.0
vc-build update -module VirtoCommerce.Cart
vc-build update -module VirtoCommerce.Cart -version 3.30.0
```

## Uninstall
```console
vc-build uninstall -module <module>
```
This uninstalls the module in question and completely removes all modules that depend on it.
It also removes uninstalled modules from your `vc-package.json` file.

### Examples:
```console
vc-build uninstall -module VirtoCommerce.Cart
```

## Configure
```console
vc-build configure -sql <sql connection string> -redis <redis connection string> -AzureBlob <container connection string> [-appsettingsPath ./appsettings.json]
```
This command will check given connection strings and update appsettings.json
