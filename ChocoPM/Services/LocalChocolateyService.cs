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
        
        /// <summary>
        /// The PowerShell runspace for this service.
        /// </summary>
        private readonly Runspace _rs;

        /// <summary>
        /// Synchornizes the GetPackages method.
        /// </summary>
        private readonly SemaphoreSlim _ss;

        /// <summary>
        /// Cache for this servce where out installed packages list is stored.
        /// </summary>
        private readonly MemoryCache _cache = MemoryCache.Default;

        /// <summary>
        /// The key in the <paramref name="_cache">Service's Memory Cache</paramref> for this service's packages./>
        /// </summary>
        private const string LocalCacheKeyName = "LocalChocolateyService.Packages";

        /// <summary>
        /// The remote package service.
        /// </summary>
        private readonly IRemoteChocolateyService _remoteService;

        /// <summary>
        /// Retrives all the local packages.
        /// </summary>
        /// <param name="logOutput">If true, writes messages out to the faux PowerShell console.</param>
        /// <returns>List of all the local packages.</returns>
        public async Task<IEnumerable<V2FeedPackage>> GetPackages(bool logOutput = false)
        {
            if (logOutput)
            {
                _mainWindowVm.OutputBuffer.Clear();
                isProcessing = true;
            }
                
            if(logOutput)
                WriteOutput("Checking cache for packages...");

            // Ensure that we only retrieve the packages once to refresh the Cache.
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
                // Should we replace this with lazy loading of some sort and move the details retrieval to the PackageViewModel?
                // Having LocalChocoService coupled with RemoteService seems like all kinds of bad idea, especially if and when we introduce multiple remote sources.
                // Remote sources cause an issue because 
                // A) If a package is not in the current remote, obviously the remote is unhappy. 
                //      The if(package == null) here _service.IgnoreResourceNotFoundException = true in RemoteChocoService are because of this.
                // B) If a package is in two remotes with identical Ids and Versions, you run into potential conflicts.
                //      This may not be an issue if the package maintainers are uploading the same nupkg to each remote.
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
                (new Action(() => isProcessing = false)).DelayedExecution(TimeSpan.FromMilliseconds(500));
            }

            _ss.Release();
            NotifyPropertyChanged("Packages");
            return packages;
        }

        /// <summary>
        /// Invalidates and refreshes the cache.
        /// </summary>
        /// <param name="logOutput">See <see cref="GetPackages"/> logOut param.</param>
        private async void RefreshPackages(bool logOutput = false)
        {
            this._cache.Remove(LocalCacheKeyName);
            await GetPackages(logOutput);
        }

        /// <summary>
        /// Uninstall the specified package.
        /// </summary>
        /// <param name="id">The Id of the package to be removed.</param>
        /// <param name="version">The Version string of the package to be removed.</param>
        /// <returns>Whether the package was successfully removed.</returns>
        public async Task<bool> UninstallPackageAsync(string id, string version)
        {
            return await ExecutePackageCommand("chocolatey uninstall " + id + " -version " + version);
        }

        /// <summary>
        /// Install the specific package.
        /// Optionally installs a specific version of the package.
        /// </summary>
        /// <param name="id">The Id of the package to be isntalled.</param>
        /// <param name="version">The specific Version string of the package to be installed.</param>
        /// <returns>Whether the package was successfully installed.</returns>
        public async Task<bool> InstallPackageAsync(string id, string version = null)
        {
            return await ExecutePackageCommand("chocolatey install " + id + (version != null ? " -version " + version : ""));
        }

        /// <summary>
        /// Checks wether the specified package is installed.
        /// </summary>
        /// <param name="id">The Id of the package.</param>
        /// <param name="version">The Version string of the package.</param>
        /// <remarks>
        ///     This method will only return true if:
        ///     A) We've already retrieved the local packages and cached them.
        ///     B) The package is found in the cache.
        ///     This is to ensure that we're not pulling the local package list until the App specifically request it.
        /// </remarks>
        /// <returns>Whether the package is installed.</returns>
        public bool IsInstalled(string id, string version)
        {
            if (_cache.Contains(LocalCacheKeyName))
            {
                return ((List<V2FeedPackage>)this._cache.Get(LocalCacheKeyName))
                    .Any(package => package.Id == id && package.Version == version);
            }
            return false;
        }

        /// <summary>
        /// Updates the installed package.
        /// </summary>
        /// <param name="id">The Id of the package.</param>
        /// <remarks>
        /// Chocolatey does not currently support updating an installed package to a specific version.
        /// There's a way to do this (basically uninstall and install specific version), but it's complex and error prone enough that we're avoiding it for now.
        /// </remarks>
        /// <returns>Whether the package was updated succesfully.</returns>
        public async Task<bool> UpdatePackageAsync(string id)
        {
            return await ExecutePackageCommand("chocolatey update " + id);
        }

        /// <summary>
        /// Executes a PowerShell command and returns whether or not there was a result. Optionally calls <see cref="RefreshPackages"/>.
        /// </summary>
        /// <param name="commandString">The PowerShell command string.</param>
        /// <param name="refreshPackages">Whether to call <see cref="RefreshPackages"/>.</param>
        /// <returns>Whether or not a result was returned from <see cref="RunPackageCommand"/>.</returns>
        public async Task<bool> ExecutePackageCommand(string commandString, bool refreshPackages = true)
        {
            return await RunPackageCommand(commandString, refreshPackages) != null;
        }
        /// <summary>
        /// Executes a PowerShell Command. 
        /// </summary>
        /// <param name="commandString">The PowerShell command string.</param>
        /// <param name="refreshPackages">Whether to call <see cref="RefreshPackages"/>.</param>
        /// <param name="logOutput">Whether the output should be logged to the faux PowerShell console or returned as results.</param>
        /// <param name="clearBuffer">Whether the faux PowerShell console should be cleared.</param>
        /// <returns>A collection of the ouptut of the PowerShell runspace. Will be empty if <paramref cref="logOuput"/> is true.</returns>
        public async Task<Collection<PSObject>> RunPackageCommand(string commandString, bool refreshPackages = true, bool logOutput = true, bool clearBuffer = true)
        {
            if(logOutput)
                isProcessing = true;

            if(clearBuffer)
                _mainWindowVm.OutputBuffer.Clear();

            // Runs the PowerShell runspace in the background.
            // Runspace does have a way of running powershell pipelines asynchronously, but making it "async" compatible is a painful and error ridden process.
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

            if (logOutput)
            {
                // Gives the user a chance to read the last messages. Should we ditch this?
                Thread.Sleep(200);
                isProcessing = false;
            }

            return result;
        }

        /// <summary>
        /// Helper function to write output messages to the faux PowerShell console.
        /// </summary>
        /// <param name="message">Message to be written.</param>
        private void WriteOutput(string message)
        {
            _mainWindowVm.OutputBuffer.Add(new PowerShellOutputLine{Text = message, Type = PowerShellLineType.Output});
        }

        /// <summary>
        /// Helper function to write error messages to the faux PowerShell console.
        /// </summary>
        /// <param name="message">Message to be written.</param>
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
