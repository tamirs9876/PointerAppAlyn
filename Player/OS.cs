using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace UI
{
    class OS
    {
        [DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(IntPtr hdc, int nIndex);
        public enum DeviceCap
        {
            VERTRES = 10,
            DESKTOPVERTRES = 117,

            // http://pinvoke.net/default.aspx/gdi32/GetDeviceCaps.html
        }


        public static float GetScalingFactor(IntPtr hwnd)
        {
            var g = Graphics.FromHwnd(hwnd);
            var desktop = g.GetHdc();
            var logicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.VERTRES);
            var physicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPVERTRES);

            var screenScalingFactor = (float)physicalScreenHeight / (float)logicalScreenHeight;

            return screenScalingFactor; // 1.25 = 125%
        }
    }
}
