using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Nuke.Common;
using Nuke.Common.ProjectModel;
using PlatformTools;
using Serilog;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Build
{
    internal partial class Build
    {
        [GeneratedRegex(@"(VirtoCommerce.+)Module", RegexOptions.Compiled)]
        private static partial Regex ModuleNameRegEx();

        public Target MatchVersions => _ => _
            .Before(Clean, Restore, WebPackBuild, BuildCustomApp, Test, Publish)
            .OnlyWhenDynamic(() => IsModule)
            .Executes(() =>
             {
                 var allPackages = new List<PackageItem>();
                 var allProjects = Solution.AllProjects;
                 foreach (var project in allProjects)
                 {
                     var packagesInfo = GetProjectPackages(project);
                     allPackages.AddRange(packagesInfo);
                 }

                 var errors = ValidateModuleDependencies(allPackages);

                 foreach (var error in errors)
                 {
                     Log.Error(error);
                 }

                 if (errors.Count > 0)
                 {
                     Assert.Fail("Dependency version mismatch detected. Please review the log for details and resolve all inconsistencies before proceeding.");
                 }
             });



        private static List<string> ValidateModuleDependencies(IList<PackageItem> allPackages)
        {
            var errors = new List<string>();

            var platformErrors = ValidatePlatformVersion(allPackages);
            errors.AddRange(platformErrors);

            var dependencyVersionErrors = ValidateModuleDependenciesVersions(allPackages);
            errors.AddRange(dependencyVersionErrors);

            var missedDependenciesErrors = ValidateForMissedDependencies(allPackages);
            errors.AddRange(missedDependenciesErrors);

            var consistencyErrors = ValidatePlatformPackagesConsistency(allPackages);
            errors.AddRange(consistencyErrors);

            return errors;
        }

        private static List<string> ValidatePlatformPackagesConsistency(IList<PackageItem> packages)
        {
            List<string> errors = [];
            var platformPackagesVersions = packages.Where(p => p.IsPlatformPackage).Select(p => p.Version).Distinct().ToList();
            if (platformPackagesVersions.Count > 1)
            {
                var projects = packages.Where(p => p.IsPlatformPackage).Select(p => p.ProjectName).Distinct().ToList();
                errors.Add($"Platform packages have multiple versions: {string.Join(", ", platformPackagesVersions)} in projects: {string.Join(", ", projects)}");
            }
            return errors;
        }

        /// <summary>
        /// Get list of VirtoCommerce packages (platform and module)
        /// </summary>
        private static IEnumerable<PackageItem> GetProjectPackages(Project project)
        {
            var msBuildProject = project.GetMSBuildProject();

            // find all VirtoCommerce references
            return msBuildProject.Items
                .Where(x => x.ItemType == "PackageReference"
                    && (x.EvaluatedInclude.StartsWith("VirtoCommerce.Platform.") || ModuleNameRegEx().IsMatch(x.EvaluatedInclude)))
                .Select(x =>
                {
                    var versionMetadata = x.Metadata.FirstOrDefault(x => x.Name == "Version");
                    if (versionMetadata == null)
                    {
                        return null;
                    }

                    var name = x.EvaluatedInclude;

                    return new PackageItem
                    {
                        Name = name,
                        Version = versionMetadata.EvaluatedValue,
                        ProjectName = project.Name,
                        IsPlatformPackage = name.StartsWith("VirtoCommerce.Platform.")
                    };
                })
                .Where(x => x != null);
        }

        /// <summary>
        /// Check match between manifest platform version and platform packages
        /// </summary>
        private static List<string> ValidatePlatformVersion(IList<PackageItem> packages)
        {
            return packages
                .Where(package => package.IsPlatformPackage && SemanticVersion.Parse(package.Version) != SemanticVersion.Parse(ModuleManifest.PlatformVersion))
                .Select(x =>
                        $"Mismatched platform dependency version found. Platform version: {ModuleManifest.PlatformVersion}, Platform package name: {x.Name}, platform package version: {x.Version}, project name: {x.ProjectName}")
                .ToList();
        }

        /// <summary>
        /// Check dependencies for module packages versions mismatch
        /// </summary>
        private static List<string> ValidateModuleDependenciesVersions(IList<PackageItem> packages)
        {
            var result = new List<string>();

            if (ModuleManifest.Dependencies.IsNullOrEmpty())
            {
                return result;
            }

            foreach (var dependency in ModuleManifest.Dependencies)
            {
                var errors = packages
                    .Where(package => !package.IsPlatformPackage
                        && HasNameMatch(package.Name, dependency.Id)
                        && SemanticVersion.Parse(package.Version) != SemanticVersion.Parse(dependency.Version))
                    .Select(package =>
                        $"Mismatched dependency version found. Dependency: {dependency.Id}, version: {dependency.Version}, Project package version: {package.Version}, project name: {package.ProjectName}");

                result.AddRange(errors);
            }

            return result;
        }

        /// <summary>
        /// Check project packages for missed dependency in manifest
        /// </summary>
        private static List<string> ValidateForMissedDependencies(IList<PackageItem> packages)
        {
            var result = new List<string>();

            foreach (var packageGroupKey in packages.Where(x => !x.IsPlatformPackage).GroupBy(x => x.Name).Select(packageGroup => packageGroup.Key))
            {
                if (!ModuleManifest.Dependencies.Any(dependency => HasNameMatch(packageGroupKey, dependency.Id)))
                {
                    result.Add($"Dependency in module.manifest is missing. Package name: {packageGroupKey}");
                }
            }

            return result;
        }

        private static bool HasNameMatch(string packageName, string dependencyName)
        {
            var match = ModuleNameRegEx().Match(packageName);
            return match.Groups.Values.Any(x => x.Value == dependencyName);
        }
    }
}
