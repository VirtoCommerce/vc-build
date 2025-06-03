using System.IO;
using Nuke.Common.IO;

namespace Extensions
{
    public static class PathExtension
    {
        public static AbsolutePath ToAbsolutePath(this string path)
        {
            if (path == null)
            {
                return null;
            }

            return AbsolutePath.Create(Path.GetFullPath(path));
        }
    }
}
