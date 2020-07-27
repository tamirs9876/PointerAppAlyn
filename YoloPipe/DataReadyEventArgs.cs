﻿using System;

namespace Alyn.Pointer.YoloPipe
{
    public class DataReadyEventArgs : EventArgs
    {
        public DataReadyEventArgs(string data)
        {
            Data = data;
        }
        public string Data { get; private set; }
    }
}