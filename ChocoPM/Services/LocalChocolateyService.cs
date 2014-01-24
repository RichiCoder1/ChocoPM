using System.Collections.ObjectModel;
using ChocoPM.ViewModels;
using ChocoPM.Models;
using ChocoPM.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Runtime.Caching;
using System.Threading.Tasks;
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

        private readonly PowerShell _ps = PowerShell.Create();

        public IEnumerable<V2FeedPackage> GetPackages()
        {
            var packages = (List<V2FeedPackage>)Cache.Get(LocalCacheKeyName);
            if (packages != null)
            {
                return packages;
            }

            var packageNames = AsyncHelpers.RunSynchronously(() => 
                RunPackageCommand("chocolatey list -lo", refreshPackages: false, logOutput: false)).Select(obj => obj.ToString());

            var packageDescriptions =
                packageNames.Select(packageName => packageName.Split(' '))
                            .Where(packageArray => packageArray.Count() == 2)
                            .Select(packageArray => new { Id = packageArray[0], Version = packageArray[1] });

            packages = new List<V2FeedPackage>();
            foreach (var packageDesc in packageDescriptions)
            {
                // ReSharper disable once ReplaceWithSingleCallToSingleOrDefault
                var package = this._remoteService.Packages.Where(pckge => packageDesc.Id == pckge.Id && packageDesc.Version == pckge.Version).SingleOrDefault();
                if (package == null)
                    continue;
                packages.Add(package);
            }
            this.Cache.Set(LocalCacheKeyName, packages, new CacheItemPolicy { SlidingExpiration = TimeSpan.FromDays(1) });
            NotifyPropertyChanged("Packages");
            return packages;
        }

        private void RefreshPackages()
        {
            Cache.Remove(LocalCacheKeyName);
            GetPackages();
        }

        public async Task<bool> UninstallPackageAsync(string id, string version)
        {
            return await ExecutePackageCommand("chocolatey uninstall " + id + " -version " + version);
        }

        public async Task<bool> InstallPackageAsync(string id, string version = null)
        {
            return await ExecutePackageCommand("chocolatey install " + id + " -version " + version);
        }

        public bool IsInstalled(string id, string version)
        {
            return GetPackages().Any(package => package.Id == id && package.Version == version);
        }

        public async Task<bool> UpdatePackageAsync(string id)
        {
            return await ExecutePackageCommand("chocolatey update " + id);
        }

        public async Task<bool> ExecutePackageCommand(string commandString, bool refreshPackages = true)
        {
            return await RunPackageCommand(commandString, refreshPackages) != null;
        }

        public async Task<Collection<PSObject>> RunPackageCommand(string commandString, bool refreshPackages = true, bool logOutput = true)
        {
            isProcessing = true;
            _mainWindowVm.OutputBuffer.Clear(); ;
            var result = await Task.Run(() =>
            {
                using (var rs = RunspaceFactory.CreateRunspace())
                {
                    rs.Open();
                    var ps = rs.CreatePipeline(commandString);
                    if (logOutput)
                    {
                        ps.Output.DataReady += (obj, args) =>
                        {
                            var outputs = ps.Output.NonBlockingRead();
                            foreach (PSObject output in outputs)
                            {
                                _mainWindowVm.OutputBuffer.Add(
                                    new PowerShellOutputLine {
                                        Text = output.ToString(),
                                        Type = PowerShellLineType.Output
                                    });
                            }
                        };
                        ps.Error.DataReady += (obj, args) =>
                        {
                            var outputs = ps.Error.NonBlockingRead();
                            foreach (PSObject output in outputs)
                            {
                                _mainWindowVm.OutputBuffer.Add(
                                    new PowerShellOutputLine {
                                        Text = output.ToString(),
                                        Type = PowerShellLineType.Error
                                    });
                            }
                        };
                    }

                    Collection<PSObject> results = null;
                    try
                    {
                        results = ps.Invoke();
                    }
                    catch (Exception e)
                    {
                        _mainWindowVm.OutputBuffer.Add(new PowerShellOutputLine { Text = e.ToString(), Type = PowerShellLineType.Error });
                        return results;
                    }

                    _mainWindowVm.OutputBuffer.Add(new PowerShellOutputLine { Text = "Executed successfully...", Type = PowerShellLineType.Output });

                    if(refreshPackages)
                        RefreshPackages();

                    return results;
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
