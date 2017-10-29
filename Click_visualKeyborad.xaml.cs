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
using System.Drawing;
using System.Windows.Interop;

namespace Eyeplayer
{
    public partial class Click_visualKeyborad : Window
    {
        int CaptureW, CaptureH;
        int size = 2;                   
        public ImageSource bitmapSource;
        public bool ClickEnable = false;
        double imgCaptureSizeW, imgCaptureSizeH;
        public Click_visualKeyborad()
        {
            InitializeComponent();
        }
        
        public void CaptureImg(int posX, int posY)
        {
            imgCaptureSizeW = (double)4/2;
            imgCaptureSizeH = (double)540/240;
            if (!ClickEnable)
            {
                CaptureW = Convert.ToInt16(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width / size / imgCaptureSizeW);
                CaptureH = Convert.ToInt16(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height / size / imgCaptureSizeH);
                Global.capturePos = new System.Windows.Point(posX - CaptureW / 2, posY - CaptureH / 2);
                Global.clickPrePos = new System.Windows.Point(posX, posY);
                System.Drawing.Rectangle rect = new System.Drawing.Rectangle(posX - CaptureW / 2, posY - CaptureH / 2, CaptureW, CaptureH);
                Bitmap bmp = new Bitmap(rect.Width, rect.Height);
                Graphics g = Graphics.FromImage(bmp);
                g.CopyFromScreen(rect.Left, rect.Top, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);
                bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                imgCapture.Source = bitmapSource;
            }
        }

        public System.Windows.Point RealPoint(int x, int y)
        {
            System.Windows.Point realpos = new System.Windows.Point(System.Windows.SystemParameters.PrimaryScreenWidth / 2, System.Windows.SystemParameters.PrimaryScreenHeight / 2);
            if (imgCapture.Source != null)
            {
                Window window = Window.GetWindow(imgCapture);
                System.Windows.Point relativePoint = imgCapture.TransformToAncestor(window).Transform(new System.Windows.Point(0, 0));
                System.Windows.Point centerPos = new System.Windows.Point(relativePoint.X + imgCapture.ActualWidth / 2, relativePoint.Y + imgCapture.ActualHeight / 2);
                int realX = (int)(Global.clickPrePos.X + (x - centerPos.X * Global.formSize) / size);
                int realY = (int)(Global.clickPrePos.Y + (y - centerPos.Y * Global.formSize) / size);
                realpos = new System.Windows.Point(realX, realY);
            }
            return realpos;
        }

        private void imgCapture_MouseLeave(object sender, MouseEventArgs e)
        {
            ClickEnable = false;
        }

        private void imgCapture_MouseEnter(object sender, MouseEventArgs e)
        {
            ClickEnable = true;
            Topmost = false;
        }
    }
}
