using ChocoPM.ViewModels;
using ChocoPM.Extensions;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Collections.Specialized;
using System;
namespace ChocoPM.Views
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home
    {
        private readonly IHomeViewModel _vm;
        public Home(IHomeViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
            _vm = vm;

            Observable.FromEventPattern<NotifyCollectionChangedEventArgs>(_vm.AvailablePackagesViewModel.Packages, "CollectionChanged")
                .Throttle(TimeSpan.FromMilliseconds(50))
                .Distinct()
                .ObserveOnDispatcher()
                .Subscribe(ev => Packages_CollectionChanged());
        }

        void Packages_CollectionChanged()
        {
            AvailablePackagesList.Items.SortDescriptions.Clear();
            if(!string.IsNullOrWhiteSpace(_vm.AvailablePackagesViewModel.SortColumn))
                AvailablePackagesList.Items.SortDescriptions.Add(new SortDescription(_vm.AvailablePackagesViewModel.SortColumn, _vm.AvailablePackagesViewModel.SortDescending ? ListSortDirection.Descending : ListSortDirection.Ascending));

            foreach (var column in AvailablePackagesList.Columns)
            {
                if (column.GetSortMemberPath() == _vm.AvailablePackagesViewModel.SortColumn)
                {
                    column.SortDirection = _vm.AvailablePackagesViewModel.SortDescending ? ListSortDirection.Descending : ListSortDirection.Ascending;
                }
                else
                    column.SortDirection = null;
            }

        }

        private void HandleLinkClick(object sender, RoutedEventArgs e)
        {
            var hl = (Hyperlink)sender;
            var navigateUri = hl.NavigateUri.ToString();
            Process.Start(new ProcessStartInfo(navigateUri));
            e.Handled = true;
        }

        private void AvailablePackagesList_Sorting(object sender, DataGridSortingEventArgs e)
        {
            string sortPropertyName = e.Column.GetSortMemberPath();
            if (!string.IsNullOrEmpty(sortPropertyName))
            {
                bool sortDescending;
                if (!e.Column.SortDirection.HasValue || (e.Column.SortDirection.Value == ListSortDirection.Ascending))
                {
                    sortDescending = true;
                }
                else 
                {
                    sortDescending = false;
                }
                _vm.AvailablePackagesViewModel.SortDescending = sortDescending;
                _vm.AvailablePackagesViewModel.SortColumn = sortPropertyName;
                e.Handled = true;
            }
        }
    }
}
