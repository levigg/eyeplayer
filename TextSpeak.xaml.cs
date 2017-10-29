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
using System.IO;
using System.Reflection;

namespace Eyeplayer
{
    /// <summary>
    /// Interaction logic for TextSpeak.xaml
    /// </summary>
    public partial class TextSpeak : Window
    {
        System.Windows.Controls.UIElementCollection controlList;
        List<System.Windows.FrameworkElement> btnZone2,btnZone0,btnZone1;
        TextToSpeech TTS;
        public State state;// = State.zh_TW;
        public KeyinStep keyinStep = KeyinStep.Null;
        List<Button> btnKeyin;
        public TextSpeak()
        {
            InitializeComponent();
            TTS = new TextToSpeech();
        }
        public enum State
        {
            zh_TW       =0,
            zh_CN       =5,
            zh_HK       =6,
            en_US       =1,
            num         =2,
            function    =3,
            symbol_usual=4,
        }
        public enum KeyinStep
        {
            Null            = 0, 
            zone2Selecting  = 3, 
            zone2Selected   = 1, 
            zone1Selected   = 2, 
            wordSelected    = 4  
        }
        public DateTime[] ctsStartTime;  
        public double []  ctsTotalTime;  
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Resources.MergedDictionaries.Count > 0) Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(Global.m_resDic);

            switch (Global.strLanguage)
            {
                case "zh-TW":
                    LoadWord_zhTW();
                    state = State.zh_TW;
                    break;
                case "zh-CN":
                    LoadWord_zhCN();
                    state = State.zh_CN;
                    break;
                case "zh-HK":
                    LoadWord_zhHK();
                    state = State.zh_HK;
                    break;
                case "en-US":
                    state = State.en_US;
                    break;
            }
            btnZone0 = new List<FrameworkElement>();
            controlList = this.gridZone0.Children;
            foreach (System.Windows.FrameworkElement ct in controlList)
            {
                btnZone0.Add(ct);
                if (ct.Name != "textScrollView")
                    ((Button)ct).FontSize = 50 / Global.ScreenSize;
                else
                    strLab.FontSize = 60 / Global.ScreenSize;
            }

            btnZone1 = new List<FrameworkElement>();
            controlList = this.gridZone1.Children;
            foreach (System.Windows.FrameworkElement ct in controlList)
            {
                btnZone1.Add(ct);
                ((Button)ct).FontSize = 50/Global.ScreenSize;
            }

            btnZone2 = new List<FrameworkElement>();
            controlList = this.gridZone2.Children;
            foreach (System.Windows.FrameworkElement ct in controlList)
            {
                btnZone2.Add(ct);
                ((Button)ct).FontSize = 50/Global.ScreenSize;
            }

            ctsStartTime = new DateTime[btnZone0.Count + btnZone1.Count + btnZone2.Count];
            ctsTotalTime = new double[btnZone0.Count + btnZone1.Count + btnZone2.Count];
            foreach (FrameworkElement ct in btnZone0)
            {
                ct.MouseEnter += new System.Windows.Input.MouseEventHandler(btnMouseEnterZone0);
                ct.MouseLeave += new MouseEventHandler(btnMouseLeave);
                if (ct.Name != "textScrollView")
                {
                    ((Button)ct).Content = "";
                    ((System.Windows.Controls.Button)ct).Click += new RoutedEventHandler(btnMouseZone0Click);
                }
                else
                {
                    ct.MouseUp += new MouseButtonEventHandler(btnMouseZone0Click);
                    strLab.MouseUp += new MouseButtonEventHandler(btnMouseZone0Click);
                }
            }
            foreach (FrameworkElement ct in btnZone1)
            {
                ((Button)ct).Content = "";
                ct.MouseEnter += new System.Windows.Input.MouseEventHandler(btnMouseEnterZone1);
                ct.MouseLeave += new MouseEventHandler(btnMouseLeave);
                ((System.Windows.Controls.Button)ct).Click += new RoutedEventHandler(btnZone1MouseClick);
            }
            foreach (FrameworkElement ct in btnZone2)
            {
                ct.MouseEnter += new System.Windows.Input.MouseEventHandler(btnMouseEnterZone2);
                ct.MouseLeave += new MouseEventHandler(btnMouseLeave);
                ((System.Windows.Controls.Button)ct).Click += new RoutedEventHandler(btnZone2MouseClick);
                ((System.Windows.Controls.Button)ct).Foreground = new SolidColorBrush(Colors.Gray);
            }
            changeBtnContent();
            btnKeyin = new List<Button>();
            keyinStep = KeyinStep.Null;
            zone1FinalWord = Global.strTTS;

            if (Global.TTSSelectIndex > 0) strLab.Content = Global.strTTS.Substring(0, Global.TTSSelectIndex);
            else strLab.Content = "";
            strLab.Content += "|";
            double offset = (double)Global.TTSSelectIndex / Global.strTTS.Length;
            textScrollView.ScrollToHorizontalOffset(Global.TTSSelectIndex*strLab.FontSize/2);
            if (Global.strTTS.Length > Global.TTSSelectIndex) strLab.Content += Global.strTTS.Substring(Global.TTSSelectIndex);
            
