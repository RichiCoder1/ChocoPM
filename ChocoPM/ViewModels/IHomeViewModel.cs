using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ChocoPM.ViewModels
{
    public interface IHomeViewModel
    {
        string SearchQuery { get; set; }
        ObservableCollection<PackageViewModel> Packages { get; set;  }
        PackageViewModel SelectedPackage { get; set; }
        string SortColumn { get; set; }
        bool SortDescending { get; set; }
    }
}
