using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Exceptions;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.DistributedLock;
using VirtoCommerce.Platform.Modules;

namespace PlatformTools.Modules
{
    public class LocalCatalog : LocalStorageModuleCatalog
    {
        private readonly LocalStorageModuleCatalogOptions _options;
        private readonly ILogger<LocalStorageModuleCatalog> _logger;
        private readonly IInternalDistributedLockService _distributedLockProvider;
        private readonly string _discoveryPath;
        public LocalCatalog(IOptions<LocalStorageModuleCatalogOptions> options, IInternalDistributedLockService distributedLockProvider, ILogger<LocalStorageModuleCatalog> logger) :
            base(options, distributedLockProvider, logger)
        {
            _options = options.Value;
            _logger = logger;
            _distributedLockProvider = distributedLockProvider;
            _discoveryPath = _options.DiscoveryPath;
        }

        protected override void InnerLoad()
        {
            if (string.IsNullOrEmpty(_options.ProbingPath))
            {
                throw new InvalidOperationException("The ProbingPath cannot contain a null value or be empty");
            }

            if (string.IsNullOrEmpty(_options.DiscoveryPath))
            {
                throw new InvalidOperationException("The DiscoveryPath cannot contain a null value or be empty");
            }

            var manifests = GetModuleManifests();

            var needToCopyAssemblies = _options.RefreshProbingFolderOnStart;

            if (!Directory.Exists(_options.ProbingPath))
            {
                needToCopyAssemblies = true; // Force to refresh assemblies anyway, even if RefreshProbeFolderOnStart set to false, because the probing path is absent
                Directory.CreateDirectory(_options.ProbingPath);
            }

            if (needToCopyAssemblies)
            {
                CopyAssembliesSynchronized(manifests);
            }

            foreach (var pair in manifests)
            {
                var manifest = pair.Value;

                var moduleInfo = AbstractTypeFactory<ManifestModuleInfo>.TryCreateInstance();
                moduleInfo.LoadFromManifest(manifest);
                moduleInfo.FullPhysicalPath = Path.GetDirectoryName(pair.Key);

                // Modules without assembly file don't need initialization
                if (string.IsNullOrEmpty(manifest.AssemblyFile))
                {
                    moduleInfo.State = ModuleState.Initialized;
                }
                else
                {
                    //Set module assembly physical path for future loading by IModuleTypeLoader instance
                    moduleInfo.Ref = GetFileAbsoluteUri(_options.ProbingPath, manifest.AssemblyFile);
                }

                moduleInfo.IsInstalled = true;
                AddModule(moduleInfo);
            }
        }

        private void CopyAssembliesSynchronized(IDictionary<string, ModuleManifest> manifests)
        {
            _distributedLockProvider.ExecuteSynchronized(GetSourceMark(), (x) =>
            {
                if (x != DistributedLockCondition.Delayed)
                {
                    CopyAssemblies(_discoveryPath, _options.ProbingPath); // Copy platform files if needed
                    foreach (var pair in manifests)
                    {
                        var modulePath = Path.GetDirectoryName(pair.Key);
                        CopyAssemblies(modulePath, _options.ProbingPath); // Copy module files if needed
                    }
                }
                else // Delayed lock acquire, do nothing here with a notice logging
                {
                    _logger.LogInformation("Skip copy assemblies to ProbingPath for local storage (another instance made it)");
                }
            });
        }

        private string GetSourceMark()
        {
            var markerFilePath = Path.Combine(_options.ProbingPath, "storage.mark");
            var marker = Guid.NewGuid().ToString();
            try
            {
                if (File.Exists(markerFilePath))
                {
                    using var stream = File.OpenText(markerFilePath);
                    marker = stream.ReadToEnd();
                }
                else
                {
                    // Non-marked storage, mark by placing a file with resource id.                    
                    using var stream = File.CreateText(markerFilePath);
                    stream.Write(marker);
                }
            }
            catch (IOException exc)
            {
                throw new PlatformException("An IO error occurred while marking local modules storage.", exc);
            }
            return $"{nameof(LocalStorageModuleCatalog)}-{marker}";
        }

