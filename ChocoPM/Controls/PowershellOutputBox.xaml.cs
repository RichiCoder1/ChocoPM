using ChocoPM.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
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
    /// Interaction logic for PowershellOutputBox.xaml
    /// </summary>
    public partial class PowershellOutputBox : UserControl
    {
        public static readonly DependencyProperty BufferProperty = DependencyProperty.Register("Buffer", typeof(ObservableRingBuffer<PowerShellOutputLine>), typeof(PowershellOutputBox),
            new FrameworkPropertyMetadata { DefaultValue = null, PropertyChangedCallback = new PropertyChangedCallback(OnBufferChanged), BindsTwoWayByDefault = true, DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

        private ObservableRingBuffer<PowerShellOutputLine> _oldBuffer;
        public ObservableRingBuffer<PowerShellOutputLine> Buffer
        {
            get { return GetValue<ObservableRingBuffer<PowerShellOutputLine>>(BufferProperty); }
            set { SetValue(BufferProperty, value); }
        }
        private readonly Func<string, string> _getNameHash;
        public PowershellOutputBox()
        {
            InitializeComponent();
            var _hashAlg = MD5.Create();
            _getNameHash = (unhashed) => "_" + _hashAlg.ComputeHash(Encoding.UTF8.GetBytes(unhashed)).Aggregate(new StringBuilder(), (sb, piece) => sb.Append(piece.ToString("X2"))).ToString();
        }

        private static void OnBufferChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            var pob = d as PowershellOutputBox;
            if (pob != null)
                pob.OnBufferChanged(args);
        }

        private void OnBufferChanged(DependencyPropertyChangedEventArgs args)
        {
            if (_oldBuffer != null)
                _oldBuffer.CollectionChanged -= OnBufferUpdated;

            _oldBuffer = Buffer;
            Buffer.CollectionChanged += OnBufferUpdated;
            OnBufferUpdated(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }


        private delegate void RunOnUI();
        private delegate void RunStringOnUI(PowerShellOutputLine line);

        protected void OnBufferUpdated(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Reset)
                App.Current.Dispatcher.Invoke(new RunOnUI(() => this.OutputBox.Inlines.Clear()));
            else if (args.Action == NotifyCollectionChangedAction.Add)
            {
                if (args.NewItems.Count == 1 && args.NewStartingIndex > 0)
                {
                    App.Current.Dispatcher.BeginInvoke(new RunStringOnUI((item) =>
                    {
                        var run = new Run();
                        run.Text = item.Text + Environment.NewLine;
                        run.Name = _getNameHash(item.Text);
                        run.Foreground = item.Type == PowerShellLineType.Output ? Brushes.White : Brushes.Red;
                        run.Background = item.Type == PowerShellLineType.Output ? Brushes.Transparent : Brushes.Black;

                        var beforeString = Buffer[args.NewStartingIndex - 1].Text;
                        var key = _getNameHash(beforeString);
                        var beforeRun = OutputBox.Inlines.FirstOrDefault(inline => inline.Name == key);
                        if (run != null)
                            OutputBox.Inlines.InsertAfter(beforeRun, run);
                        else
                            OutputBox.Inlines.Add(run);

                    }), args.NewItems[0]);
                }

                foreach (PowerShellOutputLine item in args.NewItems)
                {
                    App.Current.Dispatcher.BeginInvoke(new RunStringOnUI((line) =>
                    {
                        var run = new Run();
                        run.Text = line.Text + Environment.NewLine;
                        run.Name = _getNameHash(line.Text);
                        run.Foreground = line.Type == PowerShellLineType.Output ? Brushes.White : Brushes.Red;
                        run.Background = line.Type == PowerShellLineType.Output ? Brushes.Transparent : Brushes.Black;
                        OutputBox.Inlines.Add(run);
                    }), item);
                }
            }
            else if (args.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (PowerShellOutputLine item in args.OldItems)
                {
                    App.Current.Dispatcher.BeginInvoke(new RunStringOnUI((line) =>
                    {
                        var key = _getNameHash(line.Text);
                        var run = OutputBox.Inlines.FirstOrDefault(inline => inline.Name == key);
                        if (run != null)
                            OutputBox.Inlines.Remove(run);
                    }), item);
                }
            }
        }

        protected T GetValue<T>(DependencyProperty dp)
        {
            return (T)GetValue(dp);
        }

        private bool AutoScroll = true;

        private void ScrollViewer_ScrollChanged(Object sender, ScrollChangedEventArgs e)
        {
            // User scroll event : set or unset autoscroll mode
            if (e.ExtentHeightChange == 0)
            {   // Content unchanged : user scroll event
                if (TextScrollViewer.VerticalOffset == TextScrollViewer.ScrollableHeight)
                {   // Scroll bar is in bottom
                    // Set autoscroll mode
                    AutoScroll = true;
                }
                else
                {   // Scroll bar isn't in bottom
                    // Unset autoscroll mode
                    AutoScroll = false;
                }
            }

            // Content scroll event : autoscroll eventually
            if (AutoScroll && e.ExtentHeightChange != 0)
            {   // Content changed and autoscroll mode set
                // Autoscroll
                TextScrollViewer.ScrollToVerticalOffset(TextScrollViewer.ExtentHeight);
            }
        }
    }
}
