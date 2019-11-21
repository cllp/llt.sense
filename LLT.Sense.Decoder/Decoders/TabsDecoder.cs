using System;
using System.Dynamic;
using LLT.Sense.Decoder;

/*
namespace TabsDecoder
{

    internal class TabsDecoder : IDeviceDecoder
    {
        object IDeviceDecoder.Decode(byte[] bytes)
        {
            var result = new TabsPayload
            {
                Status = bytes[1],
                BatteryVoltage = (((float)(bytes[1] >> 4)) + 25) / 10,
                Temp = ((bytes[2] << 1) >> 1) - 32,
                RH = ((bytes[3] << 1) >> 1),
                CO2 = (bytes[5] << 8 | bytes[4]),
                VOC = (bytes[7] << 8 | bytes[6]),
                BatteryLevel = 100 * (bytes[1] >> 4) / 15
            };
            return result;
        }
    }

    internal class TabsPayload
    {
        public string VOCLabelled => VOC + "ppb";
        public int CO2 { get; set; }
        public int BatteryLevel { get; set; }
        public string TempLabelled => Temp + "C";
        public int VOC { get; set; }
        public int RH { get; set; }
        public string RHLabelled => RH + "%";
        public string BatteryVoltageLabelled => BatteryVoltage + "V";
        public int Temp { get; set; }
        public float BatteryVoltage { get; set; }
        public string CO2Labelled => CO2 + "ppm";
        public string BatteryLabelled => BatteryLevel + "%";
        public int Status { get; set; }
    }
}
*/
