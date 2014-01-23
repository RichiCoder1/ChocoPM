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

            AvailablePackagesViewModel = new ViewModels.AvailablePackagesViewModel(this);
            InstalledPackagesViewModel = new ViewModels.InstalledPackagesViewModel(this);

            var test = localService.GetPackages().ToList();
        }

        public AvailablePackagesViewModel AvailablePackagesViewModel { get; private set; }

        public InstalledPackagesViewModel InstalledPackagesViewModel { get; private set; }
    }
}
