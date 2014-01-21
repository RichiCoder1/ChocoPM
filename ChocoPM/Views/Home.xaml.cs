using ChocoPM.ViewModels;
using ChocoPM.Extensions;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Collections.Specialized;
using System;
namespace ChocoPM.Views
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : Page
    {
        private IHomeViewModel _vm;
        public Home(IHomeViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
            _vm = vm;

            Observable.FromEventPattern<NotifyCollectionChangedEventArgs>(_vm.Packages, "CollectionChanged")
                .Throttle(TimeSpan.FromMilliseconds(50))
                .Distinct()
                .ObserveOnDispatcher()
                .Subscribe(ev => this.Packages_CollectionChanged());
        }

        void Packages_CollectionChanged()
        {
            AvailablePackagesList.Items.SortDescriptions.Clear();
            if(!string.IsNullOrWhiteSpace(_vm.SortColumn))
                AvailablePackagesList.Items.SortDescriptions.Add(new SortDescription(_vm.SortColumn, _vm.SortDescending ? ListSortDirection.Descending : ListSortDirection.Ascending));

            foreach (var column in AvailablePackagesList.Columns)
            {
                if (column.GetSortMemberPath() == _vm.SortColumn)
                {
                    column.SortDirection = _vm.SortDescending ? ListSortDirection.Descending : ListSortDirection.Ascending;
                }
                else
                    column.SortDirection = null;
            }

        }

        private void HandleLinkClick(object sender, RoutedEventArgs e)
        {
            Hyperlink hl = (Hyperlink)sender;
            string navigateUri = hl.NavigateUri.ToString();
            Process.Start(new ProcessStartInfo(navigateUri));
            e.Handled = true;
        }

        private void AvailablePackagesList_Sorting(object sender, DataGridSortingEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;
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
                _vm.SortDescending = sortDescending;
                _vm.SortColumn = sortPropertyName;
                e.Handled = true;
            }
        }
    }
}