        private static string GetFileAbsoluteUri(string rootPath, string relativePath)
        {
            var builder = new UriBuilder
            {
                Host = string.Empty,
                Scheme = Uri.UriSchemeFile,
                Path = Path.GetFullPath(Path.Combine(rootPath, relativePath))
            };

            return builder.Uri.ToString();
        }

        private Dictionary<string, ModuleManifest> GetModuleManifests()
        {
            var result = new Dictionary<string, ModuleManifest>();

            if (Directory.Exists(_options.DiscoveryPath))
            {
                foreach (var manifestFile in Directory.EnumerateFiles(_options.DiscoveryPath, "module.manifest", SearchOption.AllDirectories))
                {
                    if (!manifestFile.Contains("artifacts") || _options.DiscoveryPath.Contains("artifacts"))
                    {
                        var manifest = ManifestReader.Read(manifestFile);
                        result.Add(manifestFile, manifest);
                    }
                }
            }
            return result;
        }
        private void CopyAssemblies(string sourceParentPath, string targetDirectoryPath)
        {
            if (sourceParentPath == null)
            {
                return;
            }

            var sourceDirectoryPath = Path.Combine(sourceParentPath, "bin");

            if (Directory.Exists(sourceDirectoryPath))
            {
                foreach (var sourceFilePath in Directory.EnumerateFiles(sourceDirectoryPath, "*.*", SearchOption.AllDirectories).Where(f => IsAssemblyRelatedFile(f)))
                {
                    // Copy localization resource files to related subfolders
                    var targetFilePath = Path.Combine(
                        IsLocalizationFile(sourceFilePath) ? Path.Combine(targetDirectoryPath, Path.GetFileName(Path.GetDirectoryName(sourceFilePath)))
                        : targetDirectoryPath,
                        Path.GetFileName(sourceFilePath));
                    CopyFile(sourceFilePath, targetFilePath);
                }
            }
        }

        private bool IsAssemblyRelatedFile(string path)
        {
            return _options.AssemblyFileExtensions.Union(_options.AssemblyServiceFileExtensions).Any(x => path.EndsWith(x, StringComparison.OrdinalIgnoreCase));
        }

        private bool IsLocalizationFile(string path)
        {
            return _options.LocalizationFileExtensions.Any(x => path.EndsWith(x, StringComparison.OrdinalIgnoreCase));
        }

        private void CopyFile(string sourceFilePath, string targetFilePath)
        {
            var sourceFileInfo = new FileInfo(sourceFilePath);
            var targetFileInfo = new FileInfo(targetFilePath);

            var sourceFileVersionInfo = FileVersionInfo.GetVersionInfo(sourceFilePath);
            var sourceVersion = new Version(sourceFileVersionInfo.FileMajorPart, sourceFileVersionInfo.FileMinorPart, sourceFileVersionInfo.FileBuildPart, sourceFileVersionInfo.FilePrivatePart);
            var targetVersion = sourceVersion;

            if (targetFileInfo.Exists)
            {
                var targetFileVersionInfo = FileVersionInfo.GetVersionInfo(targetFilePath);
                targetVersion = new Version(targetFileVersionInfo.FileMajorPart, targetFileVersionInfo.FileMinorPart, targetFileVersionInfo.FileBuildPart, targetFileVersionInfo.FilePrivatePart);
            }

            var versionsAreSameButLaterDate = sourceVersion == targetVersion && targetFileInfo.Exists && sourceFileInfo.Exists && targetFileInfo.LastWriteTimeUtc < sourceFileInfo.LastWriteTimeUtc;
            if (!targetFileInfo.Exists || sourceVersion > targetVersion || versionsAreSameButLaterDate)
            {
                var targetDirectoryPath = Path.GetDirectoryName(targetFilePath);
                Directory.CreateDirectory(targetDirectoryPath);

                try
                {
                    File.Copy(sourceFilePath, targetFilePath, true);
                }
                catch (IOException) when (versionsAreSameButLaterDate)
                {
                    _logger.LogWarning("File '{TargetFilePath}' was not updated by '{SourceFilePath}' of the same version but later modified date, because probably it was used by another process", targetFilePath, sourceFilePath);
                }
            }
        }
    }
}
