using ChocoPM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocoPM.ViewModels
{
    public interface IHomeViewModel
    {
        AvailablePackagesViewModel AvailablePackagesViewModel { get; }
        InstalledPackagesViewModel InstalledPackagesViewModel { get; }
        IRemoteChocolateyService RemoteService { get; }
        ILocalChocolateyService LocalService { get; }
    }
}
