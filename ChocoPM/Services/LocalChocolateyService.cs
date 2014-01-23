using ChocoPM.ViewModels;
using ChocoPM.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace ChocoPM.Services
{
    public class LocalChocolateyService : ObservableBase, ILocalChocolateyService
    {
        private readonly MemoryCache Cache = MemoryCache.Default;
        private const string LocalCacheKeyName = "LocalChocolateyService.Packages";
        internal static IMainWindowViewModel _mainWindowVm;
        private bool isProcessing
        {
            get { return _mainWindowVm != null && _mainWindowVm.IsProcessing; }
            set { if (_mainWindowVm != null) _mainWindowVm.IsProcessing = value; }
        }

        private readonly IRemoteChocolateyService _remoteService;
        public LocalChocolateyService(IRemoteChocolateyService remoteService)
        {
            _remoteService = remoteService;
        }

        public IEnumerable<V2FeedPackage> GetPackages()
        {
            List<V2FeedPackage> packages = (List<V2FeedPackage>)Cache.Get(LocalCacheKeyName);
            if (packages == null)
            {
                using (var ps = PowerShell.Create())
                {
                    ps.AddCommand("chocolatey").AddArgument("list").AddArgument("-lo");

                    var packageNames = ps.Invoke<string>();
                    var packageDescriptions =
                        packageNames.Select(packageName => packageName.Split(' '))
                            .Where(packageArray => packageArray.Count() == 2)
                            .Select(packageArray => new { Id = packageArray[0], Version = packageArray[1] });

                    packages = new List<V2FeedPackage>();
                    foreach (var packageDesc in packageDescriptions)
                    {
                        var package = _remoteService.Packages.Where(pckge => packageDesc.Id == pckge.Id && packageDesc.Version == pckge.Version).SingleOrDefault();
                        packages.Add(package);
                    }
                }
                Cache.Set(LocalCacheKeyName, packages, new CacheItemPolicy { SlidingExpiration = TimeSpan.FromDays(1) });
                NotifyPropertyChanged("Packages");
            }
            return packages;
        }

        private void RefreshPackages()
        {
            Cache.Remove(LocalCacheKeyName);
            GetPackages();
        }

        public async Task<bool> UninstallPackageAsync(string id, string version)
        {
            isProcessing = true;
            var result = await Task.Run(() =>
            {
                using (var ps = PowerShell.Create())
                {
                    ps.AddCommand("chocolatey").AddArgument("uninstall").AddArgument(id).AddArgument("-version " + version);

                    try
                    {
                        var packageNames = ps.Invoke();
                    }
                    catch (Exception)
                    {
#if DEBUG
                        throw;
#endif
                        return false;
                    }
                    RefreshPackages();
                    return true;
                }
            });
            isProcessing = false;
            return result;
        }

        public async Task<bool> InstallPackageAsync(string id, string version = null)
        {
            isProcessing = true;
            var result = await Task.Run(() =>
            {
                using (var ps = PowerShell.Create())
                {
                    ps.AddCommand("chocolatey").AddArgument("install").AddArgument(id).AddArgument("-version " + version);

                    try
                    {
                        var packageNames = ps.Invoke();
                    }
                    catch (Exception)
                    {
    #if DEBUG
                        throw;
    #endif
                        return false;
                    }
                    RefreshPackages();
                    return true;
                }
            });
            isProcessing = false;
            return result;
        }

        public bool IsInstalled(string id, string version)
        {
            return GetPackages().Any(package => package.Id == id && package.Version == version);
        }

        public async Task<bool> UpdatePackageAsync(string id)
        {
            isProcessing = true;
            var result = await Task.Run(() =>
            {
                using (var ps = PowerShell.Create())
                {
                    ps.AddCommand("chocolatey").AddArgument("update").AddArgument(id);

                    try
                    {
                        var packageNames = ps.Invoke();
                    }
                    catch (Exception)
                    {
    #if DEBUG
                        throw;
    #endif
                        return false;
                    }
                    RefreshPackages();
                    return true;
                }
            });
            isProcessing = false;
            return result;
        }
    }
}
