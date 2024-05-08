using System.Collections.Generic;

namespace Utils
{
    public class ModuleCompressionOptions
    {
        public string SourceDirectory { get; set; }
        public string OutputZipPath { get; set; }
        public string ModuleId { get; set; }
        public string ModuleManifestPath { get; set; }
        public string WebProjectDirectory { get; set; }
        public IEnumerable<string> IgnoreList { get; set; } = new List<string>();
        public IEnumerable<string> KeepList { get; set; } = new List<string>();
        public IEnumerable<string> ModuleContentFolders { get; set; } = new List<string>();
    }

    public class ModuleCompressionOptionsBuilder
    {
        private readonly ModuleCompressionOptions options = new ModuleCompressionOptions();

        public ModuleCompressionOptionsBuilder WithSourceDirectory(string sourceDirectory)
        {
            options.SourceDirectory = sourceDirectory;
            return this;
        }

        public ModuleCompressionOptionsBuilder WithOutputZipPath(string outputZipPath)
        {
            options.OutputZipPath = outputZipPath;
            return this;
        }

        public ModuleCompressionOptionsBuilder WithModuleId(string moduleId)
        {
            options.ModuleId = moduleId;
            return this;
        }

        public ModuleCompressionOptionsBuilder WithModuleManifestPath(string moduleManifestPath)
        {
            options.ModuleManifestPath = moduleManifestPath;
            return this;
        }

        public ModuleCompressionOptionsBuilder WithWebProjectDirectory(string webProjectDirectory)
        {
            options.WebProjectDirectory = webProjectDirectory;
            return this;
        }

        public ModuleCompressionOptionsBuilder WithIgnoreList(IEnumerable<string> ignoreList)
        {
            options.IgnoreList = ignoreList;
            return this;
        }

        public ModuleCompressionOptionsBuilder WithKeepList(IEnumerable<string> keepList)
        {
            options.KeepList = keepList;
            return this;
        }

        public ModuleCompressionOptionsBuilder WithModuleContentFolders(IEnumerable<string> moduleContentFolders)
        {
            options.ModuleContentFolders = moduleContentFolders;
            return this;
        }

        public ModuleCompressionOptions Build()
        {
            return options;
        }
    }
}
