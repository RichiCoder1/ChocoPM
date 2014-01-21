using System.Linq;
using System.Reactive.Linq;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ChocoPM.Services;
using ChocoPM.Extensions;

namespace ChocoPM.ViewModels
{
    public class HomeViewModel : ViewModelBase, IHomeViewModel
    {
        private string _searchQuery;
        public string SearchQuery
        {
            get { return _searchQuery; }
            set { SetPropertyValue(ref _searchQuery, value);}
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

        private bool _showPrerelease;
        public bool ShowPrerelease
        {
            get { return _showPrerelease; }
            set { SetPropertyValue(ref _showPrerelease, value); }
        }

        private string _sortColumn;
        public string SortColumn
        {
            get { return _sortColumn; }
            set { SetPropertyValue(ref _sortColumn, value); }
        }

        private bool _sortDescending;
        public bool SortDescending
        {
            get { return _sortDescending; }
            set { SetPropertyValue(ref _sortDescending, value); }
        }

        private readonly IRemoteChocolateyService _service;

        public HomeViewModel(IRemoteChocolateyService service)
        {
            _service = service;
            _sortColumn = "DownloadCount";
            _sortDescending = true;
            Packages = new ObservableCollection<PackageViewModel>();

            LoadPackages();

            Observable.FromEventPattern<PropertyChangedEventArgs>(this, "PropertyChanged")
                .Where(e => e.EventArgs.PropertyName == "SearchQuery")
                .Throttle(TimeSpan.FromMilliseconds(500))
                .ObserveOnDispatcher()
                .Subscribe(e => LoadPackages());

            Observable.FromEventPattern<PropertyChangedEventArgs>(this, "PropertyChanged")
                .Where(e => e.EventArgs.PropertyName == "SortColumn" || e.EventArgs.PropertyName == "SortDescending")
                .Throttle(TimeSpan.FromMilliseconds(200))
                .ObserveOnDispatcher()
                .Subscribe(e => LoadPackages());

            SelectedPackage = null;
        }

        public void LoadPackages()
        {
            IQueryable<V2FeedPackage> query = _service.Packages.Where(package => package.IsLatestVersion && package.Title != null && package.Title != string.Empty);
            if(!ShowPrerelease)
                query = query.Where(package => package.IsPrerelease == false);

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                query = query.Where(package => package.Title.Contains(SearchQuery));
            }
            if(!string.IsNullOrWhiteSpace(SortColumn))
                query = !SortDescending ? query.OrderBy(this._sortColumn) : query.OrderByDescending(this._sortColumn);
                
            Packages.Clear();
            query.ToList().Select(package => new PackageViewModel(package)).ToList().ForEach(Packages.Add);
        }
    }
}
