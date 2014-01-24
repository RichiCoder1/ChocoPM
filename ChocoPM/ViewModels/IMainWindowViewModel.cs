using ChocoPM.Controls;
using ChocoPM.Models;

namespace ChocoPM.ViewModels
{
    public interface IMainWindowViewModel
    {
        bool IsProcessing { get; set; }
        ObservableRingBuffer<PowerShellOutputLine> OutputBuffer { get; set; }
    }
}
