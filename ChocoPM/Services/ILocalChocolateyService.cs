using ChocoPM.ViewModels;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
namespace ChocoPM.Services
{
    public interface ILocalChocolateyService : INotifyPropertyChanged
    {
        Task<IEnumerable<V2FeedPackage>> GetPackages(bool logOutput = false);
        Task<bool> UninstallPackageAsync(string id, string version);
        Task<bool> InstallPackageAsync(string id, string version = null);
        bool IsInstalled(string id, string version);
        Task<bool> UpdatePackageAsync(string id);
    }
}
