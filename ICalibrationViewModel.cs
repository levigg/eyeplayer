using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Eyeplayer
{
    public interface ICalibrationViewModel : INotifyPropertyChanged, IDisposable
    {

        Stage Stage { get; }
        ICommand ContinueCommand { get; }
        ICommand ExitCommand { get; }
        string ErrorMessage { get; }
        ObservableCollection<Point> EyePositions { get; }
        PositioningStatus PositioningStatus { get; }
        Point CalibrationDotPosition { get; }
        void CalibrationDotAnimationCompleted();

        void StartCalibration();
        void Continue();
    }
    public enum Stage
    {
        Initializing,
        PositioningGuide,
        Calibration,
        ComputingCalibration,
        CalibrationFailed,
        Finished,
        Error
    }
    public enum PositioningStatus
    {
        TooClose,
        TooFarOrNotDetected,
        PositionOk
    }
}
