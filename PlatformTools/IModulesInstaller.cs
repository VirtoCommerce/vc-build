using System;
using System.Threading.Tasks;

namespace VirtoCommerce.Build.PlatformTools
{
    public interface IModulesInstaller
    {
        public Task Install(ModuleSource modules);
    }
}

