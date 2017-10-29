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
    /// Interaction logic for StayResidentFunction.xaml
    /// </summary>
    public partial class StayResidentFunction : Window
    {
        List<System.Windows.FrameworkElement> feSatyResident;
        Global.ClickState tempClickState;
        public System.Windows.Point srBackPos;
        private bool isDragging = false;
        public StayResidentFunction()
        {
            InitializeComponent();
        }
        public void getBackBtnPos()
        {
            srBackPos = srBack.PointToScreen(new System.Windows.Point());
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            Grid.SetColumn(srClick, 5);
            Grid.SetColumn(srChangeSide, 6);
            Grid.SetColumn(srBack, 5);

            this.Topmost = true;
            feSatyResident = new List<FrameworkElement>();
            feSatyResident.Add(this.srClick);
            feSatyResident.Add(this.srChangeSide);
            feSatyResident.Add(this.srBack);
            foreach (FrameworkElement fe in feSatyResident)
            {
                fe.MouseEnter += new MouseEventHandler(btnMouseEnter);
                fe.MouseLeave += new MouseEventHandler(btnMouseLeave);
                ((Button)fe).Click += new RoutedEventHandler(GridBtnClick);
            }
            if (Global.clickState == Global.ClickState.pause)
            {
                srClick.Visibility = System.Windows.Visibility.Hidden;
                srClick.IsEnabled = false;
                srChangeSide.Visibility = System.Windows.Visibility.Hidden;
                srChangeSide.IsEnabled = false;
                if (srBack.Visibility != System.Windows.Visibility.Visible) srBack.Visibility = System.Windows.Visibility.Visible;
                if (!srBack.IsEnabled) srBack.IsEnabled = true;
                srBackPos = srBack.PointToScreen(new System.Windows.Point());
                return;
            }
            if (Global.clickState == Global.ClickState.visualKeyborad)
            {
                srClick.Visibility = System.Windows.Visibility.Hidden;
                srClick.IsEnabled = false;
                srChangeSide.Visibility = System.Windows.Visibility.Hidden;
                srChangeSide.IsEnabled = false;
                Grid.SetRow(srBack, 1);
                if (srBack.Visibility != System.Windows.Visibility.Visible) srBack.Visibility = System.Windows.Visibility.Visible;
                if (!srBack.IsEnabled) srBack.IsEnabled = true;
                return;
            }
            ChangeBtnStyle();
            
            if (!Global.showBackMenu)
            {
                if (srBack.Visibility != System.Windows.Visibility.Hidden) srBack.Visibility = System.Windows.Visibility.Hidden;
                if (srBack.IsEnabled) srBack.IsEnabled = false;
            }
            else
            {
                if (srBack.Visibility != System.Windows.Visibility.Visible) srBack.Visibility = System.Windows.Visibility.Visible;
                if (!srBack.IsEnabled) srBack.IsEnabled = true;
            }
        }
        public void ChangeBtnStyle()
        {
            switch (Global.clickState)
            {
                case Global.ClickState.scroll:
                    this.srClick.Style = (Style)TryFindResource("styleSrScroll");
                    break;
                case Global.ClickState.oneClick:
                    this.srClick.Style = (Style)TryFindResource("styleSrClick");
                    break;
                case Global.ClickState.doubleClick:
                    this.srClick.Style = (Style)TryFindResource("styleDoubleClick");
                    break;
                case Global.ClickState.rightClick:
                    this.srClick.Style = (Style)TryFindResource("styleRight");
                    break;
                case Global.ClickState.drag:
                    this.srClick.Style = (Style)TryFindResource("styleSrDrag");
                    isDragging = true;
                    break;
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
            if (Global.isDragBtnPress) Global.LeftMouseRelease();
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
        private void GridBtnClick(object sender, RoutedEventArgs e)
        {
            this.Topmost = true;
            FrameworkElement ct = (FrameworkElement)e.Source;
            switch (ct.Name)
            {
                case "srClick":
                    if (tempClickState == Global.ClickState.scroll)
                    {
                        Global.clickState = Global.ClickState.oneClick;
                        this.srClick.Style = (Style)TryFindResource("styleSrClick");
                       
                    }
                    else if (tempClickState == Global.ClickState.oneClick || tempClickState == Global.ClickState.doubleClick || tempClickState == Global.ClickState.rightClick)
                    {
                        if (isDragging)
                        {
                            Global.clickState = Global.ClickState.drag;
                            this.srClick.Style = (Style)TryFindResource("styleSrDrag");
                        }
                        else
                        {
                            Global.clickState = Global.ClickState.scroll;
                            this.srClick.Style = (Style)TryFindResource("styleSrScroll");
                        }
                        
                    }
                    else if (tempClickState == Global.ClickState.drag)
                    {
                        Global.clickState = Global.ClickState.oneClick;
                        this.srClick.Style = (Style)TryFindResource("styleSrClickDrag");
                        Global.LeftMouseRelease();
                    }
                    if (Global.clickPage != null)
                    {
                        Global.clickPage.Close();
                        Global.clickPage = null;
                    }
                    break;
                case "srChangeSide":
                    System.Windows.Point relativePoint = srChangeSide.PointToScreen(new System.Windows.Point());
                    if (relativePoint.X > System.Windows.SystemParameters.PrimaryScreenWidth / 2)
                    {
                        Grid.SetColumn(srClick, 1);
                        Grid.SetColumn(srChangeSide, 2);
                        Grid.SetColumn(srBack, 1);
                    }
                    else
                    {
                        Grid.SetColumn(srClick, 5);
                        Grid.SetColumn(srChangeSide, 6);
                        Grid.SetColumn(srBack, 5);
                    }
                    
                    break;
                case "srBack":
                    Global.backMenu();
                    break;
            }
            Global.LeftMouseRelease();
        }
    }
}
