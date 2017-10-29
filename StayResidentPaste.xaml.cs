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

namespace Eyeplayer
{
    /// <summary>
    /// Interaction logic for StayResidentPaste.xaml
    /// </summary>
    public partial class StayResidentPaste : Window
    {
        List<System.Windows.FrameworkElement> feSatyResident;
        Global.ClickState tempClickState;
        public StayResidentPaste()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Grid.SetColumn(srEnter, 5);
            Grid.SetColumn(srChangeSide, 6);
            Grid.SetColumn(srBackTTS, 5);
            this.Topmost = true;
            feSatyResident = new List<FrameworkElement>();
            feSatyResident.Add(this.srEnter);
            feSatyResident.Add(this.srChangeSide);
            feSatyResident.Add(this.srBackTTS);
            foreach (FrameworkElement fe in feSatyResident)
            {
                fe.MouseEnter += new MouseEventHandler(btnMouseEnter);
                fe.MouseLeave += new MouseEventHandler(btnMouseLeave);
                ((Button)fe).Click += new RoutedEventHandler(btnClick);
            }
            if (Global.clickState == Global.ClickState.copy)
            {
                this.srEnter.Style = (Style)TryFindResource("styleSrPaste");
            }
            else
            {
                this.srEnter.Style = (Style)TryFindResource("styleSrEnter");
            }
        }

        private void btnMouseEnter(object sender, MouseEventArgs e)
        {
            this.Focus();
            this.Topmost = false;
            if (Global.clickState != Global.ClickState.Null)
            {
                tempClickState = Global.clickState;
                Global.clickState = Global.ClickState.Null;
                Global.cursorControl.ControlUnloading((System.Windows.FrameworkElement)e.Source);
            }
            Global.cursorControl.ControlLoading((System.Windows.FrameworkElement)e.Source);
        }

        private void btnMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.Topmost = true;
            if (Global.clickState == Global.ClickState.Null)
            {
                Global.clickState = tempClickState;
            }
            Global.cursorControl.ControlUnloading((System.Windows.FrameworkElement)e.Source);
        }

        private void btnClick(object sender, RoutedEventArgs e)
        {
            this.Topmost = true;
            if (Global.clickState == Global.ClickState.Null)
            {
                Global.clickState = tempClickState;
            }
            FrameworkElement ct = (FrameworkElement)e.Source;
            switch (ct.Name)
            {
                case "srEnter":
                    if (Global.copyPage != null)
                    {
                        Global.copyPage.Close();
                        Global.copyPage = null;
                    }
                    if (Global.clickState == Global.ClickState.copy)
                    {
                        this.srEnter.Style = (Style)TryFindResource("styleSrEnter");
                        Global.clickState = Global.ClickState.enter;
                        
                        System.Windows.Forms.SendKeys.SendWait("{ENTER}");
                        
                    }
                    else
                    {
                        this.srEnter.Style = (Style)TryFindResource("styleSrPaste");
                        Global.clickState = Global.ClickState.copy;
                        if (Global.copyPage == null)
                        {
                            Global.copyPage = new CopyPage();
                            Global.copyPage.Show();
                        }
                    }
                    if (Global.clickPage == null)
                    {
                        Global.clickPage = new Click();
                        Global.clickPage.Show();
                    }
                    break;
                case "srChangeSide":

                    System.Windows.Point relativePoint = srChangeSide.PointToScreen(new System.Windows.Point());
                    if (relativePoint.X > System.Windows.SystemParameters.PrimaryScreenWidth / 2)
                    {
                        Grid.SetColumn(srEnter, 1);
                        Grid.SetColumn(srChangeSide, 2);
                        Grid.SetColumn(srBackTTS, 1);
                    }
                    else
                    {
                        Grid.SetColumn(srEnter, 5);
                        Grid.SetColumn(srChangeSide, 6);
                        Grid.SetColumn(srBackTTS, 5);
                    }
                    if (Global.clickPage == null)
                    {
                        Global.clickPage = new Click();
                        Global.clickPage.Show();
                    }
                    break;
                case "srBackTTS":
                    Global.clickState = Global.ClickState.copy;
                    Global.SRP.Close();
                    Global.SRP = null;
                    Global.backMenu();
                    break;
            }
        }
    }
}
