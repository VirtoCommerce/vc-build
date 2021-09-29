using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Nuke.Common;
using Nuke.Common.ProjectModel;
using Nuke.Common.Utilities;
using PlatformTools;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Modularity;

namespace VirtoCommerce.Build
{
    internal partial class Build
    {
        private Regex _moduleNameRegEx = new Regex(@"(VirtoCommerce.+)Module", RegexOptions.Compiled);

        public Target MatchVersions => _ => _
            .Executes(() =>
            {
                var allPackages = new List<PackageItem>();
                var allProjects = Solution.GetProjects("*");

                foreach (var project in allProjects)
                {
                    var msBuildProject = project.GetMSBuildProject();

                    // get all VirtoCommerce references
                    var packagesInfo = msBuildProject.Items
                        .Where(x => x.ItemType == "PackageReference"
                            && (x.EvaluatedInclude.StartsWith("VirtoCommerce.Platform.") || _moduleNameRegEx.IsMatch(x.EvaluatedInclude)))
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

                    allPackages.AddRange(packagesInfo);
                }

                var errors = new List<string>();

                // check match between manifest platform version and platform packages
                var platformErrors = allPackages
                    .Where(package => package.IsPlatformPackage
                        && SemanticVersion.Parse(package.Version) != SemanticVersion.Parse(ModuleManifest.PlatformVersion))
                    .Select(x =>
                        $"Mismatched platform dependency version found. Platform version: {ModuleManifest.PlatformVersion}, Platform package name: {x.Name}, platform package version: {x.Version}, project name: {x.ProjectName}");

                errors.AddRange(platformErrors);

                if (ModuleManifest.Dependencies != null)
                {
                    // check dependencies for module packages versions mismatch
                    foreach (var dependency in ModuleManifest.Dependencies)
                    {
                        var dependencyVersionErrors = allPackages
                            .Where(package => !package.IsPlatformPackage
                                && HasNameMatch(package.Name, dependency.Id)
                                && SemanticVersion.Parse(package.Version) != SemanticVersion.Parse(dependency.Version))
                            .Select(package =>
                                $"Mismatched dependency version found. Dependency: {dependency.Id}, version: {dependency.Version}, Project package version: {package.Version}, project name: {package.ProjectName}");

                        errors.AddRange(dependencyVersionErrors);
                    }

                    // check project packages for missed dependency in manifest
                    foreach (var packageGroup in allPackages.Where(x => !x.IsPlatformPackage).GroupBy(x => x.Name))
                    {
                        if (!ModuleManifest.Dependencies.Any(dependency => HasNameMatch(packageGroup.Key, dependency.Id)))
                        {
                            errors.Add($"Dependency in module.manifest is missing. Package name: {packageGroup.Key}");
                        }
                    }
                }

                if (errors.Any())
                {
                    ControlFlow.Fail(errors.Join(Environment.NewLine));
                }
            });

        private bool HasNameMatch(string packageName, string dependencyName)
        {
            var match = _moduleNameRegEx.Match(packageName);
            return match.Groups.Values.Any(x => x.Value == dependencyName);
        }
    }
}
