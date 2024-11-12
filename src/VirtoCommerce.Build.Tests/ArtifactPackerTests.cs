using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Utils;

namespace VirtoCommerce.Build.Tests
{
    public partial class ArtifactPackerTests
    {
        [Fact]
        public void SkipFileByList_Returns_False_When_Not_In_IgnoreList()
        {
            var ignoreList = new List<string> { "file1.txt", "file2.txt" };
            var fileName = "file3.txt";

            var result = ArtifactPacker.SkipFileByList(fileName, ignoreList);

            Assert.False(result);
        }

        [Fact]
        public void SkipFileByList_Returns_True_When_In_IgnoreList()
        {
            var ignoreList = new List<string> { "file1.txt", "file2.txt" };
            var fileName = "file1.txt";

            var result = ArtifactPacker.SkipFileByList(fileName, ignoreList);

            Assert.True(result);
        }

        [Fact]
        public void SkipFileByRegex_Returns_True_When_Matching()
        {
            var regex = SkipFilesRegex();
            var fileName = "TestModule.dll";

            var result = ArtifactPacker.SkipFileByRegex(fileName, regex);

            Assert.True(result);
        }

        [Fact]
        public void SkipFileByRegex_Returns_False_When_Not_Matching()
        {
            var ignoreRegex = SkipFilesRegex();
            var fileName = "AnotherLibrary.dll";

            var result = ArtifactPacker.SkipFileByRegex(fileName, ignoreRegex);

            Assert.False(result);
        }

        [Fact]
        public void KeepFileByList_Returns_True_When_In_KeepList()
        {
            var keepList = new List<string> { "file1.txt", "file2.txt" };
            var fileName = "file1.txt";

            var result = ArtifactPacker.KeepFileByList(fileName, keepList);

            Assert.True(result);
        }

        [Fact]
        public void KeepFileByList_Returns_False_When_Not_In_KeepList()
        {
            var keepList = new List<string> { "file1.txt", "file2.txt" };
            var fileName = "file3.txt";

            var result = ArtifactPacker.KeepFileByList(fileName, keepList);

            Assert.False(result);
        }

        [Fact]
        public void KeepFileByRegex_Returns_True_When_Matching()
        {
            var regex = KeepFilesRegex();
            var fileName = "SampleModule.dll";

            var result = ArtifactPacker.KeepFileByRegex(fileName, regex);

            Assert.True(result);
        }

        [Fact]
        public void KeepFileByRegex_Returns_False_When_Not_Matching()
        {
            var regex = KeepFilesRegex();
            var fileName = "NotSample.dll";

            var result = ArtifactPacker.KeepFileByRegex(fileName, regex);

            Assert.False(result);
        }

        [GeneratedRegex(@".+Module\..*", RegexOptions.IgnoreCase, "en-GB")]
        private static partial Regex SkipFilesRegex();

        [GeneratedRegex(@".*SampleModule(Module)?\..*", RegexOptions.IgnoreCase, "en-GB")]
        private static partial Regex KeepFilesRegex();
    }
}
