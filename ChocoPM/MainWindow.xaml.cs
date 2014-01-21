using ChocoPM.Views;
using ChocoPM.IoC;
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

            var kernel = new StandardKernel(new MainModule());
            PageFrame.Navigate(kernel.Get<Home>());
        }
    }
}
