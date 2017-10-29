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
    /// Interaction logic for TextEdit.xaml
    /// </summary>
    public partial class TextEdit : Window
    {
        private System.Windows.Controls.UIElementCollection controlList;
        private List<Button> btns;
        private bool isClickState = false;
        public TextEdit()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CreateWordButton(ref canvasWords, Global.strTTS);
            if (Global.TTSSelectIndex>0)
                btns[Global.TTSSelectIndex].Foreground = new SolidColorBrush(Colors.Gold);
            controlList = this.zone_1.Children;
            foreach (System.Windows.FrameworkElement ct in controlList)
            {
                ct.MouseEnter += new System.Windows.Input.MouseEventHandler(btnFunctionEnter);
                ct.MouseLeave += new System.Windows.Input.MouseEventHandler(btnFunctionLeave);
                ((System.Windows.Controls.Button)ct).Click += new RoutedEventHandler(btnMouseClick);
            }
            Global.clickState = Global.ClickState.scroll;
            textScrollViewer.ScrollToEnd();
        }
        private void btnFunctionEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Global.clickState = Global.ClickState.Null;
            Global.cursorControl.ControlLoading((System.Windows.FrameworkElement)e.Source);
        }
        private void btnFunctionLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!isClickState) Global.clickState = Global.ClickState.scroll;
            Global.cursorControl.ControlUnloading((System.Windows.FrameworkElement)e.Source);
        }
       
        private void btnMouseClick(object sender, RoutedEventArgs e)
        {
            FrameworkElement ct = (FrameworkElement)e.Source;
            switch (ct.Name)
            {
                case "btnBack":
                    if (btns.Count < 1 ) return;
                    btns.RemoveAt(Global.TTSSelectIndex);
                    if (Global.TTSSelectIndex > 0) Global.TTSSelectIndex--;
                    string tempStr = "";
                    foreach (Button btn in btns)
                        tempStr += btn.Content.ToString();
                    Global.strTTS = tempStr ;
                    CreateWordButton(ref canvasWords, Global.strTTS);

                    if (btns.Count > 0) btns[Global.TTSSelectIndex].Foreground = new SolidColorBrush(Colors.Gold);
                    break;
                case "btnSwitch":
                    if (isClickState)
                    {
                        isClickState = false;
                        Global.clickState = Global.ClickState.scroll;
                        ct.Style = (Style)TryFindResource("styleSrScroll");
                    }
                    else 
                    {
                        isClickState = true;
                        Global.clickState = Global.ClickState.Null;
                        ct.Style = (Style)TryFindResource("styleSrClick");
                    }
                    break;
                case "btnOK":
                    
                    Global.textSpeak = new TextSpeak();
                    Global.textSpeak.Show();
                    this.Close();
                    Global.textEdit = null;
                    Global.clickState = Global.ClickState.TTS;
                    break;
            }
        }
        private void btnMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Global.cursorControl.ControlLoading((System.Windows.FrameworkElement)e.Source);
        }
        private void btnMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Global.cursorControl.ControlUnloading((System.Windows.FrameworkElement)e.Source);
        }
        private void CreateWordButton(ref Canvas canvas,  string words)
        {
            char[] allWords = Global.strTTS.ToCharArray();
            int totalCount = allWords.Length;
            canvas.Children.Clear();
            if (btns!=null)
            {
                foreach (Button b in btns)
                {
                    b.MouseEnter -= new System.Windows.Input.MouseEventHandler(btnMouseEnter);
                    b.MouseLeave -= new System.Windows.Input.MouseEventHandler(btnMouseLeave);
                    b.Click -= new RoutedEventHandler(btnTextClick);
                }
            }
            btns = new List<Button>();
            double width = (canvas.ActualWidth / 10) ;
            int vCount = Convert.ToInt16(Math.Ceiling((double)totalCount / 10));
            if (vCount*width > System.Windows.SystemParameters.PrimaryScreenHeight)
            {
                canvasWords.Height = vCount * width;
                textScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                
            }
            else
            {
                textScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            }
            int count = 0;
            for (int j = 0; j < vCount; j++)
            {
                for (int i = 0; i < 10; i++)
                {
                    Button bt = new Button() { Width = width, Height = width };
                    bt.Opacity = 1;
                    Canvas.SetTop(bt,  j * width);
                    Canvas.SetLeft(bt,  i * width);
                    if (btns.Count == 0)
                    {
                        bt.Content = "";
                        bt.FontSize = 8 / Global.ScreenSize;
                        canvas.Children.Add(bt);
                        bt.Style = (Style)TryFindResource("styleBTN_Lit");
                        btns.Add(bt);
                    }
                    else if (totalCount >= count)
                    {
                        bt.Content = allWords[count-1];
                        bt.FontSize = 80 / Global.ScreenSize;
                        canvas.Children.Add(bt);
                        bt.Style = (Style)TryFindResource("styleBTN_Lit");
                        btns.Add(bt);
                    }
                    //else if (totalCount == count)
                    //{
                    //    bt.Content = "";
                    //    bt.FontSize = 8 / Global.ScreenSize;
                    //    canvas.Children.Add(bt);
                    //    bt.Style = (Style)TryFindResource("styleBTN_Lit");
                    //    btns.Add(bt);
                    //}
                    count++;
                }
            }
            if (btns.Count > 0)
            {
                foreach (Button b in btns)
                {
                    b.MouseEnter += new System.Windows.Input.MouseEventHandler(btnMouseEnter);
                    b.MouseLeave += new System.Windows.Input.MouseEventHandler(btnMouseLeave);
                    b.Click += new RoutedEventHandler(btnTextClick);
                }
            }
        }
        private void btnTextClick(object sender, RoutedEventArgs e)
        {
            FrameworkElement ct = (FrameworkElement)e.Source;
            for (int i = 0; i < btns.Count; i++)
            {
                if (btns[i] == (Button)ct)
                {
                    btns[Global.TTSSelectIndex].Foreground = new SolidColorBrush(Colors.White);
                    Global.TTSSelectIndex = i;
                    btns[Global.TTSSelectIndex].Foreground = new SolidColorBrush(Colors.Gold);
                }
            }
            string tempStr = "";
            foreach (Button btn in btns)
                tempStr += btn.Content.ToString();
            Global.strTTS = tempStr;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Global.textEdit = null;
            string tempStr = "";
            foreach (Button btn in btns)
                tempStr += btn.Content.ToString();
            Global.strTTS = tempStr;
            foreach (System.Windows.FrameworkElement ct in controlList)
            {
                ct.MouseEnter -= new System.Windows.Input.MouseEventHandler(btnFunctionEnter);
                ct.MouseLeave -= new System.Windows.Input.MouseEventHandler(btnFunctionLeave);
                ((System.Windows.Controls.Button)ct).Click -= new RoutedEventHandler(btnMouseClick);
            }
            if (btns.Count > 0)
            {
                foreach (Button b in btns)
                {
                    b.MouseEnter -= new System.Windows.Input.MouseEventHandler(btnMouseEnter);
                    b.MouseLeave -= new System.Windows.Input.MouseEventHandler(btnMouseLeave);
                    b.Click -= new RoutedEventHandler(btnTextClick);
                }
            }
        }
    }
}
