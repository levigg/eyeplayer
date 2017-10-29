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
    /// Interaction logic for VisualKeyboardCapture.xaml
    /// </summary>
    public partial class VisualKeyboardCapture : Window
    {
        public VisualKeyboardCapture()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            imgCapture.Width = 360 * Global.ScreenSize;
            imgCapture.Height = 360 * Global.ScreenSize;
            Canvas.SetTop(imgCapture, System.Windows.Forms.Cursor.Position.Y - 180 * Global.ScreenSize);
            Canvas.SetLeft(imgCapture, System.Windows.Forms.Cursor.Position.X - 180 * Global.ScreenSize);
        }
    }
}
