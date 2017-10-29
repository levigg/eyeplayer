using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Eyeplayer
{
    class TimeZoneInfo
    {

		[DllImport("kernel32.dll")]
		private extern static uint SetSystemTime(ref SYSTEMTIME lpSystemTime);

        private struct SYSTEMTIME
        {
            public ushort wYear;
            public ushort wMonth;
            public ushort wDayOfWeek;
            public ushort wDay;
            public ushort wHour;
            public ushort wMinute;
            public ushort wSecond;
            public ushort wMilliseconds;
        }

       
        //public SYSTEMTIME oTime = new SYSTEMTIME();
        public System.DateTime oUTC;
        public bool OnRefresh()
		{
			oUTC = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc).AddSeconds(getUtcTimeStamp());
			if (oUTC.Year == 1970) { return false; }
			System.DateTime oGMT = System.TimeZoneInfo.ConvertTime(oUTC, System.TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time"));
            return true;
		}

		static double getUtcTimeStamp()
		{
			System.Net.HttpWebRequest oWRq = (System.Net.HttpWebRequest)System.Net.WebRequest.Create("http://www.timeanddate.com/scripts/ts.php?ut=1443508040915");
			oWRq.Timeout = 1000;
			try
			{
				System.Net.HttpWebResponse oWRp = (System.Net.HttpWebResponse)oWRq.GetResponse();
				using (System.IO.StreamReader oSR = new System.IO.StreamReader(oWRp.GetResponseStream(), System.Text.Encoding.UTF8))
				{
					string cUTC = oSR.ReadToEnd();
					if (cUTC.IndexOf(".") != -1  ||  cUTC.IndexOf(" ") != -1)
					{
						return System.Convert.ToDouble(cUTC.Split(' ')[0]);
					}
					else
					{
						return 0.0;
					}
				}
			}
			catch
			{
				return 0.0;
			}
		}
	}
}
