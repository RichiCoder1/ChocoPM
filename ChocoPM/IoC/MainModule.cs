using ChocoPM.ViewModels;
using ChocoPM.Services;

namespace ChocoPM.IoC
{
    public class MainModule : Ninject.Modules.NinjectModule
    {
        public override void Load()
        {
            Bind<IHomeViewModel>().To<HomeViewModel>();
            Bind<IMainWindowViewModel>().To<MainWindowViewModel>().InSingletonScope();
            Bind<ILocalChocolateyService>().To<LocalChocolateyService>().InSingletonScope();
            Bind<IRemoteChocolateyService>().To<RemoteChocolateyService>().InSingletonScope();
        }
    }
}
