using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Extensions;
using Nuke.Common.IO;

namespace Utils
{
    public static partial class ArtifactPacker
    {
        public static void CompressPlatform(AbsolutePath sourceDirectory, AbsolutePath outputZipPath)
        {
            outputZipPath.DeleteFile();
            sourceDirectory.ZipTo(outputZipPath);
        }

        public static void CompressModule(Action<ModuleCompressionOptionsBuilder> optionsBuilderAction)
        {
            var optionsBuilder = new ModuleCompressionOptionsBuilder();
            optionsBuilderAction(optionsBuilder);
            var options = optionsBuilder.Build();

            FileSystemTasks.CopyFileToDirectory(options.ModuleManifestPath, options.SourceDirectory,
                FileExistsPolicy.Overwrite);
            
            //Exclude all ignored files and *module files not related to compressed module
            var ignoreModuleFilesRegex = IgnoreModuleFilesRegex();
            var includeModuleFilesRegex =
                new Regex(@$".*{options.ModuleId}(Module)?\..*", RegexOptions.IgnoreCase);

            foreach (var folderName in options.ModuleContentFolders)
            {
                var sourcePath = Path.Combine(options.WebProjectDirectory, folderName);

                if (Directory.Exists(sourcePath))
                {
                    FileSystemTasks.CopyDirectoryRecursively(sourcePath, Path.Combine(options.SourceDirectory, folderName),
                        DirectoryExistsPolicy.Merge, FileExistsPolicy.Overwrite);
                }
            }

            bool FilesFilter(AbsolutePath path)
            {
                var fileInfo = path.ToFileInfo();
                return (!SkipFileByList(fileInfo.Name, options.IgnoreList) &&
                 !SkipFileByRegex(fileInfo.Name, ignoreModuleFilesRegex)) || KeepFileByList(fileInfo.Name, options.KeepList) ||
                KeepFileByRegex(fileInfo.Name, includeModuleFilesRegex);
            }

            options.OutputZipPath.ToAbsolutePath().DeleteFile();
            options.SourceDirectory.ToAbsolutePath().ZipTo(options.OutputZipPath, FilesFilter);
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

        [GeneratedRegex(@".+Module\..*", RegexOptions.IgnoreCase)]
        private static partial Regex IgnoreModuleFilesRegex();
    }
}
