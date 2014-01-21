using ChocoPM.ViewModels;
using ChocoPM.Views;
using ChocoPM.IoC;
using Ninject;
using System.Linq;
using System.Windows;
using MahApps.Metro.Controls;

namespace ChocoPM
{
    /// <summary>
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
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
