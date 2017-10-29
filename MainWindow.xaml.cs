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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using WindowsHookSample;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace Eyeplayer
{
    public partial class MainWindow : Window
    {
        [BrowsableAttribute(false)]
        public virtual Size AutoScaleBaseSize { get; set; }

        CheckSerialNumber checkSN;
        TimeZoneInfo timeZoneInfo;
        bool isLimitTime = false; 

        public MainWindow()
        {
            InitializeComponent();
            GlobalSetting.InitializeXMLFile();      
            KeyboardHook.Enabled = true;
            KeyboardHook.GlobalKeyUp += new EventHandler<KeyboardHook.KeyEventArgs>(Global.GetKeyUp);
            KeyboardHook.GlobalKeyDown += new EventHandler<KeyboardHook.KeyEventArgs>(Global.GetKeyDown);
            Global.setBackLight = new SetBackLight();
            Global.setBackLight.Initialize();
            LoadLanguage();
        }

        ResourceDictionary m_resDic;
        private void LoadLanguage()
        {
            Global.strLanguage = System.Globalization.CultureInfo.CurrentCulture.Name;
            switch (Global.strLanguage)
            {
                case "zh-CN":
                    Global.strLanguage = "zh-CN";
                    break;
                case "zh-HK":
                    Global.strLanguage = "zh-HK";
                    break;
                case "zh-SG":
                    Global.strLanguage = "zh-CN";
                    break;
                case "zh-MO":
                    Global.strLanguage = "zh-CN";
                    break;
                case "zh-TW":
                    break;
                case "en-US":
                    break;
              
                default:
                    Global.strLanguage = "en-US"; 
                    break;
            }
            try
            {
                Global.m_resDic = System.Windows.Application.LoadComponent(new Uri("Language/" + Global.strLanguage + ".xaml", UriKind.Relative)) as ResourceDictionary;
            }
            catch(Exception ex)
            {
                Console.WriteLine("LoadLanguage error: " + ex.ToString());
                Global.m_resDic = System.Windows.Application.LoadComponent(new Uri("Language/en-US.xaml", UriKind.Relative)) as ResourceDictionary;
            }
            if (Global.m_resDic != null)
            {
                if (Resources.MergedDictionaries.Count > 0)      Resources.MergedDictionaries.Clear();
                Resources.MergedDictionaries.Add(Global.m_resDic);
            }

            double Xscale = (double)System.Windows.SystemParameters.PrimaryScreenWidth/1920;
            double Yscale = (double)System.Windows.SystemParameters.PrimaryScreenHeight/1080;
            
            BtnScroll.Margin = new Thickness(780 * Xscale, 380 * Yscale, 780 * Xscale, 380 * Yscale);
            BtnSpeak.Margin = new Thickness(550 * Xscale, 210 * Yscale, 1160 * Xscale, 650 * Yscale);
            BtnBell.Margin = new Thickness(1170 * Xscale, 210 * Yscale, 550 * Xscale, 650 * Yscale);
            BtnSetting.Margin = new Thickness(980 * Xscale, 170 * Yscale, 800 * Xscale, 760 * Yscale);
            BtnDrag.Margin = new Thickness(800 * Xscale, 170 * Yscale, 980 * Xscale, 760 * Yscale);
            BtnDoubleClick.Margin = new Thickness(770 * Xscale, 750 * Yscale, 970 * Xscale, 170 * Yscale);
            BtnRightClick.Margin = new Thickness(970 * Xscale, 750 * Yscale, 770 * Xscale, 170 * Yscale);
            BtnPause.Margin = new Thickness(550 * Xscale, 664 * Yscale, 1145 * Xscale, 240 * Yscale);
            BtnSleep.Margin = new Thickness(1140 * Xscale, 664 * Yscale, 550 * Xscale, 240 * Yscale);
        
        }
        System.Windows.Controls.UIElementCollection controlList;
        List<System.Windows.FrameworkElement> controls;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Global.formSize = (double)Screen.PrimaryScreen.Bounds.Width / this.ActualWidth;
            if (isLimitTime)
            {
                DateTime LimitTime = Convert.ToDateTime("2016-12-30");
                timeZoneInfo = new TimeZoneInfo();
                bool pass = false;
                for (int i = 0; i < 5; i++)
                {
                    if (timeZoneInfo.OnRefresh())
                    {
                        DateTime now = DateTime.Now;
                        if (LimitTime.Year > timeZoneInfo.oUTC.Year)
                        {
                            pass = true;
                            break;
                        }
                        else
                        {
                            if (LimitTime.Year == timeZoneInfo.oUTC.Year && LimitTime.DayOfYear > timeZoneInfo.oUTC.DayOfYear)
                            {
                                pass = true;
                                break;
                            }
                            else
                            {
                                System.Windows.MessageBox.Show((string)FindResource("msgOverLimitTime"));
                                Global.CloseProcess();
                                return;
                            }
                        }
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(1000);
                    }
                }
                if (LimitTime.Year >= DateTime.Now.Year)
                {
                    if (LimitTime.Year == timeZoneInfo.oUTC.Year && LimitTime.DayOfYear > DateTime.Now.DayOfYear)
                    {
                        pass = true;
                    }
                    else
                    {
                        System.Windows.MessageBox.Show((string)FindResource("msgOverLimitTime"));
                        Global.CloseProcess();
                        return;
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show((string)FindResource("msgNoInternet"));
                    Global.CloseProcess();
                    return;
                }
                
                if (!pass) {
                    System.Windows.MessageBox.Show((string)FindResource("msgNoInternet"));
                    Global.CloseProcess();
                    return;
                }
            }
            //checkSN = new CheckSerialNumber();
            //if (checkSN.Initialize())
            //{
            //    if (!checkSN.CheckDefaultSN())
            //    {
            //        System.Windows.MessageBox.Show((string)FindResource("msgTrackerError"));
            //        Global.CloseProcess();
            //        return;
            //    }
            //}
            //else
            //{
            //    System.Windows.MessageBox.Show((string)FindResource("msgNoEyeTracker"));
            //    Global.CloseProcess();
            //    return;
            //}
            
            System.Diagnostics.Process[] proc = System.Diagnostics.Process.GetProcessesByName(System.Diagnostics.Process.GetCurrentProcess().ProcessName);
            if (proc.Length > 1) proc[1].Kill();
            Global.main = this;
            Global.clickState = new Global.ClickState();
            Global.clickState = Global.ClickState.Null;
            Global.eyeTracker = new connectEyeTracker();
            Global.eyeTracker.start();
            if (Global.cursorControl == null)
            {
                Global.cursorControl = new CursorControl();
                Global.cursorControl.Show();
            }
            controls = new List<FrameworkElement>();
            controlList = this.grid.Children;
            foreach (System.Windows.FrameworkElement ct in controlList)
            {
                if (ct.Name != "menuBg") controls.Add(ct);
            }

            foreach (System.Windows.FrameworkElement ct in controls)
            {
                ct.MouseEnter += new System.Windows.Input.MouseEventHandler(btnMouseEnter);
                ct.MouseLeave += new System.Windows.Input.MouseEventHandler(btnMouseLeave);
                ((System.Windows.Controls.Button)ct).Click += new RoutedEventHandler(btnMouseClick);
            }
        }
        private void btnMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Global.cursorControl.ControlLoading((System.Windows.FrameworkElement)e.Source);
            
            this.Topmost = false;
            Global.fullScreenEnable = false;
        }
        private void btnMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Global.cursorControl.ControlUnloading((System.Windows.FrameworkElement)e.Source);
        }

        MediaPlayer soundPlayer;
        private void btnMouseClick(object sender, RoutedEventArgs e)
        {
            FrameworkElement ct = (FrameworkElement)e.Source;
            switch (ct.Name)
            {
                case "BtnleftClick":
                    Global.clickState = Global.ClickState.oneClick;
                    this.Visibility = Visibility.Hidden;
                    if (Global.SRF == null)
                    {
                        Global.SRF = new StayResidentFunction();
                        Global.SRF.Show();
                    }
                    Global.clickPage = new Click();
                    Global.clickPage.Show();
                    break;
                case "BtnRightClick":
                    Global.clickState = Global.ClickState.rightClick;
                    this.Visibility = Visibility.Hidden;
                    if (Global.SRF == null)
                    {
                        Global.SRF = new StayResidentFunction();
                        Global.SRF.Show();
                    }
                    Global.clickPage = new Click();
                    Global.clickPage.Show();
                    break;
                case "BtnDoubleClick":
                    Global.clickState = Global.ClickState.doubleClick;
                    this.Visibility = Visibility.Hidden;
                    if (Global.SRF == null)
                    {
                        Global.SRF = new StayResidentFunction();
                        Global.SRF.Show();
                    }
                    Global.clickPage = new Click();
                    Global.clickPage.Show();
                    break;
                case "BtnPause":
                    Global.clickState = Global.ClickState.pause;
                    this.Visibility = System.Windows.Visibility.Hidden;
                    if (Global.SRF == null)
                    {
                        Global.SRF = new StayResidentFunction();
                        Global.SRF.Show();
                    }
                    break;
                case "BtnSpeak":
                    Global.clickState = Global.ClickState.TTS;
                    TextSpeak textSpeak = new TextSpeak();
                    Global.textSpeak = textSpeak;
                    this.Visibility = Visibility.Hidden;
                    textSpeak.Show();
                    System.Windows.Forms.Cursor.Hide();
                    break;
                case "BtnSetting":
                    Global.clickState = Global.ClickState.setting;
                    Global.settingPage = new Setting();
                    this.Visibility = Visibility.Hidden;
                    Global.settingPage.Show();
                    break;
                case "BtnKeyboard":
                    
                    if (Global.KeyboardProcess == null)
                    {
                        try
                        {
                            Global.KeyboardProcess = System.Diagnostics.Process.Start(@"C:\Program Files\Common Files\Microsoft Shared\ink\TabTip.exe");
                            Global.clickVSK = new Click_visualKeyborad();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                            Global.backMenu();
                            return;
                        }
                    }
                   
                    this.Visibility = Visibility.Hidden;
                    System.Threading.Thread.Sleep(1000);
                    Global.clickState = Global.ClickState.visualKeyborad;
                    if (Global.SRF == null)
                    {
                        Global.SRF = new StayResidentFunction();
                        Global.SRF.Show();
                    }
                    break;
                case "BtnScroll":
                    Global.clickState = Global.ClickState.scroll;
                    this.Visibility = Visibility.Hidden;
                    if (Global.SRF == null)
                    {
                        Global.SRF = new StayResidentFunction();
                        Global.SRF.Show();
                    }
                    break;
                case "BtnBell":
                    if (System.IO.File.Exists(Environment.CurrentDirectory + @"\Sounds\help.mp3") && soundPlayer==null)
                    {
                        soundPlayer = new MediaPlayer();
                        Uri path = new Uri(Environment.CurrentDirectory + @"\Sounds\help.mp3", UriKind.Relative);
                        soundPlayer.Open(path);
                        soundPlayer.Volume = 1;
                        soundPlayer.MediaEnded += new EventHandler(HelpBellEnd);
                        soundPlayer.Play();
                        bellTimes = Global.helpBellTimes;
                    }
                    break;
                case "BtnSleep":
                    Global.clickState = Global.ClickState.sleep;
                    Global.pausePage = new PausePage();
                    this.Visibility = System.Windows.Visibility.Hidden;
                    Global.pausePage.Show();
                    break;
                case "BtnDrag":
                    Global.clickState = Global.ClickState.drag;
                    this.Visibility = Visibility.Hidden;
                    if (Global.SRF == null)
                    {
                        Global.SRF = new StayResidentFunction();
                        Global.SRF.Show();
                    }
                    break;
            }
        }
        int bellTimes = 0;
        void HelpBellEnd(object sender, EventArgs e)
        {
            soundPlayer.Stop();
            if (bellTimes > 1)
            {
                bellTimes--;
                soundPlayer.Play();
            }
            else
            {
                soundPlayer.MediaEnded -= new EventHandler(HelpBellEnd);
                soundPlayer.Close();
                soundPlayer = null;
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            System.Windows.Forms.Cursor.Show();
            if (Global.eyeTracker != null) Global.eyeTracker.stop();
            Global.setBackLight.ReturnBrightness();
            if (controls!=null)
            {
                foreach (System.Windows.FrameworkElement ct in controls)
                {
                    ct.MouseEnter -= new System.Windows.Input.MouseEventHandler(btnMouseEnter);
                    ((System.Windows.Controls.Button)ct).Click -= new RoutedEventHandler(btnMouseClick);
                }
            }
            Global.ClearAll();
        }
      
    }
}
