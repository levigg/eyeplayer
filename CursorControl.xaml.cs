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
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;

using Shapes;
using System.Windows.Interop;

namespace Eyeplayer
{
    struct UIEC
    {
        public System.Windows.FrameworkElement element;
        public DateTime HoverTime;
        public DateTime LeaveTime;
    }
    public partial class CursorControl : Window
    {
        private const UInt32 MOUSEEVENTF_WHEEL = 0x0800;
        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, int dwData, uint dwExtraInf);
        #region Allow mouse events to pass through
        const int WS_EX_TRANSPARENT = 0x00000020;
        const int GWL_EXSTYLE = (-20);
        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hwnd, int index);
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);
        public static void SetWindowExTransparent(IntPtr hwnd)
        {
            var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
        }
        #endregion  
        public bool isBtnLoading = false;
        public DispatcherTimer dTimer;
        private double ScreenSize = 1;
        
        public CursorControl()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ScreenSize = (double)1920/System.Windows.SystemParameters.PrimaryScreenWidth;
            Global.ScreenSize = ScreenSize;
           
            dTimer = new DispatcherTimer();
            dTimer.Interval = new TimeSpan(1000*250);
            dTimer.Tick += new EventHandler(dtimer_Tick);
            dTimer.Start();
            focusTime = DateTime.Now;
        }
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowExTransparent(hwnd);
        }
       
        bool isSelectLoading = false;
        DateTime selectStartTime;
        Point selectPos;
        DateTime tickTime;
        DateTime focusTime;
        DateTime missGazeTime;
        void dtimer_Tick(object sender, EventArgs e)
        {
            if (Global.isGotGaze)
            {
                missGazeTime = DateTime.Now;
                Global.isWatching = true;
            }
            else
            {
                if ((DateTime.Now - missGazeTime).TotalMilliseconds > 250)
                {
                    Global.isWatching = false;
                    isBtnLoading = false;
                }
            }
            if (Global.main.WindowState == System.Windows.WindowState.Minimized)
            {
                Global.main.WindowState = System.Windows.WindowState.Normal;
                Global.main.Activate();
            }
            if (this.WindowState == System.Windows.WindowState.Minimized)
            {
                this.WindowState = System.Windows.WindowState.Normal;
                this.Activate();
                this.Topmost = true;
                this.Focus();
            }
            if (Global.clickState == Global.ClickState.visualKeyborad && Process.GetProcessesByName("TabTip").Length==0)
            {
                Global.backMenu();
                dTimer.IsEnabled = false;
            }
            
            if (Global.clickState == Global.ClickState.recalibration)
            {
                if (Global._viewModel.Stage == Stage.PositioningGuide)
                {
                    if ((DateTime.Now - tickTime).TotalSeconds > 5)
                    {
                        if ((DateTime.Now - tickTime).TotalSeconds > 5.2)
                        {
                            tickTime = DateTime.Now;
                            return;
                        }
                        tickTime = DateTime.Now;
                        Global._viewModel.StartCalibration();
                        return;
                    }
                }
                else if (Global._viewModel.Stage == Stage.Finished || Global._viewModel.Stage == Stage.Error || Global._viewModel.Stage == Stage.CalibrationFailed)
                {
                    Global._viewModel.Continue();
                }
                focusTime = DateTime.Now;
                return;
            }
            if (Global.isWatching || Global.clickState == Global.ClickState.sleep)
            {
                tickTime = DateTime.Now;
                if (Global.trackerStatus == Global.eyeTrackerStatus.Tracking)
                {
                    if (Global.main.Visibility == Visibility.Visible)
                    {
                        if (isBtnLoading) focusTime = DateTime.Now; 
                        if ((tickTime - focusTime).TotalMilliseconds > Global.unlockSpeed)
                        {
                            Global.main.Visibility = Visibility.Hidden;
                            Global.clickState = Global.ClickState.recalibration;
                            Global.eyeXHost.Dispose();
                            Global._viewModel = new CalibrationViewModel(Dispatcher, "--auto", new Action(Global.ExitAction));
                            Global.calibrationWd = new Calibration() { DataContext = Global._viewModel, Visibility = System.Windows.Visibility.Visible };
                            focusTime = DateTime.Now;
                            return;
                        }
                    }
                    else
                    {
                        focusTime = DateTime.Now;   
                    }
                }
                else
                {
                    focusTime = DateTime.Now; 
                }
            }
            else
            {
                focusTime = DateTime.Now; 
                if (Global.trackerStatus == Global.eyeTrackerStatus.Tracking )
                {
                    if ((DateTime.Now - tickTime).TotalMilliseconds > Global.showMenuSpeed)
                    {
                        tickTime = DateTime.Now;
                        if (Global.clickState != Global.ClickState.Null && Global.main.Visibility != Visibility.Visible) System.Windows.Forms.SendKeys.SendWait("{ESC}");
                        else Global.backMenu();
                        dTimer.IsEnabled = false;
                    }
                }
                return;
            }
            if (Global.clickState == Global.ClickState.scroll)
            {
                selectDTime = DateTime.Now;
                if (Global.textEdit != null)
                {
                    if (System.Windows.Forms.Cursor.Position.Y < System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height / 4)
                    {
                        mouse_event(MOUSEEVENTF_WHEEL, 0, 0, 1, 0);
                        System.Threading.Thread.Sleep(100);
                    }
                    if (System.Windows.Forms.Cursor.Position.Y > System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height * 3 / 4)
                    {
                        mouse_event(MOUSEEVENTF_WHEEL, 0, 0, -1, 0);
                        System.Threading.Thread.Sleep(100);
                    }
                }
                else
                {
                    if (System.Windows.Forms.Cursor.Position.Y < System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height / 3)
                        mouse_event(MOUSEEVENTF_WHEEL, 0, 0, (int)((System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height / 3 - System.Windows.Forms.Cursor.Position.Y) / 3) / 10, 0);
                    if (System.Windows.Forms.Cursor.Position.Y > System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height * 2 / 3)
                        mouse_event(MOUSEEVENTF_WHEEL, 0, 0, (int)((System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height * 2 / 3 - System.Windows.Forms.Cursor.Position.Y) / 3) / 10, 0);
                }
            }
            if (Global.clickState == Global.ClickState.pause)
            {
                Global.SRF.getBackBtnPos();
                return;
            }
            if (Global.clickState == Global.ClickState.Null || Global.clickState == Global.ClickState.sleep)
            {
                if (isBtnLoading && !Global.phyicalButtonMode)
                {
                    if (this.Visibility != System.Windows.Visibility.Visible) this.Visibility = System.Windows.Visibility.Visible;
                    if ((DateTime.Now - selectDTime).TotalMilliseconds > Global.clickSpeed)
                    {
                        selectStartTime = DateTime.Now;
                        selectDTime = DateTime.Now;
                        isBtnLoading = false;
                        cvPiePiece.Children.Clear();
                        EyeControlPoint.Visibility = System.Windows.Visibility.Hidden;
                        Global.MouseClickEvent((int)(Global.clickPos.X * Global.formSize), (int)(Global.clickPos.Y * Global.formSize));
                        return;
                    }
                    else
                    {
                        if (Global.defaultCursorType)
                        {
                            SettingPiePiece(cvPiePiece, new System.Windows.Point((int)Global.clickPos.X, (int)Global.clickPos.Y), (double)((DateTime.Now - selectDTime).TotalMilliseconds / Global.clickSpeed), 0.8);
                        }
                        else
                        {
                            DrawCirclePoint(cvPiePiece, new System.Windows.Point((int)Global.clickPos.X, (int)Global.clickPos.Y), (double)((DateTime.Now - selectDTime).TotalMilliseconds / Global.clickSpeed), radiusPiepiece);
                        }
                    }
                }
                else
                {
                    selectDTime = DateTime.Now;
                }
            }
            else if (Global.clickState == Global.ClickState.oneClick || Global.clickState == Global.ClickState.doubleClick || Global.clickState == Global.ClickState.copy || Global.clickState == Global.ClickState.enter
                || Global.clickState == Global.ClickState.rightClick || Global.clickState == Global.ClickState.visualKeyborad || Global.clickState == Global.ClickState.drag)
            {
                if (isSelectLoading && !Global.phyicalButtonMode)
                {
                    if (this.Visibility != System.Windows.Visibility.Visible) this.Visibility = System.Windows.Visibility.Visible;
                    selectPos = new Point((int)(System.Windows.Forms.Cursor.Position.X / Global.formSize), (int)(System.Windows.Forms.Cursor.Position.Y / Global.formSize));
                    if ((DateTime.Now - selectStartTime).TotalMilliseconds > Global.clickSpeed)
                    {
                        EyeControlPoint.Visibility = System.Windows.Visibility.Hidden;
                        if (cvPiePiece.Children.Count>0)
                        {
                            cvPiePiece.Children.Clear();
                            return;
                        }
                        isSelectLoading = false;
                        selectStartTime = DateTime.Now;
                        selectDTime = DateTime.Now;
                        Global.MouseClickEvent((int)(selectPos.X * Global.formSize), (int)(selectPos.Y * Global.formSize));
                        return;
                    }
                    else
                    {
                        if (!Global.isFocus)
                        {
                            isSelectLoading = false;
                            cvPiePiece.Children.Clear();
                        }
                        if (Global.defaultCursorType)
                        {
                            if ((double)((DateTime.Now - selectStartTime).TotalMilliseconds / Global.clickSpeed) > 0.08)
                                SettingPiePiece(cvPiePiece, new System.Windows.Point((int)(selectPos.X), (int)(selectPos.Y)), (double)((DateTime.Now - selectStartTime).TotalMilliseconds / Global.clickSpeed));
                        }
                        else
                        {
                            DrawCirclePoint(cvPiePiece, new System.Windows.Point((int)(selectPos.X), (int)(selectPos.Y)), (double)((DateTime.Now - selectStartTime).TotalMilliseconds / Global.clickSpeed));
                        }
                    }
                }
                else
                {
                    cvPiePiece.Children.Clear();
                    isSelectLoading = true;
                    selectStartTime = DateTime.Now;
                }
            }
            else if (Global.clickState == Global.ClickState.setting)
            {
                if (Global.settingPage == null) return;
                if (isBtnLoading && !Global.phyicalButtonMode)
                {
                    if (this.Visibility != System.Windows.Visibility.Visible) this.Visibility = System.Windows.Visibility.Visible;
                    if ((DateTime.Now - selectDTime).TotalMilliseconds > Global.clickSpeed)
                    {
                        selectStartTime = DateTime.Now;
                        selectDTime = DateTime.Now;
                        cvPiePiece.Children.Clear();
                        EyeControlPoint.Visibility = System.Windows.Visibility.Hidden;
                        Global.MouseClickEvent((int)(Global.clickPos.X * Global.formSize), (int)(Global.clickPos.Y * Global.formSize));
                        return;
                    }
                    else
                    {
                        if (Global.defaultCursorType)
                        {
                            SettingPiePiece(cvPiePiece, new System.Windows.Point((int)Global.clickPos.X, (int)Global.clickPos.Y), (double)((DateTime.Now - selectDTime).TotalMilliseconds / Global.clickSpeed), 0.8);
                        }
                        else
                        {
                            DrawCirclePoint(cvPiePiece, new System.Windows.Point((int)Global.clickPos.X, (int)Global.clickPos.Y), (double)((DateTime.Now - selectDTime).TotalMilliseconds / Global.clickSpeed), radiusPiepiece);
                        }
                    }
                }
                else
                {
                    selectDTime = DateTime.Now;
                }
            }
            else if (Global.clickState == Global.ClickState.TTS)
            {
                if (Global.textSpeak != null)
                {
                    if (Global.textSpeak.Visibility == System.Windows.Visibility.Visible)
                        Global.textSpeak.CheckBtnDuringTime();
                }
                if (Global.tsSymbol != null)
                {
                    if (Global.tsSymbol.Visibility == System.Windows.Visibility.Visible)
                        Global.tsSymbol.CheckBtnDuringTime();
                }
                if (isBtnLoading && !Global.phyicalButtonMode)
                {
                    if (this.Visibility != System.Windows.Visibility.Visible) this.Visibility = System.Windows.Visibility.Visible;
                    if (Global.textSpeak != null)
                    {
                        if (Global.textSpeak.ctsTotalTime[Global.textSpeak.lastBtnSelect] > Global.clickSpeed)
                        {
                            selectStartTime = DateTime.Now;
                            selectDTime = DateTime.Now;
                            Global.textSpeak.ClearBtnDuringTime();
                            cvPiePiece.Children.Clear();
                            EyeControlPoint.Visibility = System.Windows.Visibility.Hidden;
                            Global.MouseClickEvent((int)(centerPt.X* Global.formSize), (int)(centerPt.Y* Global.formSize));
                            return;
                        }
                        else
                        {
                            if (Global.defaultCursorType)
                            {
                                SettingPiePiece(cvPiePiece, centerPt, Global.textSpeak.ctsTotalTime[Global.textSpeak.lastBtnSelect] / Global.clickSpeed, 0.6);
                            }
                            else
                            {
                                DrawCirclePoint(cvPiePiece, centerPt, (double)(Global.textSpeak.ctsTotalTime[Global.textSpeak.lastBtnSelect] / Global.clickSpeed), radiusPiepiece*1.2);
                            }
                        }
                    }
                    if (Global.tsSymbol != null)
                    {
                        if (Global.tsSymbol.Visibility == System.Windows.Visibility.Visible)
                        {
                            if (Global.tsSymbol.ctsTotalTime[Global.tsSymbol.lastBtnSelect] > Global.clickSpeed)
                            {
                                selectStartTime = DateTime.Now;
                                selectDTime = DateTime.Now;
                                Global.tsSymbol.ClearBtnDuringTime();
                                cvPiePiece.Children.Clear();
                                EyeControlPoint.Visibility = System.Windows.Visibility.Hidden;
                                Global.MouseClickEvent((int)(centerPt.X* Global.formSize), (int)(centerPt.Y* Global.formSize));
                                return;
                            }
                            else
                            {
                                if (Global.defaultCursorType)
                                {
                                    SettingPiePiece(cvPiePiece, centerPt, Global.tsSymbol.ctsTotalTime[Global.tsSymbol.lastBtnSelect] / Global.clickSpeed, 0.6);
                                }
                                else
                                {
                                    DrawCirclePoint(cvPiePiece, centerPt, (double)(Global.tsSymbol.ctsTotalTime[Global.tsSymbol.lastBtnSelect] / Global.clickSpeed), radiusPiepiece * 1.2);
                                }

                            }
                        }
                    }
                }
                else
                {
                    selectDTime = DateTime.Now;
                    cvPiePiece.Children.Clear();
                }
            }
        }
        DateTime selectDTime,leaveBtnTime;
        System.Windows.FrameworkElement selected;
        public void ControlLoading(System.Windows.FrameworkElement ctr)
        {
            isBtnLoading = true;

            Window window = Window.GetWindow(ctr);
            Point point = ctr.TransformToAncestor(window).Transform(new Point(0, 0)); 
            System.Windows.Point centerPt = new System.Windows.Point(point.X + ctr.ActualWidth / 2, point.Y + ctr.ActualHeight / 2);

            if (ctr.ActualHeight > ctr.ActualWidth) radiusPiepiece = (int)ctr.ActualWidth / 2;
            else                radiusPiepiece = (int)ctr.ActualHeight / 2;
            leaveBtnTime = DateTime.Now;
            Global.clickPos = centerPt;
            if (selected != ctr)
            {
                selectDTime = DateTime.Now;
                selected = ctr;
            }
        }
        Point tempPoint = new Point();
        public void DrawCirclePoint(Canvas cv, System.Windows.Point centerPt, double processing)
        {
            if (tempPoint == centerPt) return;
            else tempPoint = centerPt;

            cv.Visibility = System.Windows.Visibility.Visible;
            double pointSize = System.Windows.SystemParameters.PrimaryScreenWidth / 30;
            if (processing < 0.05)
            {
                if (cv.Children.Count != 0) cv.Children.Clear();
                EyeControlPoint.Width = pointSize * 2;
                EyeControlPoint.Height = pointSize * 2;
                Canvas.SetTop(EyeControlPoint, centerPt.Y - pointSize);
                Canvas.SetLeft(EyeControlPoint, centerPt.X - pointSize);
                EyeControlPoint.Visibility = System.Windows.Visibility.Visible;
                return;
            }
            else
            {
                EyeControlPoint.Visibility = System.Windows.Visibility.Hidden;
            }
            if (cv.Children.Count != 0) cv.Children.Clear();
            if (processing <= 1)
            {
                if (processing >= 0.92)
                {
                    PiePiece pieceFinal = new PiePiece()
                    {
                        Radius = pointSize*(processing-0.91)*10,
                        InnerRadius = pointSize * (processing - 0.91) * 9,
                        CentreX = centerPt.X,
                        CentreY = centerPt.Y,
                        WedgeAngle = 359.99,
                        RotationAngle = 0,
                        Fill = System.Windows.Media.Brushes.Gold,
                    };
                    pieceFinal.Opacity = 0.9 - ((processing-0.8)*4);
                    
                    cv.Children.Insert(0, pieceFinal);
                    return;
                }
                
                double radius = pointSize*(1.1-processing);
                PiePiece piece = new PiePiece()
                {
                    Radius = radius,
                    InnerRadius = 0,
                    CentreX = centerPt.X,
                    CentreY = centerPt.Y,
                    WedgeAngle = 359.99,
                    RotationAngle = 0,
                    Fill = System.Windows.Media.Brushes.Gold,
                };
                piece.Opacity = 0.3+(0.5*processing);
                cv.Children.Insert(0, piece);
            }
        }
        public void DrawCirclePoint(Canvas cv, System.Windows.Point centerPt, double processing, double controlerSize)
        {
            EyeControlPoint.Visibility = System.Windows.Visibility.Hidden;
            cv.Visibility = System.Windows.Visibility.Visible;
            double radius = controlerSize * (1.1 - processing) * 0.8;
            if (cv.Children.Count != 0) cv.Children.Clear();
            if (processing <= 1)
            {
                if (processing >= 0.72)
                {
                    PiePiece pieceFinal = new PiePiece()
                    {
                        Radius = radius * (processing - 0.71) * 10,
                        InnerRadius = radius * (processing - 0.71) * 9,
                        CentreX = centerPt.X,
                        CentreY = centerPt.Y,
                        WedgeAngle = 359.99,
                        RotationAngle = 0,
                        Fill = System.Windows.Media.Brushes.Gold,
                    };
                    pieceFinal.Opacity = 0.9 - ((processing - 0.8) * 2);
                    
                    cv.Children.Insert(0, pieceFinal);
                    return;
                }
                PiePiece piece = new PiePiece()
                {
                    Radius = radius,
                    InnerRadius = 0,
                    CentreX = centerPt.X,
                    CentreY = centerPt.Y,
                    WedgeAngle = 359.99,
                    RotationAngle = 0,
                    Fill = System.Windows.Media.Brushes.Gold,
                };
                piece.Opacity = 0.3 + (0.5 * processing);
                cv.Children.Insert(0, piece);
            }
        }
        public void SettingPiePiece(Canvas cv, System.Windows.Point centerPt, double processing)
        {
            cv.Visibility = System.Windows.Visibility.Visible;
            if (processing <= 1)
            {
                if (cv.Children.Count != 0) cv.Children.Clear();
                double radius = System.Windows.SystemParameters.PrimaryScreenWidth/ 40;
                PiePiece piece = new PiePiece()
                {
                    Radius = radius,
                    InnerRadius = radius - (int)(radius / 2),
                    CentreX = centerPt.X,
                    CentreY = centerPt.Y,
                    WedgeAngle = processing * 360,
                    RotationAngle = 0,
                    Fill = System.Windows.Media.Brushes.Gold,
                };
                piece.Opacity = 0.5;
                cv.Children.Insert(0, piece);
            }
        }
        public void SettingPiePiece(Canvas cv, System.Windows.Point centerPt, double processing,double size)
        {
            cv.Visibility = System.Windows.Visibility.Visible;
            if (processing <= 1)
            {
                if(cv.Children.Count!=0) cv.Children.Clear();
                double radius = radiusPiepiece * size;
                PiePiece piece = new PiePiece()
                {
                    Radius = radius,
                    InnerRadius = radius - (int)(radius / 2),
                    CentreX = centerPt.X,
                    CentreY = centerPt.Y,
                    WedgeAngle = processing * 360,
                    RotationAngle = 0,
                    Fill = System.Windows.Media.Brushes.Gold,
                };
                piece.Opacity = 0.5;
                cv.Children.Insert(0, piece);
            }
        }
        System.Windows.Point centerPt;
        int radiusPiepiece = 0;
        public void SettingLoading(System.Windows.FrameworkElement ctr)
        {
            isBtnLoading = true;
            radiusPiepiece = (int)Global.clickSize / 2;
            Window window = Window.GetWindow(ctr);
            Point relativePoint = ctr.TransformToAncestor(window).Transform(new Point(0, 0)); 
            centerPt = new System.Windows.Point(relativePoint.X + ctr.ActualWidth / 2, relativePoint.Y + ctr.ActualHeight / 2);
            leaveBtnTime = DateTime.Now;
            Global.clickPos = centerPt;
            if (selected != ctr)
            {
                selectDTime = DateTime.Now;
                selected = ctr;
            }
        }
        public void ControlUnloading(System.Windows.FrameworkElement ctr)
        {
            isBtnLoading = false;
            cvPiePiece.Children.Clear(); 
            selectDTime = DateTime.Now;
            selectStartTime = DateTime.Now;
        }
        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if ((DateTime.Now - leaveBtnTime).TotalMilliseconds > 100)
            {
                isBtnLoading = false;
                selectDTime = DateTime.Now;
            }
        }
        private void ExitAction()
        {
            if (Global.calibrationWd != null)
            {
                Global.calibrationWd.Close();
                Global.calibrationWd = null;
                Global._viewModel.Dispose();
                Global._viewModel = null;
            }
        }
    }
}
