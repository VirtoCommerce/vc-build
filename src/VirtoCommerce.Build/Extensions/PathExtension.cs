using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nuke.Common.IO;
using System.IO;

namespace Extensions
{
    public static class PathExtension
    {
        public static AbsolutePath ToAbsolutePath(this string path)
        {
            return AbsolutePath.Create(Path.GetFullPath(path));
        }
    }
}
