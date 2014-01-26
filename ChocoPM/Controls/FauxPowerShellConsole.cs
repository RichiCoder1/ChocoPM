using ChocoPM.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:ChocoPM.Controls"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:ChocoPM.Controls;assembly=ChocoPM.Controls"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:FauxPowerShellConsole/>
    ///
    /// </summary>
    public class FauxPowerShellConsole : RichTextBox
    {       
        public static readonly DependencyProperty BufferProperty = DependencyProperty.Register("Buffer", typeof(ObservableRingBuffer<PowerShellOutputLine>), typeof(FauxPowerShellConsole),
            new FrameworkPropertyMetadata { DefaultValue = null, PropertyChangedCallback = new PropertyChangedCallback(OnBufferChanged), BindsTwoWayByDefault = true, DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

        private ObservableRingBuffer<PowerShellOutputLine> _oldBuffer;
        public ObservableRingBuffer<PowerShellOutputLine> Buffer
        {
            get { return GetValue<ObservableRingBuffer<PowerShellOutputLine>>(BufferProperty); }
            set { SetValue(BufferProperty, value); }
        }
        private readonly Func<string, string> _getNameHash;
        public FauxPowerShellConsole() : base(new FlowDocument())
        {
            var _hashAlg = MD5.Create();
            _getNameHash = (unhashed) => "_" + _hashAlg.ComputeHash(Encoding.UTF8.GetBytes(unhashed)).Aggregate(new StringBuilder(), (sb, piece) => sb.Append(piece.ToString("X2"))).ToString();

            _backingParagraph = new Paragraph();
            Document.Blocks.Add(_backingParagraph);
        }

        private readonly Paragraph _backingParagraph;

        private static void OnBufferChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            var pob = d as FauxPowerShellConsole;
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
                App.Current.Dispatcher.Invoke(new RunOnUI(() => _backingParagraph.Inlines.Clear()));
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
                        var beforeRun = _backingParagraph.Inlines.FirstOrDefault(inline => inline.Name == key);
                        if (run != null)
                            _backingParagraph.Inlines.InsertAfter(beforeRun, run);
                        else
                            _backingParagraph.Inlines.Add(run);

                        //this.Selection.Select(run.ContentStart, run.ContentEnd);
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
                        _backingParagraph.Inlines.Add(run);
                        //this.Selection.Select(run.ContentStart, run.ContentEnd);
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
                        var run = _backingParagraph.Inlines.FirstOrDefault(inline => inline.Name == key);
                        if (run != null)
                            _backingParagraph.Inlines.Remove(run);
                    }), item);
                }
            }
        }

        protected T GetValue<T>(DependencyProperty dp)
        {
            return (T)GetValue(dp);
        }
    }
}
