using ChocoPM.Services;
using ChocoPM.Extensions;
using ChocoPM.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.ComponentModel;
using Ninject;
using Ninject.Parameters;
using System.Globalization;

namespace ChocoPM.ViewModels
{
    public class InstalledPackagesViewModel : ObservableBase
    {
        private string _searchQuery;
        public string SearchQuery
        {
            get { return _searchQuery; }
            set { SetPropertyValue(ref _searchQuery, value); }
        }

        private bool _match;
        public bool Match
        {
            get { return _match; }
            set { SetPropertyValue(ref _match, value); }
        }

        private ObservableCollection<PackageViewModel> _packages;
        public ObservableCollection<PackageViewModel> Packages
        {
            get { return _packages; }
        }

        private PackageViewModel _selectedPackage;
        public PackageViewModel SelectedPackage
        {
            get { return _selectedPackage; }
            set { SetPropertyValue(ref _selectedPackage, value); }
        }

        private bool _isVisible;
        public bool IsVisible
        {
            get { return _isVisible; }
            set 
            { 
                SetPropertyValue(ref _isVisible, value); 
                // Enables lazy loading if the LoadPackages(true) in the Initializer is commented out.
                // Warning: Here be dragons. Causes all kinds of funny business with PackageViewModel's IsInstalled
                /*if(value && !_isLoaded) LoadPackages(true);*/ 
            }
        }

        private bool _isLoaded;
        public bool IsLoaded
        {
            get { return _isLoaded; }
            set { SetPropertyValue(ref _isLoaded, value); }
        }

        private bool _loading;
        public bool Loading
        {
            get { return _loading; }
            set { SetPropertyValue(ref _loading, value); }
        }

        private IRemoteChocolateyService _remoteService { get { return _parent.RemoteService; } }
        private ILocalChocolateyService _localService { get { return _parent.LocalService; } }
        private IHomeViewModel _parent;
        public InstalledPackagesViewModel(IHomeViewModel parent)
        {
            _parent = parent;
            _packages = new ObservableCollection<PackageViewModel>();

            LoadPackages(true);

            Observable.FromEventPattern<PropertyChangedEventArgs>(this, "PropertyChanged")
                .Where(e => e.EventArgs.PropertyName == "Match")
                .ObserveOnDispatcher()
                .Subscribe(e => LoadPackages());

            Observable.FromEventPattern<PropertyChangedEventArgs>(this, "PropertyChanged")
                .Where(e => e.EventArgs.PropertyName == "SearchQuery")
                .Throttle(TimeSpan.FromMilliseconds(400))
                .DistinctUntilChanged()
                .ObserveOnDispatcher()
                .Subscribe(e => LoadPackages());


            Observable.FromEventPattern<PropertyChangedEventArgs>(_localService, "PropertyChanged")
                .Where(e => e.EventArgs.PropertyName == "Packages")
                .ObserveOnDispatcher()
                .Subscribe(e => LoadPackages());

            SelectedPackage = null;
        }

        public async void LoadPackages(bool logOutput = false)
        {
            Loading = true;
            try
            {
                IQueryable<V2FeedPackage> packages = (await _localService.GetPackages(logOutput)).AsQueryable();
                if (!string.IsNullOrWhiteSpace(SearchQuery))
                {
                    if(Match)
                        packages = packages.Where(package => string.Compare((package.Title ?? package.Id), SearchQuery, true) == 0);
                    else
                        packages = packages.Where(package => CultureInfo.CurrentCulture.CompareInfo.IndexOf((package.Title ?? package.Id), SearchQuery, CompareOptions.OrdinalIgnoreCase) >= 0);
                }

                var packagesList = packages.Select(package => 
                                                App.Kernel.Get<PackageViewModel>(new ConstructorArgument("feedPackage", package)))
                                           .ToList();
                Packages.Clear();
                packagesList.ForEach(Packages.Add);
            }
            finally
            {
                Loading = false;
            }
            IsLoaded = true;
        }
    }
}
