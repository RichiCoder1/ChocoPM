using ChocoPM.Views;
using ChocoPM.ViewModels;
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
            PageFrame.Navigate(App.Kernel.Get<Home>());
        }
    }
}
