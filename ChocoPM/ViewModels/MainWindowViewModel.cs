using ChocoPM.Controls;
using ChocoPM.Models;


namespace ChocoPM.ViewModels
{
    public class MainWindowViewModel : ObservableBase, IMainWindowViewModel
    {
        public MainWindowViewModel()
        {
            _outputBuffer = new ObservableRingBuffer<PowerShellOutputLine>(500);
        }

        private bool _isProcessing;
        public bool IsProcessing
        {
            get { return _isProcessing; }
            set { SetPropertyValue(ref _isProcessing, value); }
        }

        private ObservableRingBuffer<PowerShellOutputLine> _outputBuffer;
        public ObservableRingBuffer<PowerShellOutputLine> OutputBuffer
        {
            get { return _outputBuffer; }
            set { SetPropertyValue(ref _outputBuffer, value); }
        }
    }
}
