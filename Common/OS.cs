﻿using System;
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
            System.Drawing.Graphics g = System.Drawing.Graphics.FromHwnd(hwnd);
            IntPtr desktop = g.GetHdc();
            int LogicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.VERTRES);
            int PhysicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPVERTRES);

            float ScreenScalingFactor = (float)PhysicalScreenHeight / (float)LogicalScreenHeight;

            return ScreenScalingFactor; // 1.25 = 125%
        }

        public static Size GetNativeResolution(IntPtr? hwnd = null)
        {
            System.Drawing.Graphics g = System.Drawing.Graphics.FromHwnd(hwnd ?? IntPtr.Zero);
            IntPtr desktop = g.GetHdc();
            int height = GetDeviceCaps(desktop, (int)DeviceCap.VERTRES);
            int width = GetDeviceCaps(desktop, (int)DeviceCap.HORZRES);

            return new Size(width, height);
        }
    }
}
