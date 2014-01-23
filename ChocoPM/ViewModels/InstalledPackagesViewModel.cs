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

        private ObservableCollection<PackageViewModel> _packages;
        public ObservableCollection<PackageViewModel> Packages
        {
            get { return _packages; }
            set { SetPropertyValue(ref _packages, value); }
        }

        private PackageViewModel _selectedPackage;
        public PackageViewModel SelectedPackage
        {
            get { return _selectedPackage; }
            set { SetPropertyValue(ref _selectedPackage, value); }
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
            Packages = new ObservableCollection<PackageViewModel>();

            LoadPackages();

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

        public async void LoadPackages()
        {
            Loading = true;
            try
            {
                var newPackages = await Task.Run(() =>
                {
                    IQueryable<V2FeedPackage> packages = _localService.GetPackages().AsQueryable();
                    if (!string.IsNullOrWhiteSpace(SearchQuery))
                    {
                        packages = packages.Where(package => CultureInfo.CurrentCulture.CompareInfo.IndexOf((package.Title == null ? package.Id : package.Title), SearchQuery, CompareOptions.OrdinalIgnoreCase) >= 0);
                    }

                    var packagesList = new List<PackageViewModel>();
                    foreach (var package in packages)
                    {
                        var newPackage = App.Kernel.Get<PackageViewModel>(new ConstructorArgument("feedPackage", package));
                        packagesList.Add(newPackage);
                    }

                    return packagesList;
                });
                Packages.Clear();
                newPackages.ForEach(Packages.Add);
            }
            catch
            {
                throw;
            }
            finally
            {
                Loading = false;
            }
        }
    }
}
