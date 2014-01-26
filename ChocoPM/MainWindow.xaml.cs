using System;
using System.ComponentModel;
using ChocoPM.Views;
using ChocoPM.ViewModels;
using System.Reactive.Linq;
using Ninject;


namespace ChocoPM
{
    /// <summary>
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class MainWindow
    {
        [Inject]
        public MainWindow()
        {
            InitializeComponent();
            var vm = App.Kernel.Get<IMainWindowViewModel>();
            DataContext = vm;
            Services.LocalChocolateyService._mainWindowVm = vm;

            Observable.FromEventPattern<PropertyChangedEventArgs>(vm, "PropertyChanged")
                .Where(property => property.EventArgs.PropertyName == "IsProcessing")
                .ObserveOnDispatcher()
                .Subscribe(e => { if (vm.IsProcessing) this.PowerShellConsole.Focus(); else this.PageFrame.Focus(); });

            PageFrame.Navigate(App.Kernel.Get<Home>());
        }
    }
}
