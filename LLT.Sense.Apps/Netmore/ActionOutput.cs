using System;
using System.Collections.Generic;

namespace Netmore
{
    public class ActionOutput
    {
        public int Devices { get; set; }
        public int TotalMessages { get; set; }
        public int TotalNewMessages { get; set; }
        public List<Dictionary<string, string>> DeviceOutputs { get; set; }
    }
}
