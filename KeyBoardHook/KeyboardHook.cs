using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;

namespace WindowsHookSample
{
    public static class KeyboardHook
    {
        public static bool Monopolize { get; set; }
        public static event EventHandler<KeyEventArgs> GlobalKeyDown;
        public static event EventHandler<KeyEventArgs> GlobalKeyUp;
        public static bool Enabled
        {
            get { return m_Enabled; }
            set
            {
                if (m_Enabled != value)
                {
                    m_Enabled = value;
                    if (value)
                        Install();
                    else
                        Uninstall();
                }
            }
        }
        private static bool m_Enabled = false;

        private static int m_HookHandle = 0;
        private static NativeStructs.HookProc m_HookProc;

        private static void Install()
        {
            if (m_HookHandle == 0)
            {
                Process curProcess = Process.GetCurrentProcess();
                ProcessModule curModule = curProcess.MainModule;

                m_HookProc = new NativeStructs.HookProc(HookProc);
                m_HookHandle = NativeMethods.SetWindowsHookEx(NativeContansts.WH_KEYBOARD_LL, m_HookProc, NativeMethods.GetModuleHandle(curModule.ModuleName), 0);

                curModule.Dispose();
                curProcess.Dispose();

                if (m_HookHandle == 0)
                    throw new Exception("Install Hook Faild.");
            }
        }
        private static void Uninstall()
        {
            if (m_HookHandle != 0)
            {
                bool ret = NativeMethods.UnhookWindowsHookEx(m_HookHandle);

                if (ret)
                    m_HookHandle = 0;
                else
                    throw new Exception("Uninstall Hook Faild.");
            }
        }
        private static int HookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            KeyEventArgs e = null;
            int wParam_Int32 = wParam.ToInt32();
            if (nCode >= 0)
            {
                NativeStructs.KEYBOARDLLHookStruct keyboardHookStruct = (NativeStructs.KEYBOARDLLHookStruct)Marshal.PtrToStructure(lParam, typeof(NativeStructs.KEYBOARDLLHookStruct));
                if (GlobalKeyDown != null && (wParam_Int32 == NativeContansts.WM_KEYDOWN || wParam_Int32 == NativeContansts.WM_SYSKEYDOWN))
                {
                    e = new KeyEventArgs(keyboardHookStruct.VirtualKeyCode);
                    GlobalKeyDown.Invoke(null, e);
                }
                else if (GlobalKeyUp != null && (wParam_Int32 == NativeContansts.WM_KEYUP || wParam_Int32 == NativeContansts.WM_SYSKEYUP))
                {
                    e = new KeyEventArgs(keyboardHookStruct.VirtualKeyCode);
                    GlobalKeyUp.Invoke(null, e);
                }
            }

            if (Monopolize || (e != null && e.Handled))
                return -1;
            return NativeMethods.CallNextHookEx(m_HookHandle, nCode, wParam, lParam);
        }
        public class KeyEventArgs : EventArgs
        {
            public bool Handled { get; set; }
            public System.Windows.Forms.Keys Keys { get { return (System.Windows.Forms.Keys)VirtualKeyCode; } }
            public System.Windows.Input.Key Key { get { return System.Windows.Input.KeyInterop.KeyFromVirtualKey(VirtualKeyCode); } }
            public bool Alt
            {
                get
                {
                    return KeyIsDown((int)System.Windows.Forms.Keys.LMenu) || KeyIsDown((int)System.Windows.Forms.Keys.RMenu);
                }
            }
            public bool Control
            {
                get
                {
                    return KeyIsDown((int)System.Windows.Forms.Keys.LControlKey) || KeyIsDown((int)System.Windows.Forms.Keys.RControlKey);
                }
            }
            public bool Shift
            {
                get
                {
                    return KeyIsDown((int)System.Windows.Forms.Keys.LShiftKey) || KeyIsDown((int)System.Windows.Forms.Keys.RShiftKey);
                }
            }
            public int VirtualKeyCode { get; private set; }
            internal KeyEventArgs(int virtualKey)
            {
                this.Handled = false;
                this.VirtualKeyCode = virtualKey;
            }

            private static bool KeyIsDown(int KeyCode)
            {
                if ((NativeMethods.GetKeyState(KeyCode) & 0x80) == 0x80)
                    return true;
                else
                    return false;
            }
        }
    }
}
