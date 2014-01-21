using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChocoPM.Controls
{
    /// <summary>
    /// Interaction logic for Pager.xaml
    /// </summary>
    public partial class Pager : UserControl
    {

        public static readonly DependencyProperty CurrentIndexProperty = RegisterProperty<int>("CurrentIndex");

        public int CurrentIndex
        {
            get { return GetValue<int>(CurrentIndexProperty); }
            set { SetValue(CurrentIndexProperty, value); }
        }

        public static readonly DependencyProperty CountProperty = RegisterProperty<long>("Count");

        public long Count
        {
            get { return GetValue<long>(CountProperty); }
            set { SetValue(CountProperty, value); }
        }
        
        public Pager()
        {
            InitializeComponent();
        }

        #region Helper Functions
        public static DependencyProperty RegisterProperty<T>(string name)
        {
            return DependencyProperty.Register(name, typeof(T), typeof(Pager));
        }

        public T GetValue<T>(DependencyProperty prop)
        {
            return (T)this.GetValue(prop);
        }
        #endregion
    }
}
