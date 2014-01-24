using ChocoPM.ViewModels;
using ChocoPM.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;
using System.Runtime.Caching;
using System.Threading.Tasks;
using ChocoPM.Controls;
using System.Management.Automation.Runspaces;
using System.Threading;

namespace ChocoPM.Services
{
    public class LocalChocolateyService : ObservableBase, ILocalChocolateyService
    {
        private readonly MemoryCache Cache = MemoryCache.Default;
        private const string LocalCacheKeyName = "LocalChocolateyService.Packages";

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
          return await RunPackageCommand("chocolatey uninstall " + id + " -version " + version);
        }

        public async Task<bool> InstallPackageAsync(string id, string version = null)
        {
            return await RunPackageCommand("chocolatey install " + id + " -version " + version);
        }

        public bool IsInstalled(string id, string version)
        {
            return GetPackages().Any(package => package.Id == id && package.Version == version);
        }

        public async Task<bool> UpdatePackageAsync(string id)
        {
            return await RunPackageCommand("chocolatey update " + id);
        }

        public async Task<bool> RunPackageCommand(string commandString, bool refreshPackages = true)
        {
            isProcessing = true;
            _mainWindowVm.OutputBuffer.Clear(); ;
            var result = await Task.Run(() =>
            {
                using (var rs = RunspaceFactory.CreateRunspace())
                {
                    rs.Open();
                    var ps = rs.CreatePipeline(commandString);
                    ps.Output.DataReady += (obj, args) =>
                    {
                        var outputs = ps.Output.NonBlockingRead();
                        foreach (PSObject output in outputs)
                        {
                            _mainWindowVm.OutputBuffer.Add(new PowerShellOutputLine { Text = output.ToString(), Type = PowerShellLineType.Output });
                        }
                    };
                    ps.Error.DataReady += (obj, args) =>
                    {
                        var outputs = ps.Error.NonBlockingRead();
                        foreach (PSObject output in outputs)
                        {
                            _mainWindowVm.OutputBuffer.Add(new PowerShellOutputLine { Text = output.ToString(), Type = PowerShellLineType.Error });
                        }
                    };

                    try
                    {
                        ps.Invoke();
                    }
                    catch (Exception e)
                    {
                        _mainWindowVm.OutputBuffer.Add(new PowerShellOutputLine { Text = e.ToString(), Type = PowerShellLineType.Error });
                        return false;
                    }

                    _mainWindowVm.OutputBuffer.Add(new PowerShellOutputLine { Text = "Executed successfully...", Type = PowerShellLineType.Output });

                    if(refreshPackages)
                        RefreshPackages();

                    return true;
                }
            });
            Thread.Sleep(200);
            isProcessing = false;
            return result;
        }

        internal static IMainWindowViewModel _mainWindowVm;
        private bool isProcessing
        {
            get { return _mainWindowVm != null && _mainWindowVm.IsProcessing; }
            set { if (_mainWindowVm != null) _mainWindowVm.IsProcessing = value; }
        }
    }
}
