using System.Collections.Generic;
using PlatformTools;
using VirtoCommerce.Platform.Core.Modularity;

namespace VirtoCommerce.Build;

internal class Error(string messageTemplate, params object[] propertyValues)
{
    public string MessageTemplate { get; } = messageTemplate;
    public object[] PropertyValues { get; } = propertyValues;

    public static Error MissingManifestDependency(string moduleId, string version)
    {
        return new Error("Missing dependency in module.manifest. Module {ModuleId}, version: {Version}",
            moduleId, version);
    }

    public static Error PlatformMultipleVersions(IList<string> versions, IList<string> projects)
    {
        return new Error("Platform has multiple versions {Versions} in projects {Projects}",
            versions, projects);
    }

    public static Error ModuleMultipleVersions(string moduleId, IList<string> versions, IList<string> projects)
    {
        return new Error("Module {ModuleId} has multiple versions {Versions} in projects {Projects}",
            moduleId, versions, projects);
    }

    public static Error PlatformVersionMismatch(string manifestVersion, PackageItem package)
    {
        return new Error(
            "Platform version mismatch. Manifest version: {ManifestVersion}, package name: {PackageName}, package version: {PackageVersion}, project name: {ProjectName}",
            manifestVersion, package.Name, package.Version, package.ProjectName);
    }

    public static Error ModuleVersionMismatch(ManifestDependency dependency, PackageItem package)
    {
        return new Error(
            "Module version mismatch. Module: {DependencyId}, manifest version: {DependencyVersion}, package name: {PackageName}, package version: {PackageVersion}, project name: {ProjectName}",
            dependency.Id, dependency.Version, package.Name, package.Version, package.ProjectName);
    }

    public static Error InvalidVersionFormat(PackageItem package) =>
        new(
            "Invalid version format. Package name: {PackageName}, package version: {PackageVersion}, project name: {ProjectName}",
            package.Name, package.Version, package.ProjectName
        );
}
