using System;
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

        [GeneratedRegex(@"^(?<ModuleId>VirtoCommerce\.[^.]+?)(Module)?\.")]
        private static partial Regex ModuleNameFromDependencyRegEx();

        [Parameter("Disable MatchVersion target")]
        public bool DisableMatchVersions { get; set; }

        public Target MatchVersions => _ => _
            .Before(Clean, Restore, WebPackBuild, BuildCustomApp, Test, Publish)
            .OnlyWhenDynamic(() => IsModule && !DisableMatchVersions)
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
                     Log.Error(error.MessageTemplate, error.PropertyValues);
                 }

                 if (errors.Count > 0)
                 {
                     Assert.Fail("Dependency version mismatch detected. Please review the log for details and resolve all inconsistencies before proceeding.");
                 }
             });

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

        private static List<Error> ValidateModuleDependencies(IList<PackageItem> allPackages)
        {
            var errors = new List<Error>();

            ValidateMissingDependencies(allPackages, errors);
            ValidatePlatformVersionMismatch(allPackages, errors);
            ValidateModuleVersionsMismatch(allPackages, errors);
            ValidatePlatformPackagesConsistency(allPackages, errors);
            ValidateModulePackagesConsistency(allPackages, errors);

            return errors;
        }

        /// <summary>
        /// Check project packages for missed dependency in manifest
        /// </summary>
        private static void ValidateMissingDependencies(IList<PackageItem> packages, List<Error> errors)
        {
            foreach (var packageGroup in packages
                         .Where(x => !x.IsPlatformPackage)
                         .GroupBy(x => x.Name)
                         .Where(g => !ModuleManifest.Dependencies?.Any(dependency => HasNameMatch(g.Key, dependency.Id)) ?? true))
            {
                errors.Add(Error.MissingManifestDependency(packageGroup.Key, packageGroup.First().Version));
            }
        }

        /// <summary>
        /// Check match between manifest platform version and platform packages
        /// </summary>
        private static void ValidatePlatformVersionMismatch(IList<PackageItem> packages, List<Error> errors)
        {
            foreach (var package in packages
                         .Where(x => x.IsPlatformPackage))
            {
                SemanticVersion packageVersion;
                try
                {
                    packageVersion = SemanticVersion.Parse(package.Version);
                }
                catch (FormatException)
                {
                    errors.Add(Error.InvalidVersionFormat(package));
                    continue;
                }
                SemanticVersion moduleManifestPlatformVersion;
                try
                {
                    moduleManifestPlatformVersion = SemanticVersion.Parse(ModuleManifest.PlatformVersion);
                }
                catch (FormatException)
                {
                    errors.Add(new Error("Platform version is invalid in the module manifest: {ManifestPlatformVersion}", ModuleManifest.PlatformVersion));
                    continue;
                }

                if (packageVersion != moduleManifestPlatformVersion)
                {
                    errors.Add(Error.PlatformVersionMismatch(ModuleManifest.PlatformVersion, package));
                }
            }
        }

        /// <summary>
        /// Check dependencies for module packages versions mismatch
        /// </summary>
        private static void ValidateModuleVersionsMismatch(IList<PackageItem> packages, List<Error> errors)
        {
            if (ModuleManifest.Dependencies.IsNullOrEmpty())
            {
                return;
            }

            foreach (var dependency in ModuleManifest.Dependencies)
            {
                foreach (var package in packages
                             .Where(x => !x.IsPlatformPackage &&
                                         HasNameMatch(x.Name, dependency.Id) &&
                                         SemanticVersion.Parse(x.Version) != SemanticVersion.Parse(dependency.Version)))
                {
                    errors.Add(Error.ModuleVersionMismatch(dependency, package));
                }
            }
        }

        private static void ValidatePlatformPackagesConsistency(IList<PackageItem> packages, List<Error> errors)
        {
            var platformPackagesVersions = packages.Where(p => p.IsPlatformPackage).Select(p => p.Version).Distinct().ToList();
            if (platformPackagesVersions.Count > 1)
            {
                var projects = packages.Where(p => p.IsPlatformPackage).Select(p => p.ProjectName).Distinct().ToList();
                errors.Add(Error.PlatformMultipleVersions(platformPackagesVersions, projects));
            }
        }

        private static void ValidateModulePackagesConsistency(IList<PackageItem> allPackages, List<Error> errors)
        {
            var groups = allPackages.Where(p => !p.IsPlatformPackage && ModuleNameRegEx().IsMatch(p.Name)).GroupBy(p => GetDependencyName(p.Name) ?? p.Name);
            foreach (var packageGroup in groups)
            {
                var versions = packageGroup.Select(p => p.Version).Distinct().ToList();
                if (versions.Count > 1)
                {
                    var projects = packageGroup.Select(p => p.ProjectName).Distinct().ToList();
                    errors.Add(Error.ModuleMultipleVersions(packageGroup.Key, versions, projects));
                }
            }
        }

        private static bool HasNameMatch(string packageName, string dependencyName)
        {
            var match = ModuleNameRegEx().Match(packageName);
            return match.Groups.Values.Any(x => x.Value == dependencyName);
        }

        private static string GetDependencyName(string packageName)
        {
            var match = ModuleNameFromDependencyRegEx().Match(packageName);
            return match.Success ? match.Groups["ModuleId"].Value : null;
        }
    }
}
