using System.Collections.ObjectModel;

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
