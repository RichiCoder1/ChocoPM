using System.Collections.ObjectModel;
using ChocoPM.ViewModels;
using ChocoPM.Models;
using ChocoPM.Extensions;
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
        public LocalChocolateyService(IRemoteChocolateyService remoteService)
        {
            _remoteService = remoteService;
            _rs = RunspaceFactory.CreateRunspace();
            _rs.Open();
            _ss = new SemaphoreSlim(1);
        }        
        
        private readonly Runspace _rs;
        private readonly SemaphoreSlim _ss;
        private readonly MemoryCache _cache = MemoryCache.Default;
        private const string LocalCacheKeyName = "LocalChocolateyService.Packages";
        private readonly IRemoteChocolateyService _remoteService;

        public async Task<IEnumerable<V2FeedPackage>> GetPackages(bool logOutput = false)
        {
            if (logOutput)
            {
                _mainWindowVm.OutputBuffer.Clear();
                isProcessing = true;
            }
                
            if(logOutput)
                WriteOutput("Checking cache for packages...");

            await _ss.WaitAsync(200);

            var packages = (List<V2FeedPackage>) this._cache.Get(LocalCacheKeyName);
            if (packages != null)
            {
                if(logOutput)
                    WriteOutput("Found cached packages");

                _ss.Release();
                return packages;
            }

            if(logOutput)
                WriteOutput("Retrieving local package list.");

            var packageResults = await RunPackageCommand("chocolatey list -lo", refreshPackages: false, logOutput: false, clearBuffer: false);

            var packageNames = packageResults != null
                ? packageResults.Select(obj => obj.ToString())
                : new List<string>();

            var packageDescriptions =
                packageNames.Select(packageName => packageName.Split(' '))
                            .Where(packageArray => packageArray.Count() == 2)
                            .Select(packageArray => new {
                                Id = packageArray[0],
                                Version = packageArray[1]
                            }).ToList();
            if(logOutput)
                WriteOutput("Found " + packageDescriptions.Count() + " local packges.");

            packages = new List<V2FeedPackage>();
            foreach (var packageDesc in packageDescriptions)
            {
                // ReSharper disable once ReplaceWithSingleCallToSingleOrDefault
                var package =
                    this._remoteService.Packages.Where(
                        pckge =>
                            packageDesc.Id == pckge.Id && packageDesc.Version == pckge.Version)
                        .SingleOrDefault();

                if (package == null)
                    continue;
                packages.Add(package);
            }
            if (logOutput)
                WriteOutput("Caching packages.");

            this._cache.Set(LocalCacheKeyName, packages, new CacheItemPolicy {
                SlidingExpiration = TimeSpan.FromDays(1)
            });

            if (logOutput)
            {
                WriteOutput("Done.");
                (new Action(() => isProcessing = false)).DelayedExecution(TimeSpan.FromMilliseconds(2000));
            }

            _ss.Release();
            NotifyPropertyChanged("Packages");
            return packages;
        }

        private async void RefreshPackages(bool logOutput = false)
        {
            this._cache.Remove(LocalCacheKeyName);
            await GetPackages(logOutput);
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
            if (_cache.Contains(LocalCacheKeyName))
            {
                return ((List<V2FeedPackage>)this._cache.Get(LocalCacheKeyName))
                    .Any(package => package.Id == id && package.Version == version);
            }
            return false;
        }

        public async Task<bool> UpdatePackageAsync(string id)
        {
            return await ExecutePackageCommand("chocolatey update " + id);
        }

        public async Task<bool> ExecutePackageCommand(string commandString, bool refreshPackages = true)
        {
            return await RunPackageCommand(commandString, refreshPackages) != null;
        }

        public async Task<Collection<PSObject>> RunPackageCommand(string commandString, bool refreshPackages = true, bool logOutput = true, bool clearBuffer = true)
        {
            if(logOutput)
                isProcessing = true;

            if(clearBuffer)
                _mainWindowVm.OutputBuffer.Clear();

            var result = await Task.Run(() =>
            {
                lock (_rs)
                {
                    var ps = _rs.CreatePipeline(commandString);
                    if (logOutput)
                    {
                        ps.Output.DataReady += (obj, args) =>
                        {
                            var outputs = ps.Output.NonBlockingRead();
                            foreach (PSObject output in outputs)
                            {
                                WriteOutput(output.ToString());
                            }
                        };
                        ps.Error.DataReady += (obj, args) =>
                        {
                            var outputs = ps.Error.NonBlockingRead();
                            foreach (PSObject output in outputs)
                            {
                                WriteError(output.ToString());
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
                        _mainWindowVm.OutputBuffer.Add(new PowerShellOutputLine {
                            Text = e.ToString(),
                            Type = PowerShellLineType.Error
                        });
                        return results;
                    }

                    _mainWindowVm.OutputBuffer.Add(new PowerShellOutputLine {
                        Text = "Executed successfully...",
                        Type = PowerShellLineType.Output
                    });

                    if (refreshPackages)
                        RefreshPackages();

                    return results;
                }
            });
            Thread.Sleep(200);
            if(logOutput)
                isProcessing = false;

            return result;
        }

        private void WriteOutput(string message)
        {
            _mainWindowVm.OutputBuffer.Add(new PowerShellOutputLine{Text = message, Type = PowerShellLineType.Output});
        }

        private void WriteError(string message)
        {
            _mainWindowVm.OutputBuffer.Add(new PowerShellOutputLine { Text = message, Type = PowerShellLineType.Error });
        }

        internal static IMainWindowViewModel _mainWindowVm;
        private bool isProcessing
        {
            get { return _mainWindowVm != null && _mainWindowVm.IsProcessing; }
            set { if (_mainWindowVm != null) _mainWindowVm.IsProcessing = value; }
        }
    }
}