            this.Topmost = false;
            Global.clickState = Global.ClickState.TTS;
        }

        public void CheckBtnDuringTime()
        {
            for (int i = 0; i < ctsStartTime.Length; i++)
            {
                if ((DateTime.Now - ctsStartTime[i]).TotalMilliseconds > Global.clickSpeed * 3 && ctsTotalTime[i] > 0)
                {
                    ctsTotalTime[i] = 0;
                    ctsStartTime[i] = DateTime.Now;
                }
            }
            if (!Global.cursorControl.isBtnLoading) return;
            ctsTotalTime[lastBtnSelect] += (DateTime.Now - lastBtnEnterTime).TotalMilliseconds;

            lastBtnEnterTime = DateTime.Now;
        }
        public void ClearBtnDuringTime()
        {
            for (int i = 0; i < ctsStartTime.Length; i++)
            {
                ctsTotalTime[i] = 0;
                ctsStartTime[i] = DateTime.Now;
            }
        }
        private void btnMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Global.cursorControl.isBtnLoading = false;
            if (((System.Windows.FrameworkElement)e.Source).Name == "textScrollView") ((ScrollViewer)(System.Windows.FrameworkElement)e.Source).Background = Brushes.White;
        }

        private int btnSelect(System.Windows.FrameworkElement fe)
        {
            for (int i = 0; i < btnZone0.Count; i++)
                if (fe == btnZone0[i])     
                   return i;
            for (int i = 0; i < btnZone1.Count; i++)
                if (fe == btnZone1[i]) 
                    return i + btnZone0.Count;
            for (int i = 0; i < btnZone2.Count; i++)
                if (fe == btnZone2[i]) 
                    return i + btnZone0.Count + btnZone1.Count;
            return -1;
        }
        DateTime lastBtnEnterTime = DateTime.Now;
        public int lastBtnSelect;
        private void btnMouseEnterZone0(object sender, System.Windows.Input.MouseEventArgs e)
        {
            lastBtnEnterTime = DateTime.Now;
            lastBtnSelect = btnSelect((System.Windows.FrameworkElement)e.Source);
            Global.cursorControl.SettingLoading((System.Windows.FrameworkElement)e.Source);
            if (((System.Windows.FrameworkElement)e.Source).Name == "textScrollView") ((ScrollViewer)(System.Windows.FrameworkElement)e.Source).Background = Brushes.Gray;
        }
        private void btnMouseEnterZone1(object sender, System.Windows.Input.MouseEventArgs e)
        {
            lastBtnEnterTime = DateTime.Now;
            lastBtnSelect = btnSelect((System.Windows.FrameworkElement)e.Source);
            if (((Button)(System.Windows.FrameworkElement)e.Source).Content.ToString() == "")
            {
                if (((Button)(System.Windows.FrameworkElement)e.Source).Style != (Style)TryFindResource("styleTextRight") && ((Button)(System.Windows.FrameworkElement)e.Source).Style != (Style)TryFindResource("styleTextLeft"))
                {
                    Global.cursorControl.isBtnLoading = false;
                    return;
                }
            }
            Global.cursorControl.SettingLoading((System.Windows.FrameworkElement)e.Source);
        }
        private void btnMouseEnterZone2(object sender, System.Windows.Input.MouseEventArgs e)
        {
            lastBtnEnterTime = DateTime.Now;
            lastBtnSelect = btnSelect((System.Windows.FrameworkElement)e.Source);
            if (((Button)(System.Windows.FrameworkElement)e.Source).Name == "")
            {
                Global.cursorControl.isBtnLoading = false;
                return;
            }
            Global.cursorControl.SettingLoading((System.Windows.FrameworkElement)e.Source);
        }
        private void btnMouseZone0Click(object sender, RoutedEventArgs e)
        {
            string strA = "";
            FrameworkElement ct = (FrameworkElement)e.Source;
            switch (ct.Name)
            {
                case "textScrollView":
                    if (Global.strTTS == "") return;
                    if (Global.textEdit == null){
                        Global.textEdit = new TextEdit();
                        Global.textEdit.Show();
                        this.Close();
                        Global.textSpeak = null;
                    }
                    break;
                case "strLab":
                    if (Global.strTTS == "") return;
                    if (Global.textEdit == null)
                    {
                        Global.textEdit = new TextEdit();
                        Global.textEdit.Show();
                        this.Close();
                        Global.textSpeak = null;
                    }
                    break;
                case "btnBack":
                    switch (keyinStep)
                    {
                        case KeyinStep.Null:
                            if (Global.strTTS != "" && strLab.Content.ToString().Split('|')[0].ToArray().Length > 0)
                            {
                                if (Global.TTSSelectIndex > 1) strLab.Content = Global.strTTS.Substring(0, Global.TTSSelectIndex - 1);
                                else strLab.Content = "";
                                strLab.Content += "|";
                                if (Global.strTTS.Length > Global.TTSSelectIndex) strLab.Content += Global.strTTS.Substring(Global.TTSSelectIndex);
                                Global.TTSSelectIndex--;
                                Global.strTTS = strLab.Content.ToString().Split('|')[0] + strLab.Content.ToString().Split('|')[1];
                                textScrollView.ScrollToHorizontalOffset(Global.TTSSelectIndex * strLab.FontSize / 2);
                            }
                            break;
                        case KeyinStep.zone2Selecting:
                            foreach (FrameworkElement bt in btnZone2) ((System.Windows.Controls.Button)bt).Foreground = new SolidColorBrush(Colors.Gray);
                            if (btnKeyin.Count > 1)
                            {
                                btnKeyin.RemoveAt(btnKeyin.Count - 1);
                                foreach (Button b in btnKeyin) b.Foreground = new SolidColorBrush(Colors.Gold);
                                if (Global.strLanguage == "zh-TW") Keyin_zhTW();
                                if (Global.strLanguage == "zh-CN") Keyin_zhCN();
                                if (Global.strLanguage == "zh-HK") Keyin_zhHK();
                            }
                            else
                            {
                                ClearZone1();
                                keyinStep = KeyinStep.Null;
                            }
                            break;
                        case KeyinStep.zone2Selected:
                            keyinStep = KeyinStep.zone2Selecting;
                            if (Global.strLanguage == "zh-TW") Keyin_zhTW();
                            if (Global.strLanguage == "zh-CN") Keyin_zhCN();
                            if (Global.strLanguage == "zh-HK") Keyin_zhHK();
                            break;
                        case KeyinStep.zone1Selected:
                            keyinStep = KeyinStep.zone2Selecting;
                            if (Global.strLanguage == "zh-TW") Keyin_zhTW();
                            if (Global.strLanguage == "zh-CN") Keyin_zhCN();
                            if (Global.strLanguage == "zh-HK") Keyin_zhHK();
                            break;
                        case KeyinStep.wordSelected:
                            ClearZone1();
                            break;
                    }
                    break;
                
            }
        }
        private string zone1FinalWord = "";
        private string zone2FinalWord = ""; 
        private char[] zone2ChinsesWords;   
        private List<string> zone2Vocabularys;  
        private int zone2Page = 0;
        private void btnZone1MouseClick(object sender, RoutedEventArgs e)
        {
            string strA = "";
            if (((Button)(System.Windows.FrameworkElement)e.Source).Style == (Style)TryFindResource("styleTextRight"))
            {
                switch (keyinStep)
                {
                    case KeyinStep.zone2Selecting:
                        KeyinChangePage(findWords, keyinPage + 1);
                        break;
                    case KeyinStep.zone1Selected: 
                        zone2Page++;
                        showZone2Word(zone2ChinsesWords, zone2Page);
                        break;
                    case KeyinStep.wordSelected:
                        zone2Page++;
                        showZone2Word(zone2Vocabularys, zone2Page);
                        break;
                }
                return;
            }
            else if (((Button)(System.Windows.FrameworkElement)e.Source).Style == (Style)TryFindResource("styleTextLeft"))
            {
                switch (keyinStep)
                {
                    case KeyinStep.zone2Selecting:
                        KeyinChangePage(findWords, keyinPage - 1);
                        break;
                    case KeyinStep.zone1Selected: 
                        zone2Page--;
                        showZone2Word(zone2ChinsesWords, zone2Page);
                        break;
                    case KeyinStep.wordSelected:
                        zone2Page--;
                        showZone2Word(zone2Vocabularys, zone2Page);
                        break;
                }
                return;
            }
            else if (((Button)(System.Windows.FrameworkElement)e.Source).Content != null)
            {
                if(((Button)(System.Windows.FrameworkElement)e.Source).Content.ToString() == "") return;
                if (state == State.en_US) keyinStep = KeyinStep.zone1Selected;
                switch (keyinStep)
                {
                    case KeyinStep.zone2Selecting:
                        if (state == State.zh_TW)
                        {
                            foreach (string s in rowWordIndex)
                            {
                                if (s.Split(',')[0] == ((Button)(System.Windows.FrameworkElement)e.Source).Content.ToString())
                                {
                                    zone2FinalWord = s.Split(',')[0];
                                    List<string> symbols = new List<string>();
                                    for (int i = 1; i < s.Split(',').Length; i++)
                                        if (s.Split(',')[i] != "　")
                                            symbols.Add(s.Split(',')[i]);
                                    showZone2Word(symbols, 0);
                                    for (int i = 0; i < btnZone1.Count; i++)
                                    {
                                        btnZone1[i].Style = (Style)TryFindResource("styleBTN_Lit");
                                        ((Button)btnZone1[i]).Content = "";
                                        if (i < symbols.Count) ((Button)btnZone1[i]).Content = symbols[i];
                                    }
                                    keyinStep = KeyinStep.zone2Selected;
                                }
                            }
                        }
                        if (state == State.zh_CN)
                        {
                            foreach (string s in rowWordIndex)
                            {
                                if (s.Split(',')[0] == ((Button)(System.Windows.FrameworkElement)e.Source).Content.ToString())
                                {
                                    zone2FinalWord = s.Split(',')[0];
                                    List<string> symbols = new List<string>();
                                    for (int i = 1; i < s.Split(',').Length; i++)
                                        if (s.Split(',')[i] != "　")
                                            symbols.Add(s.Split(',')[i]);
                                    showZone2Word(symbols, 0);
                                    for (int i = 0; i < btnZone1.Count; i++)
                                    {
                                        btnZone1[i].Style = (Style)TryFindResource("styleBTN_Lit");
                                        ((Button)btnZone1[i]).Content = "";
                                        if (i < symbols.Count) ((Button)btnZone1[i]).Content = symbols[i];
                                    }
                                    keyinStep = KeyinStep.zone2Selected;
                                }
                            }
                        }
                        if (state == State.zh_HK)
                        {
                            foreach (string s in rowWordIndex)
                            {
                                if (s.Split(',')[0] == ((Button)(System.Windows.FrameworkElement)e.Source).Content.ToString())
                                {
                                    zone2FinalWord = s.Split(',')[0];
                                    List<string> symbols = new List<string>();
                                    for (int i = 1; i < s.Split(',').Length; i++)
                                        if (s.Split(',')[i] != "　")
                                            symbols.Add(s.Split(',')[i]);
                                    showZone2Word(symbols, 0);
                                    for (int i = 0; i < btnZone1.Count; i++)
                                    {
                                        btnZone1[i].Style = (Style)TryFindResource("styleBTN_Lit");
                                        ((Button)btnZone1[i]).Content = "";
                                        if (i < symbols.Count) ((Button)btnZone1[i]).Content = symbols[i];
                                    }
                                    keyinStep = KeyinStep.zone2Selected;
                                }
                            }
                        }
                        break;

                    case KeyinStep.zone2Selected:
                        if (state == State.zh_TW)
                        {
                            if (((Button)(System.Windows.FrameworkElement)e.Source).Content.ToString() != "＿")
                                zone2FinalWord += ((Button)(System.Windows.FrameworkElement)e.Source).Content.ToString();
                            foreach (string s in rowWords)
                            {
                                if (s.Split(',')[0] == zone2FinalWord)
                                {
                                    zone2ChinsesWords = s.Split(',')[1].ToCharArray();
                                    ((Button)btnZone1[0]).Content = s.Split(',')[1];
                                    showZone2Word(zone2ChinsesWords, 0);
                                }
                            }
                            keyinStep = KeyinStep.zone1Selected;
                            foreach (FrameworkElement bt in btnZone2) ((System.Windows.Controls.Button)bt).Foreground = new SolidColorBrush(Colors.Gray);
                        }
                        if (state == State.zh_CN)
                        {
                            if (((Button)(System.Windows.FrameworkElement)e.Source).Content.ToString() != "＿")
                                zone2FinalWord += ((Button)(System.Windows.FrameworkElement)e.Source).Content.ToString();
                            foreach (string s in rowWords)
                            {
                                if (s.Split(',')[0] == zone2FinalWord)
                                {
                                    zone2ChinsesWords = s.Split(',')[1].ToCharArray();
                                    ((Button)btnZone1[0]).Content = s.Split(',')[1];
                                    showZone2Word(zone2ChinsesWords, 0);
                                }
                            }
                            keyinStep = KeyinStep.zone1Selected;
                        }
                        if (state == State.zh_HK)
                        {
                            if (((Button)(System.Windows.FrameworkElement)e.Source).Content.ToString() != "＿")
                                zone2FinalWord += ((Button)(System.Windows.FrameworkElement)e.Source).Content.ToString();
                            foreach (string s in rowWords)
                            {
                                if (s.Split(',')[0] == zone2FinalWord)
                                {
                                    zone2ChinsesWords = s.Split(',')[1].ToCharArray();
                                    ((Button)btnZone1[0]).Content = s.Split(',')[1];
                                    showZone2Word(zone2ChinsesWords, 0);
                                }
                            }
                            keyinStep = KeyinStep.zone1Selected;
                        }
                        break;
                    case KeyinStep.zone1Selected:
                        Global.strTTS = strLab.Content.ToString().Split('|')[0];
                        Global.strTTS += ((Button)(System.Windows.FrameworkElement)e.Source).Content.ToString();
                        strA = strLab.Content.ToString().Split('|')[1];
                        strLab.Content = Global.strTTS + "|";
                        strLab.Content += strA;
                        Global.strTTS += strLab.Content.ToString().Split('|')[1];
                        Global.TTSSelectIndex++;
                        textScrollView.ScrollToHorizontalOffset(Global.TTSSelectIndex * strLab.FontSize / 2);
                        if (state != State.zh_TW && state!=State.zh_HK)
                        {
                            ClearZone1();
                            return;
                        }

                        foreach (string s in rowVocabulary)
                        {
                            if (s.Split(',')[0] == ((Button)(System.Windows.FrameworkElement)e.Source).Content.ToString())
                            {
                                zone2Vocabularys = new List<string>();
                                for (int i = 1; i < s.Split(',').Length; i++)
                                {
                                    zone2Vocabularys.Add(s.Split(',')[i]);
                                }
                                showZone2Word(zone2Vocabularys, 0);
                                keyinStep = KeyinStep.wordSelected;
                                zone2Page = 0;
                                return;
                            }
                        }
                        
                        ClearZone1();
                        break;
                    case KeyinStep.wordSelected:
                        Global.strTTS = strLab.Content.ToString().Split('|')[0];
                        Global.strTTS += ((Button)(System.Windows.FrameworkElement)e.Source).Content.ToString();
                        strA = strLab.Content.ToString().Split('|')[1];
                        strLab.Content = Global.strTTS + "|";
                        strLab.Content += strA;
                        Global.strTTS += strLab.Content.ToString().Split('|')[1];
                        Global.TTSSelectIndex++;
                        textScrollView.ScrollToHorizontalOffset(Global.TTSSelectIndex * strLab.FontSize / 2);
                        foreach (string s in rowVocabulary)
                        {
                            if (s.Split(',')[0] == ((Button)(System.Windows.FrameworkElement)e.Source).Content.ToString())
                            {
                                zone2Vocabularys = new List<string>();
                                for (int i = 1; i < s.Split(',').Length; i++)
                                {
                                    zone2Vocabularys.Add(s.Split(',')[i]);
                                }
                                showZone2Word(zone2Vocabularys, 0);
                                zone2Page = 0;
                                return;
                            }
                        }
                        break;
                }
            }
            
        }
        
        private void btnZone2MouseClick(object sender, RoutedEventArgs e)
        {
            string strA = "";
            string name = ((Button)((System.Windows.FrameworkElement)e.Source)).Name;
            foreach (FrameworkElement ct in btnZone2) ((System.Windows.Controls.Button)ct).Foreground = new SolidColorBrush(Colors.Gray);
            switch (name)
            {
                case "btnNumber":
                    if (keyinStep != KeyinStep.Null)      ClearZone1();
                    state = State.num;
                    break;
                case "btnEnglish":
                    if (keyinStep != KeyinStep.Null)      ClearZone1();
                    switch (Global.strLanguage)
                    {
                        case "zh-TW":
                            if (state != State.zh_TW)  state = State.zh_TW;
                            else state = State.en_US;
                            ClearZone1();
                            break;
                        case "zh-CN":
                            if (state != State.zh_CN) state = State.zh_CN;
                            else state = State.en_US;
                            ClearZone1();
                            break;
                        case "zh-HK":
                            if (state != State.zh_HK) state = State.zh_HK;
                            else state = State.en_US;
                            ClearZone1();
                            break;
                        case "en-US":
                            state = State.en_US;
                            ClearZone1();
                            break;
                    }
                    break;
                case "btnSearch":
                    if (Global.strTTS != "")
                    {
                        doSearch(Global.strTTS);
                        Global.clickState = Global.ClickState.scroll;
                        this.Close();
                        Global.textSpeak = null;
                        System.Windows.Forms.Cursor.Show();
                        if (Global.SRF == null)
                        {
                            Global.SRF = new StayResidentFunction();
                            Global.SRF.Show();
                        }
                        Global.cursorControl.cvPiePiece.Children.Clear();
                    }
                    break;
                case "btnCopy":
                    if (Global.strTTS.ToCharArray().Length > 0)
                    {
                        Clipboard.SetText(Global.strTTS);
                        Global.clickState = Global.ClickState.copy;
                        if (Global.copyPage == null)
                        {
                            Global.copyPage = new CopyPage();
                            Global.copyPage.Show();
                        }
                        if (Global.SRP == null)
                        {
                            Global.SRP = new StayResidentPaste();
                            Global.SRP.Show();
                        }
                        Global.cursorControl.cvPiePiece.Children.Clear();
                        if (Global.clickPage == null)
                        {
                            Global.clickPage = new Click();
                            Global.clickPage.Show();
                        }
                        this.Close();
                        Global.textSpeak = null;
                        System.Windows.Forms.Cursor.Show();
                        return;
                    }
                    break;
                case "btnSpace":
                    if (keyinStep == KeyinStep.Null)
                    {
                        Global.strTTS = strLab.Content.ToString().Split('|')[0]+" ";
                        strA = strLab.Content.ToString().Split('|')[1];
                        strLab.Content = Global.strTTS + "|";
                        strLab.Content += strA;
                        Global.strTTS += strLab.Content.ToString().Split('|')[1];
                        Global.TTSSelectIndex++;
                        textScrollView.ScrollToHorizontalOffset(Global.TTSSelectIndex * strLab.FontSize / 2);
                    }
                    else if (state == State.zh_TW && keyinStep == KeyinStep.zone2Selecting)
                    {
                        KeyinZhSpace(btnKeyin.Count);
                        foreach (Button b in btnKeyin) b.Foreground = new SolidColorBrush(Colors.Gold);
                    }
                    if (keyinStep == KeyinStep.zone2Selected && state == State.zh_TW)
                        foreach (Button b in btnKeyin) b.Foreground = new SolidColorBrush(Colors.Gold);
                    break;
                case "btnBackMenu":
                    Global.cursorControl.cvPiePiece.Children.Clear();
                    Global.backMenu();
                    break;
                case "btnSymbol_usual":
                    if (keyinStep != KeyinStep.Null) ClearZone1();
                    state = State.symbol_usual;
                    break;
                case "btnSpeak":
                    if (Global.strTTS != "" && !TTS.isSpeech)
                        TTS.Say(Global.strTTS);
                    break;
                case "btnBell":
                    if (System.IO.File.Exists(Environment.CurrentDirectory + @"\Sounds\help.mp3") && soundPlayer == null)
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
                case "btnClear":
                    ClearZone1();
                    Global.strTTS = "";
                    strLab.Content = "|";
                    Global.TTSSelectIndex = 0;
                    textScrollView.ScrollToRightEnd();
                    break;
                default :
                    switch (state)
                    {
                        case State.zh_TW:
                            if (keyinStep == KeyinStep.zone1Selected || keyinStep == KeyinStep.wordSelected)
                            {
                                ClearZone1();
                                btnKeyin.Add((Button)((System.Windows.FrameworkElement)e.Source));
                                
                            }
                            else if (keyinStep == KeyinStep.zone2Selected)
                            {
                                foreach (Button b in btnKeyin) b.Foreground = new SolidColorBrush(Colors.Gold);
                                return;
                            }
                            btnKeyin.Add((Button)((System.Windows.FrameworkElement)e.Source));
                            ZhTWKeyInCheck();
                            Keyin_zhTW();
                            break;
                        case State.zh_CN:
                            if (keyinStep == KeyinStep.zone1Selected || keyinStep == KeyinStep.wordSelected)
                            {
                                ClearZone1();
                                btnKeyin.Add((Button)((System.Windows.FrameworkElement)e.Source));
                                Keyin_zhHK();
                                return;
                            }
                            else if (keyinStep == KeyinStep.zone2Selected) return;
                            btnKeyin.Add((Button)((System.Windows.FrameworkElement)e.Source));
                            Keyin_zhCN();
                            break;
                        case State.zh_HK:
                            if (keyinStep == KeyinStep.zone1Selected || keyinStep == KeyinStep.wordSelected)
                            {
                                ClearZone1();
                                btnKeyin.Add((Button)((System.Windows.FrameworkElement)e.Source));
                                Keyin_zhHK();
                                return;
                            }
                            else if (keyinStep == KeyinStep.zone2Selected) return;
                           
                            btnKeyin.Add((Button)((System.Windows.FrameworkElement)e.Source));
                            Keyin_zhHK();
                            break;
                        case State.en_US:
                            btnKeyin.Add((Button)((System.Windows.FrameworkElement)e.Source));
                            Keyin_enUS();
                            break;
                        case State.num:
                            btnKeyin.Add((Button)((System.Windows.FrameworkElement)e.Source));
                            Keyin_num();
                            break;
                    }
                    return;
            }
            changeBtnContent();
        }

        private void ZhTWKeyInCheck()
        {
            int lv = -1;
            List<Button> lv_0 = new List<Button>();
            List<Button> lv_1 = new List<Button>();
            List<Button> lv_2 = new List<Button>();
            lv_0.Add(btn0); lv_0.Add(btn1); lv_0.Add(btn4); lv_0.Add(btn5); lv_0.Add(btn7); lv_0.Add(btn8);
            lv_1.Add(btn2);
            lv_2.Add(btn3); lv_2.Add(btn6); lv_2.Add(btn9); lv_2.Add(btn10);
            if (btnKeyin.Count > 1)
            {
                foreach (Button b0 in lv_0) if (btnKeyin[btnKeyin.Count - 1] == b0) lv = 0;
                if (btnKeyin[btnKeyin.Count - 1] == lv_1[0]) lv = 1;
                foreach (Button b2 in lv_2) if (btnKeyin[btnKeyin.Count - 1] == b2) lv = 2;
                for (int i = 0; i < btnKeyin.Count - 1; i++)
                {
                    switch (lv)
                    {
                    case 0:
                        foreach (Button b0 in lv_0)
                        {
                            if (btnKeyin[i] == b0)
                            {
                                btnKeyin[i] = btnKeyin[btnKeyin.Count - 1];
                                btnKeyin.RemoveAt(btnKeyin.Count - 1);
                                return;
                            }
                        }
                        Button tempBtn = btnKeyin[i];
                        btnKeyin[i] = btnKeyin[btnKeyin.Count - 1];
                        btnKeyin[btnKeyin.Count - 1] = tempBtn;
                        break;
                    case 1:
                        foreach (Button b0 in lv_2)
                        {
                            if (btnKeyin[i] == b0)
                            {
                                Button temp = btnKeyin[i];
                                btnKeyin[i] = btnKeyin[btnKeyin.Count - 1];
                                btnKeyin[btnKeyin.Count - 1] = temp;
                                return;
                            }
                        }
                        break;
                    case 2:
                        if (btnKeyin[btnKeyin.Count - 1] == btn10 && btnKeyin.Count > 2)
                        {
                            btnKeyin.RemoveAt(btnKeyin.Count - 1);
                            return;
                        }
                        foreach (Button b0 in lv_2)
                        {
                            if (btnKeyin[i] == b0)
                            {
                                btnKeyin[i] = btnKeyin[btnKeyin.Count - 1];
                                btnKeyin.RemoveAt(btnKeyin.Count - 1);
                                return;
                            }
                        }
                        break;
                    }
                }
            }
        }

        MediaPlayer soundPlayer;
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
        private void doSearch(string keyword)
        {
            if (keyword.ToCharArray().Length > 4)
            {
                if (keyword.Substring(0, 4) == "http" || keyword.Substring(0, 3) == "www.")
                {
                    System.Diagnostics.Process.Start(keyword);
                    return;
                }
            }
            System.Diagnostics.Process.Start("http://www.google.com.tw/search?hl=en&q=" + keyword + "&meta=");
        }
        public void changeBtnContent()
        {
            switch (state)
            {
                case State.zh_TW:
                    btn0.Content = "ㄅㄆㄇㄈ";
                    btn1.Content = "ㄐ ㄑ ㄒ";
                    btn2.Content = "ㄧ ㄨ ㄩ";
                    btn3.Content = "ㄢㄣㄤㄥ";
                    btn4.Content = "ㄉㄊㄋㄌ";
                    btn5.Content = "ㄓㄔㄕㄖ";
                    btn6.Content = "ㄚㄛㄜㄝ";
                    btn7.Content = "ㄍ ㄎ ㄏ";
                    btn8.Content = "ㄗ ㄘ ㄙ";
                    btn9.Content = "ㄞㄟㄠㄡ";
                    btn10.Content = "ㄦ";
                    
                    break;
                case State.en_US:
                    btn0.Content = "A B C";
                    btn1.Content = "J K L";
                    btn2.Content = "S T U";
                    btn3.Content = "/";
                    btn4.Content = "D E F";
                    btn5.Content = "M N O";
                    btn6.Content = "V W X";
                    btn7.Content = "G H I";
                    btn8.Content = "P Q R";
                    btn9.Content = "Y Z";
                    btn10.Content = ".";
                    break;
                case State.num:
                    btn0.Content = "1";
                    btn1.Content = "4";
                    btn2.Content = "7";
                    btn3.Content = "0";
                    btn4.Content = "2";
                    btn5.Content = "5";
                    btn6.Content = "8";
                    btn7.Content = "3";
                    btn8.Content = "6";
                    btn9.Content = "9";
                    btn10.Content = "";
                    break;
                case State.symbol_usual:
                    Global.tsSymbol = new TextSpeak_symbol();
                    Global.tsSymbol.state = TextSpeak_symbol.State.symbol_usual;
                    Global.tsSymbol.Show();
                    this.Close();
                    Global.textSpeak = null;
                    break;
                case State.zh_CN:
                    btn0.Content = "a b c";
                    btn1.Content = "j k l";
                    btn2.Content = "s t u";
                    btn3.Content = "";
                    btn4.Content = "d e f";
                    btn5.Content = "m n o";
                    btn6.Content = "v w x";
                    btn7.Content = "g h i";
                    btn8.Content = "p q r";
                    btn9.Content = "y z";
                    btn10.Content = "";
                    break;
                case State.zh_HK:
                    btn0.Content = "a b c";
                    btn1.Content = "j k l";
                    btn2.Content = "s t u";
                    btn3.Content = "";
                    btn4.Content = "d e f";
                    btn5.Content = "m n o";
                    btn6.Content = "v w x";
                    btn7.Content = "g h i";
                    btn8.Content = "p q r";
                    btn9.Content = "y z";
                    btn10.Content = "";
                    break;
                
            }
        }

        Assembly _assembly;
        List<string> rowWordIndex;
        List<string> rowWords;
        List<string> rowVocabulary;
        private void LoadWord_zhTW()
        {
            _assembly = Assembly.GetExecutingAssembly();
            System.IO.StreamReader sr = new StreamReader(_assembly.GetManifestResourceStream("Eyeplayer.config.zhTW-index.csv"), Encoding.Default);
            rowWordIndex = new List<string>();
            while (sr.Peek() != -1)   rowWordIndex.Add((string)sr.ReadLine());
            sr.Close();
            System.IO.StreamReader srWords = new StreamReader(_assembly.GetManifestResourceStream("Eyeplayer.config.zhTW.csv"), Encoding.Default);
            rowWords = new List<string>();
            while (srWords.Peek() != -1)   rowWords.Add(srWords.ReadLine());
            srWords.Close();
            System.IO.StreamReader srVb = new StreamReader(_assembly.GetManifestResourceStream("Eyeplayer.config.zhTW-vocabulary.csv"), Encoding.Default);
            rowVocabulary = new List<string>();
            while (srVb.Peek() != -1) rowVocabulary.Add(srVb.ReadLine());
            srVb.Close();
        }
        private void LoadWord_zhCN()
        {
            _assembly = Assembly.GetExecutingAssembly();
            System.IO.StreamReader sr = new StreamReader(_assembly.GetManifestResourceStream("Eyeplayer.config.zhCN.csv"), Encoding.Default);
            rowWordIndex = new List<string>();
            List<string> listString = new List<string>();
            while (sr.Peek() != -1) listString.Add(sr.ReadLine());
            sr.Close();
            for (int i = 0; i < listString.Count; i++)
            {
                bool isExist = false;
                for (int j = 0; j < rowWordIndex.Count; j++)
                {
                    if (rowWordIndex[j].Split(',')[0] == listString[i].Split('\t')[0].Split(' ')[0])
                    {
                        rowWordIndex[j] = rowWordIndex[j] + "," + listString[i].Split('\t')[0].Split(' ')[1];
                        isExist = true;
                    }
                }
                if (!isExist)
                {
                    rowWordIndex.Add(listString[i].Split('\t')[0].Split(' ')[0]);
                    if (rowWordIndex.Count>0)
                        rowWordIndex[rowWordIndex.Count - 1] = rowWordIndex[rowWordIndex.Count - 1] + "," + listString[i].Split('\t')[0].Split(' ')[1];
                    else
                        rowWordIndex[0] = rowWordIndex[0] + "," + listString[i].Split('\t')[0].Split(' ')[1];
                }
            }

            System.IO.StreamReader srWords = new StreamReader(_assembly.GetManifestResourceStream("Eyeplayer.config.zhCN.csv"), Encoding.Default);
            rowWords = new List<string>();
            while (srWords.Peek() != -1) rowWords.Add(srWords.ReadLine());
            srWords.Close();
            for (int i = 0; i < rowWords.Count;i++ )
            {
                rowWords[i] = rowWords[i].Split(' ')[0] + rowWords[i].Split(' ')[1].Split('\t')[0] + "," + rowWords[i].Split(' ')[1].Split('\t')[1];
            }
            
        }
        private void LoadWord_zhHK()
        {
            _assembly = Assembly.GetExecutingAssembly();
            System.IO.StreamReader sr = new StreamReader(_assembly.GetManifestResourceStream("Eyeplayer.config.zhHK.csv"), Encoding.Default);
            rowWordIndex = new List<string>();
            List<string> listString = new List<string>();
            while (sr.Peek() != -1) listString.Add(sr.ReadLine());
            sr.Close();
            for (int i = 0; i < listString.Count; i++)
            {
                bool isExist = false;
                for (int j = 0; j < rowWordIndex.Count; j++)
                {
                    if (rowWordIndex[j].Split(',')[0] == listString[i].Split('\t')[0].Split(' ')[0])
                    {
                        rowWordIndex[j] = rowWordIndex[j] + "," + listString[i].Split('\t')[0].Split(' ')[1];
                        isExist = true;
                    }
                }
                if (!isExist)
                {
                    rowWordIndex.Add(listString[i].Split('\t')[0].Split(' ')[0]);
                    if (rowWordIndex.Count>0)
                        rowWordIndex[rowWordIndex.Count - 1] = rowWordIndex[rowWordIndex.Count - 1] + "," + listString[i].Split('\t')[0].Split(' ')[1];
                    else
                        rowWordIndex[0] = rowWordIndex[0] + "," + listString[i].Split('\t')[0].Split(' ')[1];
                }
            }
            System.IO.StreamReader srWords = new StreamReader(_assembly.GetManifestResourceStream("Eyeplayer.config.zhHK.csv"), Encoding.Default);
            rowWords = new List<string>();
            while (srWords.Peek() != -1) rowWords.Add(srWords.ReadLine());
            srWords.Close();
            for (int i = 0; i < rowWords.Count;i++ )
            {
                rowWords[i] = rowWords[i].Split(' ')[0] + rowWords[i].Split(' ')[1].Split('\t')[0] + "," + rowWords[i].Split(' ')[1].Split('\t')[1];
            }
            System.IO.StreamReader srVb = new StreamReader(_assembly.GetManifestResourceStream("Eyeplayer.config.zhTW-vocabulary.csv"), Encoding.Default);
            rowVocabulary = new List<string>();
            while (srVb.Peek() != -1) rowVocabulary.Add(srVb.ReadLine());
            srVb.Close();
            
        }
        private void showZone2Word(List<string> words, int page)
        {
            if (page >= 1)
            {
                btnZone1[0].Style = (Style)TryFindResource("styleTextLeft");
                ((Button)btnZone1[0]).Content = "";
                if (words.Count > page * 7 + 8)
                {
                    btnZone1[btnZone1.Count - 1].Style = (Style)TryFindResource("styleTextRight");
                    ((Button)btnZone1[btnZone1.Count - 1]).Content = "";
                }
                else
                {
                    btnZone1[btnZone1.Count - 1].Style = (Style)TryFindResource("styleBTN_Lit");
                }
                
                for (int i = 1; i < btnZone1.Count-1; i++)
                {
                    btnZone1[i].Style = (Style)TryFindResource("styleBTN_Lit");
                    if ((page - 1) * 7 + 7 + i < words.Count)
                    {
                        ((Button)btnZone1[i]).Content = words[(page - 1) * 7 + 7 + i];
                    }
                    else
                    {
                        ((Button)btnZone1[i]).Content = "";
                    }
                }
            }
            else
            {
                if (words.Count > btnZone1.Count)
                {
                   
                    for (int i = 0; i < btnZone1.Count - 1; i++)
                    {
                        btnZone1[i].Style = (Style)TryFindResource("styleBTN_Lit");
                        if (i < words.Count)    ((Button)btnZone1[i]).Content = words[i];
                        
                    }
                    btnZone1[btnZone1.Count - 1].Style = (Style)TryFindResource("styleTextRight");
                    ((Button)btnZone1[btnZone1.Count - 1]).Content = "";
                }
                else
                {
                    for (int i = 0; i < btnZone1.Count; i++)
                    {
                        btnZone1[i].Style = (Style)TryFindResource("styleBTN_Lit");
                        if (i < words.Count)
                        {
                            ((Button)btnZone1[i]).Content = words[i];
                        }
                        else
                        {
                            ((Button)btnZone1[i]).Content = "";
                        }
                    }
                }
            }

        }
        private void showZone2Word(char[] words, int page)
        {
            if (page >= 1)
            {
                btnZone1[0].Style = (Style)TryFindResource("styleTextLeft");
                ((Button)btnZone1[0]).Content = "";
                if (words.Length > page * 7 + 8)
                {
                    btnZone1[btnZone1.Count - 1].Style = (Style)TryFindResource("styleTextRight");
                    ((Button)btnZone1[btnZone1.Count - 1]).Content = "";
                }
                else
                {
                    btnZone1[btnZone1.Count - 1].Style = (Style)TryFindResource("styleBTN_Lit");
                }
                for (int i = 1; i < btnZone1.Count - 1; i++)
                {
                    btnZone1[i].Style = (Style)TryFindResource("styleBTN_Lit");
                    if ((page - 1) * 7 + 7 + i < words.Length)
                    {
                        ((Button)btnZone1[i]).Content = words[(page - 1) * 7 + 7 + i];
                    }
                    else
                    {
                        ((Button)btnZone1[i]).Content = "";
                    }
                }
            }
            else
            {
                if (words.Length > btnZone1.Count)
                {
                    for (int i = 0; i < btnZone1.Count - 1; i++)
                    {
                        btnZone1[i].Style = (Style)TryFindResource("styleBTN_Lit");
                        if (i < words.Length)
                        {
                            ((Button)btnZone1[i]).Content = words[i];
                        }
                    }
                    btnZone1[btnZone1.Count - 1].Style = (Style)TryFindResource("styleTextRight");
                    ((Button)btnZone1[btnZone1.Count - 1]).Content = "";
                }
                else
                {
                    for (int i = 0; i < btnZone1.Count; i++)
                    {
                        btnZone1[i].Style = (Style)TryFindResource("styleBTN_Lit");
                        if (i < words.Length)
                        {
                            ((Button)btnZone1[i]).Content = words[i];
                        }
                        else
                        {
                            ((Button)btnZone1[i]).Content = "";
                        }
                    }
                }
            }
        }
        List<Char> arrayChat_0, arrayChat_1, arrayChat_2, arrayChat_3;
        List<string> zone2Words;
        List<string> findWords;
        private void Keyin_zhTW()
        {
            findWords = new List<string>();
            if (keyinStep == KeyinStep.Null) keyinStep = KeyinStep.zone2Selecting;

           
            if (btnKeyin.Count > 0)
            {
                arrayChat_0 = new List<char>();
                for (int i = 0; i < btnKeyin[0].Content.ToString().ToCharArray().Length; i++)
                {
                    if (btnKeyin[0].Content.ToString().ToCharArray()[i].ToString() != " ") arrayChat_0.Add(btnKeyin[0].Content.ToString().ToCharArray()[i]);
                }
            }
            if (btnKeyin.Count > 1)
            {
                arrayChat_1 = new List<char>();
                for (int i = 0; i < btnKeyin[1].Content.ToString().ToCharArray().Length; i++)
                {
                    if (btnKeyin[1].Content.ToString().ToCharArray()[i].ToString() != " ") arrayChat_1.Add(btnKeyin[1].Content.ToString().ToCharArray()[i]);
                }
            }
            if (btnKeyin.Count > 2)
            {
                arrayChat_2 = new List<char>();
                for (int i = 0; i < btnKeyin[2].Content.ToString().ToCharArray().Length; i++)
                {
                    if (btnKeyin[2].Content.ToString().ToCharArray()[i].ToString() != " ") arrayChat_2.Add(btnKeyin[2].Content.ToString().ToCharArray()[i]);
                }
            }
            foreach (string s in rowWordIndex)
            {
                foreach (char w1 in arrayChat_0)
                {
                    if (s.Split(',')[0].Substring(0, 1) == w1.ToString())
                    {
                        if (btnKeyin.Count == 1)
                        {
                            findWords.Add(s.Split(',')[0]);
                            break;
                        }
                        foreach (char w2 in arrayChat_1)
                        {
                            if (s.Split(',')[0].Length>1 && s.Split(',')[0].Substring(1, 1) == w2.ToString())
                            {
                                if (btnKeyin.Count == 2)
                                {
                                    findWords.Add(s.Split(',')[0]);
                                    break;
                                }
                                foreach (char w3 in arrayChat_2)
                                {
                                    if (s.Split(',')[0].Length > 2 && s.Split(',')[0].Substring(2, 1) == w3.ToString())
                                    {
                                        if (btnKeyin.Count == 3) findWords.Add(s.Split(',')[0]);
                                    }
                                }
                        
                            }
                        }
                    }
                }
                
            }
            if (findWords.Count > 0)
            {
                for (int i = 0; i < btnZone1.Count; i++) btnZone1[i].Style = (Style)TryFindResource("styleBTN_Lit");
                zone2Words = new List<string>();
                zone2Words = findWords;
                if (findWords.Count >= btnZone1.Count)
                {
                    btnZone1[btnZone1.Count - 1].Style = (Style)TryFindResource("styleTextRight");
                    ((Button)btnZone1[btnZone1.Count - 1]).Content = "";
                    for (int i = 0; i < btnZone1.Count-1; i++)
                    {
                        if (i < findWords.Count)
                        {
                            ((Button)btnZone1[i]).Content = findWords[i];
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < btnZone1.Count; i++)
                    {
                        btnZone1[i].Style = (Style)TryFindResource("styleBTN_Lit");
                        if (i < findWords.Count)
                        {
                            ((Button)btnZone1[i]).Content = findWords[i];
                        }
                        else
                        {
                            ((Button)btnZone1[i]).Content = "";
                        }
                    }
                }
            }
            else
            {
                btnKeyin.RemoveAt(btnKeyin.Count - 1);
                if(btnKeyin.Count>0) Keyin_zhTW();
            }
            foreach (Button b in btnKeyin) b.Foreground = new SolidColorBrush(Colors.Gold);
            keyinPage = 0;
        }
        private int keyinPage = 0;
        private void KeyinChangePage(List<string> str,int page)
        {
            keyinPage = page;
            if (page >= 1)
            {
                if (findWords.Count-8-page*7 >= 1)
                {
                    btnZone1[btnZone1.Count - 1].Style = (Style)TryFindResource("styleTextRight");
                    ((Button)btnZone1[btnZone1.Count - 1]).Content = "";
                    btnZone1[0].Style = (Style)TryFindResource("styleTextLeft");
                    ((Button)btnZone1[0]).Content = "";
                    for (int i = 1; i < btnZone1.Count - 1; i++)
                    {
                        if (page * 7 + i < findWords.Count) ((Button)btnZone1[i]).Content = findWords[page * 7 + i];
                    }
                }
                else
                {
                    btnZone1[0].Style = (Style)TryFindResource("styleTextLeft");
                    ((Button)btnZone1[0]).Content = "";
                    for (int i = 1; i < btnZone1.Count; i++)
                    {
                        btnZone1[i].Style = (Style)TryFindResource("styleBTN_Lit");
                        if (page * 7 + i < findWords.Count)
                            ((Button)btnZone1[i]).Content = findWords[page * 7 + i];
                        else
                            ((Button)btnZone1[i]).Content = "";
                    }
                }
            }
            else
            {
                if (findWords.Count >= btnZone1.Count)
                {
                    btnZone1[btnZone1.Count - 1].Style = (Style)TryFindResource("styleTextRight");
                    ((Button)btnZone1[btnZone1.Count - 1]).Content = "";
                    for (int i = 0; i < btnZone1.Count - 1; i++)
                    {
                        btnZone1[i].Style = (Style)TryFindResource("styleBTN_Lit");
                        if (i < findWords.Count)      ((Button)btnZone1[i]).Content = findWords[i];
                    }
                }
                else
                {
                    for (int i = 0; i < btnZone1.Count; i++)
                    {
                        btnZone1[i].Style = (Style)TryFindResource("styleBTN_Lit");
                        if (i < findWords.Count)
                            ((Button)btnZone1[i]).Content = findWords[i];
                        else
                            ((Button)btnZone1[i]).Content = "";
                    }
                }
            }
        }
        private void KeyinZhSpace(int Length)
        {
            bool isEmpty = true;
            List<string> str = new List<string>();
            foreach (string s in findWords)
            {
                if (s.ToCharArray().Length <= Length)
                {
                    str.Add(s);
                    isEmpty = false;
                }
            }
            if (isEmpty) return;
            findWords.Clear();
            findWords = str;
            KeyinChangePage(findWords, 0);
        }

        private void Keyin_zhCN()
        {
            findWords = new List<string>();
            if (keyinStep == KeyinStep.Null) keyinStep = KeyinStep.zone2Selecting;
            switch (btnKeyin.Count)
            {
                case 1:
                    arrayChat_0 = new List<char>();
                    for (int i = 0; i < btnKeyin[0].Content.ToString().ToCharArray().Length; i++)
                    {
                        if (btnKeyin[0].Content.ToString().ToCharArray()[i].ToString() != " ") arrayChat_0.Add(btnKeyin[0].Content.ToString().ToCharArray()[i]);
                    }
                    break;
                case 2:
                    arrayChat_1 = new List<char>();
                    for (int i = 0; i < btnKeyin[1].Content.ToString().ToCharArray().Length; i++)
                    {
                        if (btnKeyin[1].Content.ToString().ToCharArray()[i].ToString() != " ") arrayChat_1.Add(btnKeyin[1].Content.ToString().ToCharArray()[i]);
                    }
                    break;
                case 3:
                    arrayChat_2 = new List<char>();
                    for (int i = 0; i < btnKeyin[2].Content.ToString().ToCharArray().Length; i++)
                    {
                        if (btnKeyin[2].Content.ToString().ToCharArray()[i].ToString() != " ") arrayChat_2.Add(btnKeyin[2].Content.ToString().ToCharArray()[i]);
                    }
                    break;
                case 4:
                    arrayChat_3 = new List<char>();
                    for (int i = 0; i < btnKeyin[3].Content.ToString().ToCharArray().Length; i++)
                    {
                        if (btnKeyin[3].Content.ToString().ToCharArray()[i].ToString() != " ") arrayChat_3.Add(btnKeyin[3].Content.ToString().ToCharArray()[i]);
                    }
                    break;
            }
            foreach (string s in rowWordIndex)
            {
                foreach (char w1 in arrayChat_0)
                {
                    if (s.Split(',')[0].Substring(0, 1) == w1.ToString())
                    {
                        if (btnKeyin.Count == 1)
                        {
                            findWords.Add(s.Split(',')[0]);
                            break;
                        }
                        foreach (char w2 in arrayChat_1)
                        {
                            if (s.Split(',')[0].Length > 1 && s.Split(',')[0].Substring(1, 1) == w2.ToString())
                            {
                                if (btnKeyin.Count == 2)
                                {
                                    findWords.Add(s.Split(',')[0]);
                                    break;
                                }
                                foreach (char w3 in arrayChat_2)
                                {
                                    if (s.Split(',')[0].Length > 2 && s.Split(',')[0].Substring(2, 1) == w3.ToString())
                                    {
                                        if (btnKeyin.Count == 3) findWords.Add(s.Split(',')[0]);
                                    }
                                }

                            }
                        }
                    }
                }
            }
            if (findWords.Count > 0)
            {
                zone2Words = new List<string>();
                zone2Words = findWords;
                if (findWords.Count >= btnZone1.Count)
                {
                    btnZone1[btnZone1.Count - 1].Style = (Style)TryFindResource("styleTextRight");
                    ((Button)btnZone1[btnZone1.Count - 1]).Content = "";
                    for (int i = 0; i < btnZone1.Count - 1; i++)
                    {
                        if (i < findWords.Count)
                        {
                            ((Button)btnZone1[i]).Content = findWords[i];
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < btnZone1.Count; i++)
                    {
                        btnZone1[i].Style = (Style)TryFindResource("styleBTN_Lit");
                        if (i < findWords.Count)
                        {
                            ((Button)btnZone1[i]).Content = findWords[i];
                        }
                        else
                        {
                            ((Button)btnZone1[i]).Content = "";
                        }
                    }
                }
            }
            else
            {
                btnKeyin.RemoveAt(btnKeyin.Count - 1);
            }
        }
        private void Keyin_zhHK()
        {
            findWords = new List<string>();
            if (keyinStep == KeyinStep.Null) keyinStep = KeyinStep.zone2Selecting;
            switch (btnKeyin.Count)
            {
                case 1:
                    arrayChat_0 = new List<char>();
                    for (int i = 0; i < btnKeyin[0].Content.ToString().ToCharArray().Length; i++)
                    {
                        if (btnKeyin[0].Content.ToString().ToCharArray()[i].ToString() != " ") arrayChat_0.Add(btnKeyin[0].Content.ToString().ToCharArray()[i]);
                    }
                    break;
                case 2:
                    arrayChat_1 = new List<char>();
                    for (int i = 0; i < btnKeyin[1].Content.ToString().ToCharArray().Length; i++)
                    {
                        if (btnKeyin[1].Content.ToString().ToCharArray()[i].ToString() != " ") arrayChat_1.Add(btnKeyin[1].Content.ToString().ToCharArray()[i]);
                    }
                    break;
                case 3:
                    arrayChat_2 = new List<char>();
                    for (int i = 0; i < btnKeyin[2].Content.ToString().ToCharArray().Length; i++)
                    {
                        if (btnKeyin[2].Content.ToString().ToCharArray()[i].ToString() != " ") arrayChat_2.Add(btnKeyin[2].Content.ToString().ToCharArray()[i]);
                    }
                    break;
                case 4:
                    arrayChat_3 = new List<char>();
                    for (int i = 0; i < btnKeyin[3].Content.ToString().ToCharArray().Length; i++)
                    {
                        if (btnKeyin[3].Content.ToString().ToCharArray()[i].ToString() != " ") arrayChat_3.Add(btnKeyin[3].Content.ToString().ToCharArray()[i]);
                    }
                    break;
            }
            foreach (string s in rowWordIndex)
            {
                foreach (char w1 in arrayChat_0)
                {
                    if (s.Split(',')[0].Substring(0, 1) == w1.ToString())
                    {
                        if (btnKeyin.Count == 1)
                        {
                            findWords.Add(s.Split(',')[0]);
                            break;
                        }
                        foreach (char w2 in arrayChat_1)
                        {
                            if (s.Split(',')[0].Length > 1 && s.Split(',')[0].Substring(1, 1) == w2.ToString())
                            {
                                if (btnKeyin.Count == 2)
                                {
                                    findWords.Add(s.Split(',')[0]);
                                    break;
                                }
                                foreach (char w3 in arrayChat_2)
                                {
                                    if (s.Split(',')[0].Length > 2 && s.Split(',')[0].Substring(2, 1) == w3.ToString())
                                    {
                                        if (btnKeyin.Count == 3) findWords.Add(s.Split(',')[0]);
                                    }
                                }

                            }
                        }
                    }
                }
            }
            if (findWords.Count > 0)
            {
                zone2Words = new List<string>();
                zone2Words = findWords;
                if (findWords.Count >= btnZone1.Count)
                {
                    btnZone1[btnZone1.Count - 1].Style = (Style)TryFindResource("styleTextRight");
                    ((Button)btnZone1[btnZone1.Count - 1]).Content = "";
                    for (int i = 0; i < btnZone1.Count - 1; i++)
                    {
                        if (i < findWords.Count)
                        {
                            ((Button)btnZone1[i]).Content = findWords[i];
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < btnZone1.Count; i++)
                    {
                        btnZone1[i].Style = (Style)TryFindResource("styleBTN_Lit");
                        if (i < findWords.Count)
                        {
                            ((Button)btnZone1[i]).Content = findWords[i];
                        }
                        else
                        {
                            ((Button)btnZone1[i]).Content = "";
                        }
                    }
                }
            }
            else
            {
                btnKeyin.RemoveAt(btnKeyin.Count - 1);
            }
        }
        private void Keyin_enUS()
        {
            string strA = "";
            if (keyinStep == KeyinStep.Null && btnKeyin[0]!=null)
            {
                List<string> arrayStr = new List<string>();
                for (int i = 0; i < btnKeyin[0].Content.ToString().ToCharArray().Length; i++)
                {
                    if (btnKeyin[0].Content.ToString().ToCharArray()[i].ToString() != " ")
                    {
                        arrayStr.Add(btnKeyin[0].Content.ToString().ToCharArray()[i].ToString());
                        if (btnKeyin[0].Name != "btn3" && btnKeyin[0].Name != "btn10")
                        {
                            arrayStr.Add(btnKeyin[0].Content.ToString().ToCharArray()[i].ToString().ToLower());
                            showZone2Word(arrayStr, 0);
                        }
                        else
                        {
                            Global.strTTS = strLab.Content.ToString().Split('|')[0];
                            Global.strTTS += btnKeyin[0].Content.ToString().ToCharArray()[i].ToString();
                            strA = strLab.Content.ToString().Split('|')[1];
                            strLab.Content = Global.strTTS + "|";
                            strLab.Content += strA;
                            Global.strTTS += strLab.Content.ToString().Split('|')[1];
                            Global.TTSSelectIndex++;
                            textScrollView.ScrollToHorizontalOffset(Global.TTSSelectIndex * strLab.FontSize / 2);
                            
                        }
                    }
                }
                btnKeyin.Clear();
            }
        }
        private void Keyin_num()
        {
            string strA = "";
            if (keyinStep == KeyinStep.Null && btnKeyin[0] != null)
            {
                List<string> arrayStr = new List<string>();
                for (int i = 0; i < btnKeyin[0].Content.ToString().ToCharArray().Length; i++)
                {
                    if (btnKeyin[0].Content.ToString().ToCharArray()[i].ToString() != " ")
                    {
                        Global.strTTS = strLab.Content.ToString().Split('|')[0];
                        Global.strTTS += btnKeyin[0].Content.ToString().ToCharArray()[i].ToString();
                        strA = strLab.Content.ToString().Split('|')[1];
                        strLab.Content = Global.strTTS + "|";
                        strLab.Content += strA;
                        Global.strTTS += strLab.Content.ToString().Split('|')[1];
                        Global.TTSSelectIndex++;
                        textScrollView.ScrollToHorizontalOffset(Global.TTSSelectIndex * strLab.FontSize / 2);
                    }
                }
            }
            btnKeyin.Clear();
        }
        private void ClearZone1()
        {
            keyinStep = KeyinStep.Null; 
            zone2Page = 0;
            keyinPage = 0;
            if (btnKeyin != null) btnKeyin.Clear();
            if (arrayChat_0 != null) arrayChat_0.Clear();
            if (arrayChat_1 != null) arrayChat_1.Clear();
            if (arrayChat_2 != null) arrayChat_2.Clear();
            if (findWords != null) findWords.Clear();
            for (int i = 0; i < btnZone1.Count; i++)
            {
                btnZone1[i].Style = (Style)TryFindResource("styleBTN_Lit");
                ((Button)btnZone1[i]).Content = "";
            }
        }
    }
}
