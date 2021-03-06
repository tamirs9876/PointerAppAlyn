﻿using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Alyn.Pointer.Common
{
    public class OS
    {
        /// <summary>
        /// Returns device-specific capabilities information
        /// </summary>
        /// <remarks>See more at http://pinvoke.net/default.aspx/gdi32/GetDeviceCaps.html</remarks>
        /// <param name="hdc"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        [DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(IntPtr hdc, int index);

        public enum DeviceCap
        {
            /// <summary>
            /// Horizontal width in pixels
            /// </summary>
            HORZRES = 8,

            /// <summary>
            /// Height, in raster lines, of the screen
            /// </summary>
            VERTRES = 10,

            /// <summary>
            /// Vertical height of entire desktop in pixels
            /// </summary>
            DESKTOPVERTRES = 117
        }

        public static float GetScalingFactor(IntPtr windowHandle)
        {
            var window = Graphics.FromHwnd(windowHandle);
            var deviceContext = window.GetHdc();
            var logicalScreenHeight = GetDeviceCaps(deviceContext, (int)DeviceCap.VERTRES);
            var physicalScreenHeight = GetDeviceCaps(deviceContext, (int)DeviceCap.DESKTOPVERTRES);

            var screenScalingFactor = (float)physicalScreenHeight / (float)logicalScreenHeight;

            return screenScalingFactor; // 1.25 = 125%
        }

        public static Size GetNativeResolution(IntPtr? windowHandle = null)
        {
            var window = Graphics.FromHwnd(windowHandle ?? IntPtr.Zero);
            var deviceContext = window.GetHdc();
            var height = GetDeviceCaps(deviceContext, (int)DeviceCap.VERTRES);
            var width = GetDeviceCaps(deviceContext, (int)DeviceCap.HORZRES);

            return new Size(width, height);
        }
    }
}
