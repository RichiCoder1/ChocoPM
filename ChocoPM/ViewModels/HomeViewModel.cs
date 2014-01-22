﻿using System.Linq;
using System.Reactive.Linq;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ChocoPM.Services;
using ChocoPM.Extensions;
using System.Threading.Tasks;

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

        private long _pageCount;
        public long PageCount
        {
            get { return _pageCount; }
            set { SetPropertyValue(ref _pageCount, value); }
        }

        private bool _loading;
        public bool Loading
        {
            get { return _loading; }
            set { SetPropertyValue(ref _loading, value); }
        }

        private readonly IRemoteChocolateyService _service;

        public HomeViewModel(IRemoteChocolateyService service)
        {
            _service = service;
            _sortColumn = "DownloadCount";
            _sortDescending = true;
            _currentPage = 0;
            _pageSize = 50;
            _totalCount = _service.Packages.LongCount();
            _pageCount = _totalCount / _pageSize + (_totalCount % _pageSize > 0 ? 1 : 0);
            Packages = new ObservableCollection<PackageViewModel>();

            LoadPackages();

            Observable.FromEventPattern<PropertyChangedEventArgs>(this, "PropertyChanged")
                .Where(e => e.EventArgs.PropertyName == "SearchQuery")
                .Throttle(TimeSpan.FromMilliseconds(500))
                .DistinctUntilChanged()
                .ObserveOnDispatcher()
                .Subscribe(e => LoadPackages());

            Observable.FromEventPattern<PropertyChangedEventArgs>(this, "PropertyChanged")
                .Where(e => e.EventArgs.PropertyName == "SortColumn" || e.EventArgs.PropertyName == "SortDescending")
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
                    IQueryable<V2FeedPackage> query = _service.Packages.Where(package => package.IsLatestVersion && package.Title != null && package.Title != string.Empty);
                    if (!ShowPrerelease)
                        query = query.Where(package => package.IsPrerelease == false);

                    if (!string.IsNullOrWhiteSpace(SearchQuery))
                    {
                        query = query.Where(package => package.Title.Contains(SearchQuery));
                    }
                    if (!string.IsNullOrWhiteSpace(SortColumn))
                        query = !SortDescending ? query.OrderBy(this._sortColumn) : query.OrderByDescending(this._sortColumn);

                    query = query.Skip(CurrentPage * PageSize).Take(PageSize);

                    return query.ToList().Select(package => new PackageViewModel(package)).ToList();
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

        public void GoBack()
        {
            if (CurrentPage > 0)
                CurrentPage--;
        }

        public void GoForward()
        {
            if (CurrentPage < PageCount - 1)
                CurrentPage++;
        }
    }
}
