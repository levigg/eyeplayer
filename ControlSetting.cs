using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;
using System.Xml.Linq;
using System.IO;

namespace Eyeplayer
{
    public class GlobalSetting
    {
        public static string settingXmlName = Environment.CurrentDirectory + @"\config\setting.xml";

        public static int defaultClickSpeed = 1000;
        public static bool defaultShowBtnBack = false;
        public static bool defaultPhyicalButton = false;
        public static int clickSpeedMax = 4200;
        public static int clickSpeedMin = 600;
        public static bool defaultCursorType = true;
        static int defaultUnlockSpeed = 3500;
        public static void InitializeXMLFile()
        {
            try
            {
                System.IO.FileInfo configfile = new System.IO.FileInfo(settingXmlName);
                configfile.Attributes = FileAttributes.Normal;
                if (configfile.Directory.Exists && System.IO.File.Exists(GlobalSetting.settingXmlName))
                {
                    LoadSetting();
                }
                else
                {
                    configfile.Directory.Create();
                    XDocument xml = new XDocument(
                        new XDeclaration("1.0", "utf-8", "yes"),
                        new XComment("System Setting"),
                        new XElement("SystemConfiguration",
                            new XComment("點選速度：" + clickSpeedMin.ToString() + "~" + clickSpeedMax.ToString()+"ms"),
                            new XElement("clickSpeed", new XAttribute("Value", defaultClickSpeed)),
                            new XComment("顯示返回按鈕"),
                            new XElement("showBackMenu", new XAttribute("Value", defaultShowBtnBack)),
                            new XComment("實體按鈕"),
                            new XElement("phyicalButtonMode", new XAttribute("Value", defaultPhyicalButton)),
                            new XComment("進入校正時間"),
                            new XElement("RecalibrateTime", new XAttribute("Value", defaultUnlockSpeed)),
                            new XComment("CursorType"),
                            new XElement("DefaultCursorType", new XAttribute("Value", defaultCursorType)),
                            new XComment("Close computer")

                            ));
                    xml.Save(settingXmlName);

                    Global.phyicalButtonMode = defaultPhyicalButton;
                    Global.clickSpeed = defaultClickSpeed;
                    Global.showBackMenu = defaultShowBtnBack;
                    Global.unlockSpeed = defaultUnlockSpeed;
                    Global.defaultCursorType = defaultCursorType;
                }
            }
            catch (Exception ex)
            { GlobalSetting.ExceptionOutput(ex); }
        }
        public static void ExceptionOutput(Exception ex)
        {
            Console.WriteLine("InitializeXMLFile Error: "+ ex.ToString());
        }

        public static void SaveValue()
        {
            XDocument xml = new XDocument(
                        new XDeclaration("1.0", "utf-8", "yes"),
                        new XComment("System Setting"),
                        new XElement("SystemConfiguration",
                            new XComment("點選速度：" + clickSpeedMin.ToString() + "~" + clickSpeedMax.ToString() + "ms"),
                            new XElement("clickSpeed", new XAttribute("Value", Global.clickSpeed)),
                            new XComment("顯示返回按鈕"),
                            new XElement("showBackMenu", new XAttribute("Value", Global.showBackMenu)),
                            new XComment("實體按鈕"),
                            new XElement("phyicalButtonMode", new XAttribute("Value", Global.phyicalButtonMode)),
                            new XComment("進入校正時間"),
                            new XElement("RecalibrateTime", new XAttribute("Value", Global.unlockSpeed)),
                            new XComment("CursorType"),
                            new XElement("CursorType", new XAttribute("Value", Global.defaultCursorType)),
                            new XComment("Close computer")
                            ));
            xml.Save(settingXmlName);
        }
        public static void LoadSetting()
        {
            if (System.IO.File.Exists(GlobalSetting.settingXmlName))
            {
                try
                {
                    XDocument xml = XDocument.Load(GlobalSetting.settingXmlName);
                    XElement root = xml.Root;
                    if (root.Element("clickSpeed") != null && root.Element("clickSpeed").Attribute("Value") != null)
                    {
                        Global.clickSpeed = Convert.ToInt16(root.Element("clickSpeed").Attribute("Value").Value);
                        if (Global.clickSpeed > clickSpeedMax) Global.clickSpeed = clickSpeedMax;
                        if (Global.clickSpeed < clickSpeedMin) Global.clickSpeed = clickSpeedMin;
                    }
                    else
                    {
                        Global.clickSpeed = Convert.ToInt32(GlobalSetting.defaultClickSpeed);
                    }
                    if (root.Element("showBackMenu") != null && root.Element("showBackMenu").Attribute("Value") != null)
                    {
                        Global.showBackMenu = Convert.ToBoolean(root.Element("showBackMenu").Attribute("Value").Value);
                    }
                    else
                    {
                        Global.showBackMenu = GlobalSetting.defaultShowBtnBack;
                    }
                    if (root.Element("phyicalButtonMode") != null && root.Element("phyicalButtonMode").Attribute("Value") != null)
                    {
                        Global.phyicalButtonMode = Convert.ToBoolean(root.Element("phyicalButtonMode").Attribute("Value").Value);
                    }
                    else
                    {
                        Global.phyicalButtonMode = GlobalSetting.defaultPhyicalButton;
                    }
                    if (root.Element("CursorType") != null && root.Element("CursorType").Attribute("Value") != null)
                    {
                        Global.defaultCursorType = Convert.ToBoolean(root.Element("CursorType").Attribute("Value").Value);
                    }
                    else
                    {
                        Global.defaultCursorType = GlobalSetting.defaultCursorType;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Loading setting error: "+ ex.ToString());
                    SaveValue();
                }
            }
            else
            {
                GlobalSetting.InitializeXMLFile();
            }
        }
    }
}
