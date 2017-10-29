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

namespace Eyeplayer
{
    /// <summary>
    /// Interaction logic for CopyPage.xaml
    /// </summary>
    public partial class CopyPage : Window
    {
        private const int WsExTransparent = 0x20;
        private const int GwlExstyle = (-20);
        [DllImport("user32", EntryPoint = "SetWindowLong")]
        private static extern uint SetWindowLong(IntPtr hwnd, int nIndex, uint dwNewLong);
        [DllImport("user32", EntryPoint = "GetWindowLong")]
        private static extern uint GetWindowLong(IntPtr hwnd, int nIndex);
        public CopyPage()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Resources.MergedDictionaries.Count > 0) Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(Global.m_resDic);

            strCopy.Content = Global.strTTS;
            SourceInitialized += delegate
            {
                IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
                uint extendedStyle = GetWindowLong(hwnd, GwlExstyle);
                SetWindowLong(hwnd, GwlExstyle, extendedStyle | WsExTransparent);
            };
            this.Topmost = true;
        }
    }
}
