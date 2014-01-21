using System.Windows;

namespace ChocoPM.Controls
{
    /// <summary>
    /// Interaction logic for Pager.xaml
    /// </summary>
    public partial class Pager
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
            return (T)GetValue(prop);
        }
        #endregion
    }
}
