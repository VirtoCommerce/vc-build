namespace PlatformTools
{
    internal class PackageItem
    {
        public string Name { get; set; }

        public string ProjectName { get; set; }

        public string Version { get; set; }

        /// <summary>
        /// Whether or not the package is a module package or a platfrom package
        /// </summary>
        public bool IsPlatformPackage { get; set; }
    }
}
