using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Common
{
    public class OS
    {
        [DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(IntPtr hdc, int nIndex);
        public enum DeviceCap
        {
            VERTRES = 10,
            HORZRES = 8,
            DESKTOPVERTRES = 117,

            // http://pinvoke.net/default.aspx/gdi32/GetDeviceCaps.html
        }


        public static float getScalingFactor(IntPtr hwnd)
        {
            var g = System.Drawing.Graphics.FromHwnd(hwnd);
            var desktop = g.GetHdc();
            var LogicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.VERTRES);
            var PhysicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPVERTRES);

            var ScreenScalingFactor = (float)PhysicalScreenHeight / (float)LogicalScreenHeight;

            return ScreenScalingFactor; // 1.25 = 125%
        }

        public static Size GetNativeResolution(IntPtr? hwnd = null)
        {
            var g = System.Drawing.Graphics.FromHwnd(hwnd ?? IntPtr.Zero);
            var desktop = g.GetHdc();
            var height = GetDeviceCaps(desktop, (int)DeviceCap.VERTRES);
            var width = GetDeviceCaps(desktop, (int)DeviceCap.HORZRES);

            return new Size(width, height);
        }
    }
}
