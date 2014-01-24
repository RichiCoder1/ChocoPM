using System.Linq;
using System.Reactive.Linq;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ChocoPM.Services;
using ChocoPM.Extensions;
using System.Threading.Tasks;
using ChocoPM.Models;

namespace ChocoPM.ViewModels
{
    public class HomeViewModel : ObservableBase, IHomeViewModel
    {
        private readonly IRemoteChocolateyService _remoteService;
        public IRemoteChocolateyService RemoteService
        {
            get { return _remoteService; }
        }

        private readonly ILocalChocolateyService _localService;
        public ILocalChocolateyService LocalService
        {
            get { return _localService; }
        }

        public HomeViewModel(IRemoteChocolateyService remoteService, ILocalChocolateyService localService)
        {
            _remoteService = remoteService;
            _localService = localService;

            _lazyAvailableVm = new Lazy<AvailablePackagesViewModel>(() => new AvailablePackagesViewModel(this));
            _lazyInstalledVm = new Lazy<InstalledPackagesViewModel>(() => new InstalledPackagesViewModel(this));
        }

        private readonly Lazy<AvailablePackagesViewModel> _lazyAvailableVm; 
        public AvailablePackagesViewModel AvailablePackagesViewModel { get { return _lazyAvailableVm.Value; } }

        private readonly Lazy<InstalledPackagesViewModel> _lazyInstalledVm;
        public InstalledPackagesViewModel InstalledPackagesViewModel { get { return _lazyInstalledVm.Value; } }

    }
}
