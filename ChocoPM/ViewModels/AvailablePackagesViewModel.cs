using ChocoPM.Services;
using ChocoPM.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Ninject;
using Ninject.Parameters;
using ChocoPM.Models;

namespace ChocoPM.ViewModels
{
    public class AvailablePackagesViewModel : ObservableBase
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

        private long _totalCount;
        public long TotalCount
        {
            get { return _totalCount; }
            set { SetPropertyValue(ref _totalCount, value); }
        }

        private int _currentPage;
        public int CurrentPage
        {
            get { return _currentPage; }
            set { SetPropertyValue(ref _currentPage, value); }
        }

        private int _pageSize;
        public int PageSize
        {
            get { return _pageSize; }
            set { SetPropertyValue(ref _pageSize, value); }
        }

        private int _pageCount;
        public int PageCount
        {
            get { return _pageCount; }
            set { SetPropertyValue(ref _pageCount, value); }
        }

        private bool _allVersions;
        public bool AllVersions
        {
            get { return _allVersions; }
            set { SetPropertyValue(ref _allVersions, value); }
        }

        private bool _prerelease;
        public bool Prerelease
        {
            get { return _prerelease; }
            set { SetPropertyValue(ref _prerelease, value); }
        }


        private bool _match;
        public bool Match
        {
            get { return _match; }
            set { SetPropertyValue(ref _match, value); }
        }

        private bool _loading;
        public bool Loading
        {
            get { return _loading; }
            set { SetPropertyValue(ref _loading, value); }
        }

        private IRemoteChocolateyService _remoteService { get { return _parent.RemoteService; } }
        private ILocalChocolateyService _localService { get { return _parent.LocalService; } }

        private readonly IHomeViewModel _parent;
        public AvailablePackagesViewModel(IHomeViewModel parent)
        {
            _parent = parent;
            _sortColumn = "DownloadCount";
            _sortDescending = true;
            _currentPage = 0;
            _pageSize = 50;
            _totalCount = _remoteService.Packages.Where(package => package.IsLatestVersion).LongCount();
            _pageCount = (int)(_totalCount / _pageSize);
            Packages = new ObservableCollection<PackageViewModel>();

            LoadPackages();

            var immediateProperties = new [] {
                "SortColumn", "SortDescending", "AllVersions", "Prerelease", "Match"
            };

            Observable.FromEventPattern<PropertyChangedEventArgs>(this, "PropertyChanged")
                .Where(e => e.EventArgs.PropertyName == "SearchQuery")
                .Throttle(TimeSpan.FromMilliseconds(500))
                .DistinctUntilChanged()
                .ObserveOnDispatcher()
                .Subscribe(e => LoadPackages());

            Observable.FromEventPattern<PropertyChangedEventArgs>(this, "PropertyChanged")
                .Where(e => immediateProperties.Contains(e.EventArgs.PropertyName))
                .ObserveOnDispatcher()
                .Subscribe(e => LoadPackages());


            Observable.FromEventPattern<PropertyChangedEventArgs>(this, "PropertyChanged")
                .Where(e => e.EventArgs.PropertyName == "CurrentPage")
                .Throttle(TimeSpan.FromMilliseconds(300))
                .DistinctUntilChanged()
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
                    IQueryable<V2FeedPackage> query = _remoteService.Packages.Where(package => package.IsPrerelease == Prerelease || package.IsPrerelease == false);

                    if (!AllVersions)
                        query = query.Where(package => package.IsLatestVersion || package.IsAbsoluteLatestVersion);

                    if (!string.IsNullOrWhiteSpace(SearchQuery))
                    {
                        query = Match ? 
                            query.Where(package => package.Id == SearchQuery || package.Title == SearchQuery) : 
                            query.Where(package => package.Id.Contains(SearchQuery) || package.Title.Contains(SearchQuery));
                    }
                    TotalCount = query.LongCount();
                    PageCount = (int)(_totalCount / _pageSize);

                    if (!string.IsNullOrWhiteSpace(SortColumn))
                        query = !SortDescending ? query.OrderBy(this._sortColumn) : query.OrderByDescending(this._sortColumn);

                    query = query.Skip(CurrentPage * PageSize).Take(PageSize);

                    return query.ToList().Select(package => App.Kernel.Get<PackageViewModel>(new ConstructorArgument("feedPackage", package))).ToList();
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

        public bool CanGoToFirst()
        {
            return CurrentPage != 0;
        }

        public void GoToFirst()
        {
            CurrentPage = 0;
        }

        public bool CanGoBack()
        {
            return CurrentPage > 0;
        }

        public void GoBack()
        {
            if (CurrentPage > 0)
                CurrentPage--;
        }

        public bool CanGoForward()
        {
            return CurrentPage < PageCount;
        }


        public void GoForward()
        {
            if (CurrentPage < PageCount)
                CurrentPage++;
        }

        public bool CanGoToLast()
        {
            return CurrentPage != PageCount;
        }

        public void GoToLast()
        {
            CurrentPage = PageCount;
        }
    }
}
