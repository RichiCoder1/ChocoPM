using ChocoPM.Models;


namespace ChocoPM.ViewModels
{
    public class MainWindowViewModel : ObservableBase, IMainWindowViewModel
    {
        private bool _isProcessing;
        public bool IsProcessing
        {
            get { return _isProcessing; }
            set { SetPropertyValue(ref _isProcessing, value); }
        }
    }
}
