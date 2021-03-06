﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows;
using WindowsHookSample;
using System.Diagnostics;
using System.Threading;
using EyeXFramework;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Eyeplayer
{
    public class Global
    {
        public static MainWindow main;
        public static ResourceDictionary m_resDic;
        public static string strLanguage = "";          
        public static bool testMode = false;            
        public static bool phyicalButtonMode = false;   
        public static int changeModeTime = 3000;        
        public static System.Windows.Point eyePoint;    
        public static bool isWatching = true;           
        public static System.Windows.Point SmoothPoint; 
        public static bool isFocus = false;             
        public static double ScreenSize = 1;             
        public static double formSize = 1;              
        public static int helpBellTimes = 2;            
        public static System.Windows.Point clickPos;    
        public static EyeXHost eyeXHost;                
        public static StayResidentFunction SRF;         
        public static StayResidentPaste SRP;            
        public static connectEyeTracker eyeTracker;    
        public static ICalibrationViewModel _viewModel;
        public static Calibration calibrationWd;

        public static CursorControl cursorControl;      
        public static System.Diagnostics.Process KeyboardProcess = null;
        public static Click clickPage;
        public static TextSpeak textSpeak;              
        public static TextSpeak_symbol tsSymbol;
        public static string strTTS = "";
        public static int TTSSelectIndex = 0;
        public static TextEdit textEdit;  

        public static bool fullScreenEnable = false;    
        public static System.Windows.Point capturePos;  
        public static System.Windows.Point clickPrePos;    
        public static Setting settingPage;
        public static Click_visualKeyborad clickVSK;    
        public static PausePage pausePage;              
        public static SetBackLight setBackLight;        
        public static SetBackLight.RAMP ramp;           
        public static CopyPage copyPage;                

        public static int clickSpeed = 1000;            
        public static bool showBackMenu = true;
        public static int clickSize = 100;              
        public static int unlockSpeed = 3500;           
        public static int showMenuSpeed = 1500;         

        public static ClickState clickState;            
        public static eyeTrackerStatus trackerStatus;   
        public static bool isGotGaze = false;           
        public static bool defaultCursorType = true;
        public static bool isIS4Device = false; 

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, int dx, int dy, uint cButtons, uint dwExtraInfo);
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        public enum ClickState{
            Null            =0,
            oneClick        =1,
            doubleClick     =2,
            rightClick      =3,
            scroll          =4,
            visualKeyborad  =5,
            pause           =6,
            TTS             =7,
            setting         =8,
            fullScreen      =9,
            copy            =10,
            sleep           =11,
            recalibration   =12,
            enter           =13,
            drag            =14,
            
        }
        public enum eyeTrackerStatus
        {
            Tracking = 0,
            Configuring =1,
            Initializing =2,
            NoTracking =3
        }
        public static void GetKeyUp(object sender, KeyboardHook.KeyEventArgs e)
        {
            switch (e.Key.ToString())
            {
                case "Escape":
                    Console.WriteLine(clickState.ToString());
                    if (Global.main.Visibility == Visibility.Visible && (DateTime.Now-backMenuTime).TotalMilliseconds>500)
                        CloseProcess();
                    else 
                        backMenu();
                    break;
                case "R":
                    backMenu();
                    break;
                case "A":
                    testMode = true;
                    break;
                case "S":
                    clickState = ClickState.scroll;
                    main.Visibility = Visibility.Hidden;
                    if (SRF == null)
                    {
                        SRF = new StayResidentFunction();
                        SRF.Show();
                    }
                    break;
                case "D":
                    Global.clickState = Global.ClickState.TTS;
                    TextSpeak textSpeak = new TextSpeak();
                    Global.textSpeak = textSpeak;
                    main.Visibility = Visibility.Hidden;
                    textSpeak.Show();
                    break;
                case "C":
                    Global.clickState = Global.ClickState.oneClick;
                    main.Visibility = Visibility.Hidden;
                    if (Global.SRF == null)
                    {
                        Global.SRF = new StayResidentFunction();
                        Global.SRF.Show();
                    }
                    if (clickPage == null) clickPage = new Click();
                    clickPage.Show();
                    break;
                case "Insert":
                    isInsertDown = false;
                    break;
            }
        }

        static bool isInsertDown = false;
        public static void GetKeyDown(object sender, KeyboardHook.KeyEventArgs e)
        {
            switch (e.Key.ToString())
            {
                case "Insert":
                    if (!isInsertDown)
                    {
                        isInsertDown = true;
                        MouseClickEvent(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
                    }
                    break;
            }
        }

        public static void CloseProcess()
        {
            System.Windows.Application.Current.Shutdown();
            System.Diagnostics.Process[] proc = System.Diagnostics.Process.GetProcessesByName(System.Diagnostics.Process.GetCurrentProcess().ProcessName);
            foreach (System.Diagnostics.Process p in proc) p.Kill();
        }
        private static System.Windows.Media.MediaPlayer soundPlayer;
        public static void MouseClickEvent(int x, int y)
        {
            cursorControl.cvPiePiece.Children.Clear();
            if (System.IO.File.Exists(Environment.CurrentDirectory + @"\Sounds\click.mp3"))
            {
                soundPlayer = new System.Windows.Media.MediaPlayer();
                Uri path = new Uri(Environment.CurrentDirectory + @"\Sounds\click.mp3", UriKind.Relative);
                soundPlayer.Open(path);
                soundPlayer.Volume = 1;
                soundPlayer.Play();
            }
            switch (clickState)
            {
                case ClickState.Null:
                    LeftMouseClick(new System.Windows.Point(x, y));
                    break;
                case ClickState.pause:
                    //LeftMouseClick(new System.Windows.Point(x, y));
                    break;
                case ClickState.sleep:
                    LeftMouseClick(new System.Windows.Point(x, y));
                    break;
                case ClickState.oneClick:
                    if (clickPage == null)
                    {
                        clickPage = new Click();
                        clickPage.Show();
                    }
                    if (clickPage.ClickEnable)
                    {
                        LeftMouseClick(clickPage.RealPoint(x, y));
                    }
                    else preView(x, y);
                    break;
                case ClickState.rightClick:
                    if (clickPage == null)
                    {
                        clickPage = new Click();
                        clickPage.Show();
                    }
                    if (clickPage.ClickEnable)
                    {
                        
                        RightMOuseClick(clickPage.RealPoint(x, y));
                        clickState = ClickState.oneClick;
                        SRF.ChangeBtnStyle();
                        Thread.Sleep(50);
                    }
                    else { preView(x, y); }
                    
                    break;
                case ClickState.doubleClick:
                    if (clickPage == null)
                    {
                        clickPage = new Click();
                        clickPage.Show();
                    }
                    if (clickPage.ClickEnable)
                    {
                        clickState = ClickState.oneClick;
                        SRF.ChangeBtnStyle();
                        LeftMOuseDoubleClick(clickPage.RealPoint(x, y));
                        Thread.Sleep(50);
                    }
                    else { preView(x, y); }
                    break;
                case ClickState.visualKeyborad:
                    if (clickVSK == null)
                    {
                        clickVSK = new Click_visualKeyborad();
                        clickVSK.Show();
                    }
                    if (clickVSK.ClickEnable)
                    {
                        LeftMouseClick(clickVSK.RealPoint(x, y));
                    }
                    else VisualKeyboradPreview(x, y);
                    break;
                case ClickState.TTS:
                    LeftMouseClick(new System.Windows.Point(x,y));
                    break;
                case ClickState.setting:
                    LeftMouseClick(new System.Windows.Point(x, y));
                    break;
                case ClickState.copy:
                    if (clickPage == null)
                    {
                        clickPage = new Click();
                        clickPage.Show();
                    }
                    if (clickPage.ClickEnable)
                    {
                        LeftMouseClick(clickPage.RealPoint(x, y));
                        System.Windows.Forms.SendKeys.SendWait("^{v}");
                        Thread.Sleep(50);
                    }
                    else { preView(x, y); }
                    break;
                case ClickState.enter:
                    if (clickPage == null)
                    {
                        clickPage = new Click();
                        clickPage.Show();
                    }
                    if (clickPage.ClickEnable)
                    {
                        LeftMouseClick(clickPage.RealPoint(x, y));
                        System.Windows.Forms.SendKeys.SendWait("{ENTER}");
                        Thread.Sleep(50);
                    }
                    else { preView(x, y); }
                    break;
                case ClickState.drag:
                    if (!isDragBtnPress)
                    {
                        if (clickPage == null)
                        {
                            clickPage = new Click();
                            clickPage.Show();
                        }
                        if (clickPage.ClickEnable)
                        {
                            LeftMouseDrag(clickPage.RealPoint(x, y));
                        }
                        else {    preView(x, y);  }
                    }
                    else
                    {    
                        LeftMouseDrag(new System.Windows.Point(x, y));      
                    }
                    break;
            }
        }
        public static void preView(int x, int y)
        {
            ClearClickPage();
            if (clickPage == null) clickPage = new Click();
            clickPage.CaptureImg(x, y);
            clickPage.Show();
            clickPage.Topmost = true;
        }
        public static void VisualKeyboradPreview(int x, int y)
        {
            ClearClickPage();
            clickVSK = new Click_visualKeyborad();
            clickVSK.CaptureImg(x, y);
            clickVSK.Show();
            clickVSK.Topmost = true;
        }
        public static bool isDragBtnPress = false;
        public static void LeftMouseDrag(System.Windows.Point pos)
        {
            Global.testMode = true;
            ClearClickPage();
            SetCursorPos((int)(pos.X), (int)(pos.Y));
            if (!isDragBtnPress)
            {
                mouse_event(MOUSEEVENTF_LEFTDOWN, (int)(pos.X), (int)(pos.Y), 0, 0);
                isDragBtnPress = true;
                Thread.Sleep(600);
            }
            else
            {
                mouse_event(MOUSEEVENTF_LEFTUP, (int)(pos.X), (int)(pos.Y), 0, 0);
                isDragBtnPress = false;
                SRF.Activate();
                SRF.Focus();
                SRF.Topmost = true;
            }
            Global.testMode = false;
        }
        public static void LeftMouseRelease()
        {
            mouse_event(MOUSEEVENTF_LEFTUP, (int)(System.Windows.Forms.Cursor.Position.X), (int)(System.Windows.Forms.Cursor.Position.Y), 0, 0);
            isDragBtnPress = false;
        }
        public static void LeftMouseClick(System.Windows.Point pos)
        {
            Global.testMode = true;
            ClearClickPage();
            SetCursorPos((int)(pos.X), (int)(pos.Y));
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, (int)(pos.X), (int)(pos.Y), 0, 0);
            Global.testMode = false;
        }
        public static void LeftMOuseDoubleClick(System.Windows.Point pos)
        {
            ClearClickPage();
            Global.testMode = true;
            SetCursorPos((int)(pos.X ), (int)(pos.Y ));
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, (int)(pos.X), (int)(pos.Y), 0, 0);
            Thread.Sleep(50);
            SetCursorPos((int)(pos.X ), (int)(pos.Y ));
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, (int)(pos.X), (int)(pos.Y), 0, 0);
            Global.testMode = false;
        }
        public static void RightMOuseClick(System.Windows.Point pos)
        {
            ClearClickPage();
            SetCursorPos((int)(pos.X ), (int)(pos.Y ));
            mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, (int)(pos.X), (int)(pos.Y), 0, 0);
        }
        private static void ClearClickPage()
        {
            try
            {
                
                if (clickPage != null)
                {
                    clickPage.Close();
                    clickPage = null;
                }
                if (clickVSK != null)
                {
                    clickVSK.Close();
                    clickVSK = null;
                }
            }
            catch (Exception ex) { Console.WriteLine("Clear click page error: " + ex.ToString()); }
        }
        private static DateTime backMenuTime;
        public static void backMenu()
        {
            backMenuTime = DateTime.Now;
            if (clickState == ClickState.drag)      LeftMouseRelease();
            cursorControl.EyeControlPoint.Visibility = System.Windows.Visibility.Hidden;
            if (Global.main.Visibility != Visibility.Visible)
            {
                cursorControl.dTimer.IsEnabled = false;
                cursorControl.cvPiePiece.Children.Clear();
                if (System.IO.File.Exists(Environment.CurrentDirectory + @"\Sounds\open.mp3"))
                {
                    System.Windows.Media.MediaPlayer soundPlayer = new System.Windows.Media.MediaPlayer();
                    Uri path = new Uri(Environment.CurrentDirectory + @"\Sounds\open.mp3", UriKind.Relative);
                    soundPlayer.Open(path);
                    soundPlayer.Volume = 1;
                    soundPlayer.Play();
                }
                if (clickState == ClickState.TTS) System.Windows.Forms.Cursor.Show();
                if (clickState == ClickState.copy)
                {
                    clickState = ClickState.TTS;
                    if (copyPage != null)
                    {
                        copyPage.Close();
                        copyPage = null;
                    }
                    if (SRP != null)
                    {
                        SRP.Close();
                        SRP = null;
                    }
                    textSpeak = new TextSpeak();
                    textSpeak.Topmost = true;
                    textSpeak.Show(); 
                    System.Windows.Forms.Cursor.Hide();
                    return;
                }
                if (KeyboardProcess != null)
                {
                    if (Process.GetProcessesByName("TabTip").Length != 0)        foreach (Process p in Process.GetProcessesByName("TabTip"))     p.Kill();
                    KeyboardProcess.Dispose();
                    KeyboardProcess = null;
                }
                ClearClickPage();
                if (textSpeak != null)
                {
                    textSpeak.Close();
                    textSpeak = null;
                }
                if (tsSymbol != null)
                {
                    tsSymbol.Close();
                    tsSymbol = null;
                }
                if (textEdit != null)
                {
                    textEdit.Close();
                    textEdit = null;
                }
                if (settingPage != null)
                {
                    settingPage.Close();
                    settingPage = null;
                }
                if (pausePage != null)
                {
                    pausePage.Close();
                    pausePage = null;
                }
                if (copyPage != null)
                {
                    copyPage.Close();
                    copyPage = null;
                }
                if (SRF != null)
                {
                    SRF.Close();
                    SRF = null;
                }
                if (SRP != null)
                {
                    SRP.Close();
                    SRP = null;
                }
                Global.main.Visibility = Visibility.Visible;
                Global.main.Topmost = true;
            }
            clickState = ClickState.Null;
            
        }
        public static void ClearAll()
        {
            if (Process.GetProcessesByName("TabTip").Length != 0)
                foreach (Process p in Process.GetProcessesByName("TabTip")) p.Kill();
        }
        public static double Distance(System.Windows.Point pointA, System.Windows.Point pointB)
        {
            double dist = Math.Sqrt(Math.Pow(pointB.X - pointA.X, 2) + Math.Pow(pointB.Y - pointA.Y, 2));
            return dist;
        }
        public static void ExitAction()
        {
            if (calibrationWd != null)
            {
                calibrationWd.Close();
                calibrationWd = null;
                _viewModel.Dispose();
                _viewModel = null;
                backMenu();
            }
        }

    }
}
