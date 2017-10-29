//-----------------------------------------------------------------------
// Copyright 2014 Tobii Technology AB. All rights reserved.
//-----------------------------------------------------------------------

namespace Eyeplayer
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Threading;
    using Tobii.Gaze.Core;

    /// <summary>
    /// View model for the calibration window.
    /// </summary>
    internal sealed class CalibrationViewModel : ICalibrationViewModel
    {
        private const double CalibrationNearLimit = 0.3;
        private const double CalibrationFarLimit = 0.7;
        private static readonly Point[] CalibrationPoints = new Point[]
        {
            new Point(0.5, 0.5),
            new Point(0.9, 0.1),
            new Point(0.9, 0.9),
            new Point(0.1, 0.9),
            new Point(0.1, 0.1)
        };

        private Dispatcher _dispatcher;
        private Action _exitAction;
        public IEyeTracker _tracker;
        private int _currentCalibrationPoint;
        
        public CalibrationViewModel(Dispatcher dispatcher, string eyeTrackerUrl, Action exitAction)
        {
            _dispatcher = dispatcher;
            _exitAction = exitAction;
            Stage = Stage.Initializing;
            ContinueCommand = new ActionCommand(Continue);
            ExitCommand = new ActionCommand(exitAction);
            EyePositions = new ObservableCollection<Point>();

            Uri url;
            
            if (eyeTrackerUrl == "--auto")
            {
                url = new EyeTrackerCoreLibrary().GetConnectedEyeTracker();
                if (url == null)
                {
                    Stage = Stage.Error;
                    ErrorMessage = "No eye tracker found.";
                    return;
                }
            }
            else
            {
                try
                {
                    url = new Uri(eyeTrackerUrl);
                }
                catch (UriFormatException)
                {
                    Stage = Stage.Error;
                    ErrorMessage = "Invalid eye tracker URL.";
                    return;
                }
            }            
            InitializeEyeTracker(url);
        }

        
        public event PropertyChangedEventHandler PropertyChanged;

        public Stage Stage { get; private set; }
        public ICommand ContinueCommand { get; private set; }
        public ICommand ExitCommand { get; private set; }
        public string ErrorMessage { get; private set; }
        public ObservableCollection<Point> EyePositions { get; private set; }
        public PositioningStatus PositioningStatus { get; private set; }
        public Point CalibrationDotPosition
        {
            get
            {
                return CalibrationPoints[_currentCalibrationPoint];
            }
        }
        public void CalibrationDotAnimationCompleted()
        {
            Trace.WriteLine(string.Format("Adding calibration point {0}", _currentCalibrationPoint + 1));
           _tracker.AddCalibrationPointAsync(ToPoint2D(CalibrationDotPosition), OnAddCalibrationPointCompleted);
        }
        public void Dispose()
        {
            if (_tracker != null)
            {
                _tracker.Disconnect();
                _tracker.Dispose();
                _tracker = null;
            }
            Global.eyeTracker = new connectEyeTracker();
            Global.eyeTracker.start();
        }

        private static Point2D ToPoint2D(Point p)
        {
            return new Point2D(p.X, p.Y);
        }
        private static Point ToPoint(Point2D p)
        {
            return new Point(p.X, p.Y);
        }
        public void Continue()
        {
            switch (Stage)
            {
                case Stage.PositioningGuide:
                    StartCalibration();
                    break;
                case Stage.CalibrationFailed:
                    StartPositioningGuide();
                    break;
                case Stage.Finished:
                    Dispose();
                    _exitAction();
                    break;
                case Stage.Error:
                    Dispose();
                    _exitAction();
                    break;
                default:
                    break;
            }
        }
        public void exitAction()
        {
            Dispose();
            _exitAction();
        }

        private void StartPositioningGuide()
        {
            Stage = Stage.PositioningGuide;
            OnPropertyChanged("Stage");
        }

        public void StartCalibration()
        {
            Stage = Stage.Calibration;
            _currentCalibrationPoint = 0;
            OnPropertyChanged("Stage");
            OnPropertyChanged("CalibrationDotPosition"); // triggers the animation -- the view should call CalibrationDotAnimationCompleted when it finishes.
        }

        private void InitializeEyeTracker(Uri url)
        {
            Trace.WriteLine("Initializing eye tracker " + url.ToString());
            try
            {
                _tracker = new EyeTracker(url);
            }
            catch (EyeTrackerException ex)
            {
                HandleError(ex.Message);
                return;
            }
            _tracker.EyeTrackerError += OnEyeTrackerError;
            _tracker.GazeData += OnGazeData;
            _tracker.RunEventLoopOnInternalThread(OnGenericOperationCompleted);
            _tracker.ConnectAsync(OnConnectCompleted);
        }

        private void OnGazeData(object sender, GazeDataEventArgs e)
        {
            var left = new Point2D(1 - e.GazeData.Left.EyePositionInTrackBoxNormalized.X, e.GazeData.Left.EyePositionInTrackBoxNormalized.Y);
            var right = new Point2D(1 - e.GazeData.Right.EyePositionInTrackBoxNormalized.X, e.GazeData.Right.EyePositionInTrackBoxNormalized.Y);
            var z = 1.1;
            switch (e.GazeData.TrackingStatus)
            {
                case TrackingStatus.BothEyesTracked:
                    z = (e.GazeData.Left.EyePositionInTrackBoxNormalized.Z + e.GazeData.Right.EyePositionInTrackBoxNormalized.Z) / 2;
                    break;
                case TrackingStatus.OnlyLeftEyeTracked:
                    z = e.GazeData.Left.EyePositionInTrackBoxNormalized.Z;
                    right = new Point2D(double.NaN, double.NaN);
                    break;
                case TrackingStatus.OnlyRightEyeTracked:
                    z = e.GazeData.Right.EyePositionInTrackBoxNormalized.Z;
                    left = new Point2D(double.NaN, double.NaN);
                    break;
                default:
                    left = right = new Point2D(double.NaN, double.NaN);
                    break;
            }
            _dispatcher.BeginInvoke(new Action(() =>
                {
                    SetEyePositions(left, right, z);
                }));
        }

        private void SetEyePositions(Point2D left, Point2D right, double z)
        {
            EyePositions.Clear();

            if (!double.IsNaN(left.X))
            {
                EyePositions.Add(ToPoint(left));
            }

            if (!double.IsNaN(right.X))
            {
                EyePositions.Add(ToPoint(right));
            }

            if (z < CalibrationNearLimit)
            {
                PositioningStatus = PositioningStatus.TooClose;
            }
            else if (z <= CalibrationFarLimit)
            {
                PositioningStatus = PositioningStatus.PositionOk;               
            }
            else
            {
                PositioningStatus = PositioningStatus.TooFarOrNotDetected;
            }

            OnPropertyChanged("PositioningStatus");
        }

        private void OnEyeTrackerError(object sender, EyeTrackerErrorEventArgs e)
        {
            Trace.WriteLine("The eye tracker reported a spurious error.");
            HandleError(e.Message);
        }
        private void OnConnectCompleted(ErrorCode errorCode)
        {
            _tracker.StartCalibrationAsync(OnStartCalibrationCompleted);
        }
        private void OnStartCalibrationCompleted(ErrorCode errorCode)
        {
            _dispatcher.Invoke(new Action(StartPositioningGuide));
            _tracker.StartTrackingAsync(OnGenericOperationCompleted);
        }
        private void OnAddCalibrationPointCompleted(ErrorCode errorCode)
        {
            Trace.WriteLine("Add calibration point completed.");
            if (errorCode != ErrorCode.Success)
            {
                HandleError(Tobii.Gaze.Core.ErrorMessage.GetErrorMessage(errorCode));
                return;
            }
            if (_currentCalibrationPoint + 1 < CalibrationPoints.Length)
            {
                _dispatcher.Invoke(new Action(() =>
                {
                    _currentCalibrationPoint++;
                    OnPropertyChanged("CalibrationDotPosition");
                }));
            }
            else
            {
                _dispatcher.Invoke(new Action(() =>
                {
                    Stage = Stage.ComputingCalibration;
                    OnPropertyChanged("Stage");
                }));
                Trace.WriteLine("Computing and setting calibration.");
                _tracker.ComputeAndSetCalibrationAsync(OnComputeAndSetCalibrationCompleted);
            }
        }

        private void OnComputeAndSetCalibrationCompleted(ErrorCode errorCode)
        {
            _tracker.StopCalibrationAsync(OnStopCalibrationCompleted);
        }

        private void OnStopCalibrationCompleted(ErrorCode errorCode)
        {
            _dispatcher.Invoke(new Action(() =>
            {
                Stage = Stage.Finished;
                OnPropertyChanged("Stage");
            }));
        }

        private void OnGenericOperationCompleted(ErrorCode errorCode)
        {
            Trace.WriteLine("Operation completed.");
        }

        private void HandleError(string message)
        {
            Trace.WriteLine("Error: " + message);

            var action = new Action(() =>
                {
                    Stage = Stage.Error;
                    ErrorMessage = message;
                    OnPropertyChanged("Stage");
                });

            if (_dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                _dispatcher.BeginInvoke(action);
            }
        }
        private void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
