using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tobii.Gaze.Core;
using System.Management;

namespace Eyeplayer
{
    class CheckSerialNumber
    {
        Uri uri;
        EyeTracker tracker;
        public List<string> serialNumbers;
        public List<string> RexSNs;
        public List<string> IS4SNs;
        private string deviceSN;
        
        public bool CheckDefaultSN()
        {
            //IS4
            IS4SNs = new List<string>();
            IS4SNs.Add("IS404-100106431951");

            //REX
            RexSNs = new List<string>();
            RexSNs.Add("tet-usb://rexdl-030104737754/");
            RexSNs.Add("tet-usb://rexdl-030104738724/");
            RexSNs.Add("tet-usb://rexdl-030104737784/");
            RexSNs.Add("tet-usb://rexdl-030104736794/");

            serialNumbers = new List<string>();
            //EYEXC
            serialNumbers.Add("EYEXC-030114622213");
            //EYEX2
            serialNumbers.Add("EYEX2-030135823232");
            serialNumbers.Add("EYEX2-030145125552");
            serialNumbers.Add("EYEX2-030145125542");
            serialNumbers.Add("EYEX2-030135523271");
            serialNumbers.Add("EYEX2-030135522271");
            serialNumbers.Add("EYEX2-030115207385");//大陸
            serialNumbers.Add("EYEX2-030115208375");//大陸
            serialNumbers.Add("EYEX2-030176701583");//2016.8
            serialNumbers.Add("EYEX2-030186919085");//2016.8
            serialNumbers.Add("EYEX2-030196813035");//2016.8
            serialNumbers.Add("EYEX2-030196813005");//2016.8
            serialNumbers.Add("EYEX2-030176701593");//2016.8
            serialNumbers.Add("EYEX2-030135525241");//蔡介立
            serialNumbers.Add("EYEX2-030135522211");

            //
            serialNumbers.Add("6904130020021501308");
            serialNumbers.Add("6904130020021501140");
            serialNumbers.Add("6904130020021501717");
            serialNumbers.Add("6904130020031501903");//晁禾醫療
            serialNumbers.Add("6904130020021501140");//晁禾醫療
            serialNumbers.Add("6904130020021501717");//板橋個案
            serialNumbers.Add("6904130020101504486");
            serialNumbers.Add("6904130020101504573");
            serialNumbers.Add("6904130020101504496");
            serialNumbers.Add("6904130020101504463");
            serialNumbers.Add("6904130020101503809");

            serialNumbers.Add("6904130020141505064");
            serialNumbers.Add("6904130020141505846");
            serialNumbers.Add("6904130020101504148");
            serialNumbers.Add("6904130020101504113");
            serialNumbers.Add("6904130020101504261");
            serialNumbers.Add("6904130020101504478");
            serialNumbers.Add("6904130020101504079");
            serialNumbers.Add("6904130020101504257");
            serialNumbers.Add("6904130020101504480");
            serialNumbers.Add("6904130020101504504");


            //PCEyeGo
            serialNumbers.Add("PCEGO-010176119754");
            serialNumbers.Add("PCEGO-030165248671");
            serialNumbers.Add("PCEGO-030155931422");
            serialNumbers.Add("PCEGO-010176119754");

            //PCEXP
            serialNumbers.Add("PCEEX-030105528222");

            //PCEyeMini
            serialNumbers.Add("PCE1M-03011662582");
            if (!Initialize()) return false;

            foreach (string str in RexSNs)
            {
                if (uri.AbsoluteUri == str)
                {
                    tracker.StopTracking();
                    tracker.Disconnect();
                    tracker.Dispose();
                    return true;
                }
            }
            foreach (string str in serialNumbers)
            {
                if (str == tracker.GetDeviceInfo().SerialNumber)
                {
                    tracker.StopTracking();
                    tracker.Disconnect();
                    tracker.Dispose();
                    return true;
                }
            }
            uri = null;
            tracker.StopTracking();
            tracker.Disconnect();
            tracker.Dispose();
            return false;
        }

        public bool Initialize()
        {
            try
            {
                uri = new EyeTrackerCoreLibrary().GetConnectedEyeTracker();
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetConnectedEyeTracker ERROR:" + ex.ToString());
            }
            if (uri == null)
            {
                return false;
            }
            tracker = new EyeTracker(uri);
            var thread = new System.Threading.Thread(() =>
                {
                    try
                    {
                        tracker.RunEventLoop();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("EyeTracker RunEventLoop ERROR:" + ex.ToString());
                    }
                }
            );
            thread.Start();
            tracker.Connect();
            return true;
        }
    }
}
