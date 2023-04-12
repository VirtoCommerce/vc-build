using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NuGet.Packaging;
using Nuke.Common.IO;
using Octokit;
using VirtoCommerce.Platform.Core.Modularity;

namespace Utils
{
    public static class ArtifactPacker
    {
        public static void CompressPlatform(string sourceDirectory, string outputZipPath)
        {
            FileSystemTasks.DeleteFile(outputZipPath);
            CompressionTasks.CompressZip(sourceDirectory, outputZipPath);
        }

        public static void CompressModuleAsync(string sourceDirectory, string outputZipPath, string moduleId, string moduleManifestPath, string webProjectDirectory, IEnumerable<string> ignoreList, IEnumerable<string> keepList, string[] moduleContentFolders)
        {
            FileSystemTasks.CopyFileToDirectory(moduleManifestPath, sourceDirectory,
                FileExistsPolicy.Overwrite);
            
            //Exclude all ignored files and *module files not related to compressed module
            var ignoreModuleFilesRegex = new Regex(@".+Module\..*", RegexOptions.IgnoreCase);
            var includeModuleFilesRegex =
                new Regex(@$".*{moduleId}(Module)?\..*", RegexOptions.IgnoreCase);

            foreach (var folderName in moduleContentFolders)
            {
                var sourcePath = Path.Combine(webProjectDirectory, folderName);

                if (Directory.Exists(sourcePath))
                {
                    FileSystemTasks.CopyDirectoryRecursively(sourcePath, Path.Combine(sourceDirectory, folderName),
                        DirectoryExistsPolicy.Merge, FileExistsPolicy.Overwrite);
                }
            }

            bool FilesFilter(FileInfo x) =>
                (!SkipFileByList(x.Name, ignoreList) &&
                 !SkipFileByRegex(x.Name, ignoreModuleFilesRegex)) || KeepFileByList(x.Name, keepList) ||
                KeepFileByRegex(x.Name, includeModuleFilesRegex);

            FileSystemTasks.DeleteFile(outputZipPath);
            CompressionTasks.CompressZip(sourceDirectory, outputZipPath, FilesFilter);
        }


        public static bool SkipFileByList(string name, IEnumerable<string> ignoreList)
        {
            return ignoreList.Contains(name, StringComparer.OrdinalIgnoreCase);
        }

        public static bool SkipFileByRegex(string name, Regex ignoreRegex)
        {
            return ignoreRegex.IsMatch(name);
        }

        public static bool KeepFileByList(string name, IEnumerable<string> keepList)
        {
            return keepList.Contains(name, StringComparer.OrdinalIgnoreCase);
        }

        public static bool KeepFileByRegex(string name, Regex keepRegex)
        {
            return keepRegex.IsMatch(name);
        }
    }
}
