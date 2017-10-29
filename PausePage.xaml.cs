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

using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Eyeplayer
{
    public partial class PausePage : Window
    {
        
        public PausePage()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Global.setBackLight.SetBrightness(100);
        }

        private void BtnleftClick_MouseEnter(object sender, MouseEventArgs e)
        {
            Global.cursorControl.ControlLoading((System.Windows.FrameworkElement)e.Source);
        }

        private void BtnleftClick_MouseLeave(object sender, MouseEventArgs e)
        {
            Global.cursorControl.ControlUnloading((System.Windows.FrameworkElement)e.Source);
        }

        private void BtnleftClick_Click(object sender, RoutedEventArgs e)
        {
            Global.backMenu();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Global.setBackLight.ReturnBrightness();
        }
        

       
    }
}
