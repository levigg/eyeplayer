using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Drawing;

namespace Eyeplayer
{
    public class SetBackLight
    {
        [DllImport("gdi32.dll")]
        public unsafe static extern bool SetDeviceGammaRamp(IntPtr hdc, void* ramp);
        [DllImport("gdi32.dll")]
        public unsafe static extern bool GetDeviceGammaRamp(IntPtr hdc,ref RAMP r);
        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hWnd);
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct RAMP
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public UInt16[] Red;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public UInt16[] Green;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public UInt16[] Blue;
        }
        public void  Initialize()
        {
            Global.ramp = new RAMP();
            GetDeviceGammaRamp(GetDC(IntPtr.Zero), ref Global.ramp);
        }

        public unsafe void SetBrightness(short brightness)
        {
            if (brightness > 255)  brightness = 255;
            if (brightness < 0)    brightness = 0;
            short* gArray = stackalloc short[3 * 256];
            short* idx = gArray;
            for (int j = 0; j < 3; j++)
            {
                for (int i = 0; i < 256; i++)
                {
                    int arrayVal = i * (brightness + 128);
                    if (arrayVal > 65535) arrayVal = 65535;
                    *idx = (short)arrayVal;
                    idx++;
                }
            }
            bool retVal = SetDeviceGammaRamp(GetDC(IntPtr.Zero), gArray);
        }

        public unsafe void ReturnBrightness()
        {
            if (Global.ramp.Red == null) return;
            short* gArray = stackalloc short[3 * 256];
            short* idx = gArray;
            for (int i = 0; i < 256; i++)
            {
                int arrayVal = Global.ramp.Red[i];
                if (arrayVal > 65535) arrayVal = 65535;
                *idx = (short)arrayVal;
                idx++;
            }
            for (int i = 0; i < 256; i++)
            {
                int arrayVal = Global.ramp.Green[i];
                if (arrayVal > 65535) arrayVal = 65535;
                *idx = (short)arrayVal;
                idx++;
            }
            for (int i = 0; i < 256; i++)
            {
                int arrayVal = Global.ramp.Blue[i];
                if (arrayVal > 65535) arrayVal = 65535;
                *idx = (short)arrayVal;
                idx++;
            }
            SetDeviceGammaRamp(GetDC(IntPtr.Zero), gArray);
        }
    }
}

