using ChocoPM.IoC;
using Ninject;
namespace ChocoPM
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public static StandardKernel Kernel = new StandardKernel(new MainModule());
    }
}
