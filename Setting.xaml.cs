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
    /// Interaction logic for Setting.xaml
    /// </summary>
    public partial class Setting : Window
    {
        List<Button> BtnList;
        bool tempBtnMode, tempCursorType;
        int tempSpeed;
        public Setting()
        {
            InitializeComponent();
        }
        private int ValueToCount(int value,int max,int min)
        {
            int count =Convert.ToInt16(5 * (value-min) / (max - min));
            return count;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Resources.MergedDictionaries.Count > 0) Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(Global.m_resDic);
            tempBtnMode = Global.phyicalButtonMode;
            tempCursorType = Global.defaultCursorType;
            tempSpeed = Global.clickSpeed;
            BtnList = new List<Button>();
            BtnList.Add(btnEye);
            BtnList.Add(btnBtn);
            BtnList.Add(btnArrow);
            BtnList.Add(btnPoint);
            BtnList.Add(btnPlus);
            BtnList.Add(btnMinus);
            BtnList.Add(saveSetting);
            BtnList.Add(cancelSetting);

            foreach (System.Windows.FrameworkElement ct in BtnList)
            {
                ct.MouseEnter += new System.Windows.Input.MouseEventHandler(btnMouseEnter);
                ct.MouseLeave += new System.Windows.Input.MouseEventHandler(btnMouseLeave);
                ((System.Windows.Controls.Button)ct).Click += new RoutedEventHandler(btnMouseClick);
            }
            btnEye.OverridesDefaultStyle = false;
            imgEye.Source = ((ImageBrush)TryFindResource("imgEyeCtlH")).ImageSource;
            imgBtn.Source = ((ImageBrush)TryFindResource("imgBtnCtlH")).ImageSource;
            imgArrow.Source = ((ImageBrush)TryFindResource("imgArrowH")).ImageSource;
            imgPoint.Source = ((ImageBrush)TryFindResource("imgPointH")).ImageSource;
            if (Global.phyicalButtonMode) imgBtn.Visibility = System.Windows.Visibility.Visible;
            else  imgEye.Visibility = System.Windows.Visibility.Visible;
            if (Global.defaultCursorType) imgArrow.Visibility = System.Windows.Visibility.Visible;
            else imgPoint.Visibility = System.Windows.Visibility.Visible;
            labSec.Content = Math.Round((double)Global.clickSpeed / 1000, 1) + (String)TryFindResource("strSec");
            if (Global.clickSpeed >= GlobalSetting.clickSpeedMax) btnPlus.IsEnabled = false;
            if (Global.clickSpeed <= GlobalSetting.clickSpeedMin) btnMinus.IsEnabled = false;
        }

        private void btnMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Global.cursorControl.ControlLoading((System.Windows.FrameworkElement)e.Source);
            this.Topmost = false;
            if (tempBtnMode && !Global.phyicalButtonMode)
            {
                if (((FrameworkElement)e.Source).Name == "saveSetting")
                {
                    Global.cursorControl.isBtnLoading = false;
                    Global.cursorControl.cvPiePiece.Children.Clear();
                }
            }
            
        }
        private void btnMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Global.cursorControl.ControlUnloading((System.Windows.FrameworkElement)e.Source);
        }
        private void btnMouseClick(object sender,RoutedEventArgs e)
        {
            switch (((FrameworkElement)e.Source).Name)
            {
                case "btnEye":
                    tempBtnMode = false;
                    break;
                case "btnBtn":
                    tempBtnMode = true;
                    break;
                case "btnArrow":
                    Global.defaultCursorType = true;
                    break;
                case "btnPoint":
                    Global.defaultCursorType = false;
                    break;
                case "btnPlus":
                    Global.clickSpeed += 200;
                    if (!btnMinus.IsEnabled) btnMinus.IsEnabled = true;
                    if (Global.clickSpeed >= GlobalSetting.clickSpeedMax) btnPlus.IsEnabled = false;
                    break;
                case "btnMinus":
                    Global.clickSpeed -= 200;
                    if (!btnPlus.IsEnabled) btnPlus.IsEnabled = true;
                    if (Global.clickSpeed <= GlobalSetting.clickSpeedMin) btnMinus.IsEnabled = false;
                    break;
                case "saveSetting":
                    if (Global.phyicalButtonMode != tempBtnMode) Global.phyicalButtonMode = tempBtnMode;
                    GlobalSetting.SaveValue();
                    Global.backMenu();
                    break;
                case "cancelSetting":
                    Global.defaultCursorType = tempCursorType;
                    Global.clickSpeed = tempSpeed;
                    Global.backMenu();
                    break;
            }
            showBtnState();
        }

        private void showBtnState()
        {
            if (tempBtnMode)
            {
                imgBtn.Visibility = System.Windows.Visibility.Visible;
                imgEye.Visibility = System.Windows.Visibility.Hidden;
            }
            else 
            {
                imgBtn.Visibility = System.Windows.Visibility.Hidden;
                imgEye.Visibility = System.Windows.Visibility.Visible; 
            }
            if (Global.defaultCursorType)
            {
                imgArrow.Visibility = System.Windows.Visibility.Visible;
                imgPoint.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                imgArrow.Visibility = System.Windows.Visibility.Hidden;
                imgPoint.Visibility = System.Windows.Visibility.Visible;
            }
            labSec.Content = Math.Round((double)Global.clickSpeed / 1000, 1) + (String)TryFindResource("strSec");
        }
        
    }
}
