using System;
using System.Collections.Generic;
using System.Dynamic;
using LLT.Sense.Decoder;

namespace ElsysDecoder
{
    public class ElsysDecoder : IDeviceDecoder
    {
        public object Decode(byte[] bytes)
        {
            dynamic result = new ExpandoObject();

            for (var i = 0; i < bytes.Length; i++)
            {
                switch (bytes[i])
                {
                    case (byte)Sensor.Temp:
                        var temp = (bytes[i + 1] << 8) | (bytes[i + 2]);
                        temp = Bin16Dec(temp);
                        result.temperature = temp / 10.0;
                        i += 2;
                        break;

                    case (byte)Sensor.Humidity:
                        var rh = (bytes[i + 1]);
                        result.humidity = rh;
                        i += 1;
                        break;
                    case (byte)Sensor.Acc:
                        result.x = Bin8Dec(bytes[i + 1]);
                        result.y = Bin8Dec(bytes[i + 2]);
                        result.z = Bin8Dec(bytes[i + 3]);
                        i += 3;
                        break;
                    case (byte)Sensor.Light:
                        result.light = (bytes[i + 1] << 8) | (bytes[i + 2]);
                        i += 2;
                        break;
                    case (byte)Sensor.Motion:
                        result.motion = (bytes[i + 1]);
                        i += 1;
                        break;
                    case (byte)Sensor.CO2:
                        result.co2 = (bytes[i + 1] << 8) | (bytes[i + 2]);
                        i += 2;
                        break;
                    case (byte)Sensor.VDD:
                        result.vdd = (bytes[i + 1] << 8) | (bytes[i + 2]);
                        i += 2;
                        break;
                    case (byte)Sensor.Analog1:
                        result.analog1 = (bytes[i + 1] << 8) | (bytes[i + 2]);
                        i += 2;
                        break;
                    case (byte)Sensor.GPS:
                        result.latitude = (bytes[i + 1] << 16) | (bytes[i + 2] << 8) | (bytes[i + 3]);
                        result.longitude = (bytes[i + 4] << 16) | (bytes[i + 5] << 8) | (bytes[i + 6]);
                        i += 6;
                        break;
                    case (byte)Sensor.Pulse1:
                        result.pulse1 = (bytes[i + 1] << 8) | (bytes[i + 2]);
                        i += 2;
                        break;
                    case (byte)Sensor.Pulse1Abs:
                        var pulseAbs = (bytes[i + 1] << 24) | (bytes[i + 2] << 16) | (bytes[i + 3] << 8) | (bytes[i + 4]);
                        result.pulseAbs = pulseAbs;
                        i += 4;
                        break;
                    case (byte)Sensor.ExtTemp1:
                        var extTemp = (bytes[i + 1] << 8) | (bytes[i + 2]);
                        extTemp = Bin16Dec(extTemp);
                        result.externalTemperature = extTemp / 10;
                        i += 2;
                        break;
                    case (byte)Sensor.ExtDigital:
                        result.digital = (bytes[i + 1]);
                        i += 1;
                        break;
                    case (byte)Sensor.ExtDistance:
                        result.distance = (bytes[i + 1] << 8) | (bytes[i + 2]);
                        i += 2;
                        break;
                    case (byte)Sensor.AccMotion:
                        result.accMotion = (bytes[i + 1]);
                        i += 1;
                        break;
                    case (byte)Sensor.IrTemp:
                        var iTemp = (bytes[i + 1] << 8) | (bytes[i + 2]);
                        iTemp = Bin16Dec(iTemp);
                        var eTemp = (bytes[i + 3] << 8) | (bytes[i + 4]);
                        eTemp = Bin16Dec(eTemp);
                        result.irInternalTemperature = iTemp / 10;
                        result.irExternalTemperature = eTemp / 10;
                        i += 4;
                        break;
                    case (byte)Sensor.Occupancy:
                        result.occupancy = (bytes[i + 1]);
                        i += 1;
                        break;
                    case (byte)Sensor.WaterLeak:
                        result.waterleak = (bytes[i + 1]);
                        i += 1;
                        break;
                    case (byte)Sensor.GridEye:
                        i += 65;
                        break;
                    case (byte)Sensor.Pressure:
                        var press = (bytes[i + 1] << 24) | (bytes[i + 2] << 16) | (bytes[i + 3] << 8) | (bytes[i + 4]);
                        result.pressure = press / 1000;
                        i += 4;
                        break;
                    case (byte)Sensor.Sound:
                        result.soundPeak = bytes[i + 1];
                        result.soundAvg = bytes[i + 2];
                        i += 2;
                        break;
                    case (byte)Sensor.Pulse2:
                        result.pulse2 = (bytes[i + 1] << 8) | (bytes[i + 2]);
                        i += 2;
                        break;
                    case (byte)Sensor.Pulse2Abs:
                        result.pulseAbs2 = (bytes[i + 1] << 24) | (bytes[i + 2] << 16) | (bytes[i + 3] << 8) | (bytes[i + 4]);
                        i += 4;
                        break;
                    case (byte)Sensor.Analog2:
                        result.analog2 = (bytes[i + 1] << 8) | (bytes[i + 2]);
                        i += 2;
                        break;
                    case (byte)Sensor.ExtTemp2:
                        var extTemp2 = (bytes[i + 1] << 8) | (bytes[i + 2]);
                        extTemp2 = Bin16Dec(extTemp2);
                        result.externalTemperature2 = extTemp2 / 10;
                        i += 2;
                        break;
                    default:
                        throw new Exception("Something is wrong with byte array");
                }
            }
            
            return result;
        }

        public object Decode(byte[] bytes, out List<DeviceData> deviceData)
        {
            deviceData = new List<DeviceData>();
            dynamic result = Decode(bytes);

            foreach(var item in result)
            {
                deviceData.Add(new DeviceData(item.Key.ToString(), item.Value.ToString(), ""));
            }

            //todo serialize json resul to List<DeviceData>

            return result;
        }

        private int Bin16Dec(int bin)
        {
            var num = bin & 0xFFFF;
            if (0x8000 == num)
                num = -(0x010000 - num);
            return num;
        }

        private int Bin8Dec(int bin)
        {
            var num = bin & 0xFF;
            if (0x80 == num)
                num = -(0x0100 - num);
            return num;
        }
    }

    internal enum Sensor
    {
        Temp = 0x01, //temp 2 bytes -3276.8°C -->3276.7°C
        Humidity = 0x02, //Humidity 1 byte  0-100%
        Acc = 0x03, //acceleration 3 bytes X,Y,Z -128 --> 127 +/-63=1G
        Light = 0x04, //Light 2 bytes 0-->65535 Lux
        Motion = 0x05, //No of motion 1 byte  0-255
        CO2 = 0x06, //Co2 2 bytes 0-65535 ppm 
        VDD = 0x07, //VDD 2 byte 0-65535mV
        Analog1 = 0x08, //VDD 2byte 0-65535mV
        GPS = 0x09, //3 bytes lat 3bytes long binary
        Pulse1 = 0x0A, //2 bytes relative pulse count
        Pulse1Abs = 0x0B,  //4 bytes no 0->0xFFFFFFFF
        ExtTemp1 = 0x0C,  //2 bytes -3276.5C-->3276.5C
        ExtDigital = 0x0D,  //1 bytes value 1 or 0
        ExtDistance = 0x0E,  //2 bytes distance in mm
        AccMotion = 0x0F,  //1 byte number of vibration/motion
        IrTemp = 0x10,  //2 bytes internal temp 2bytes external temp -3276.5C-->3276.5C
        Occupancy = 0x11,  //1 byte data
        WaterLeak = 0x12,  //1 byte data 0-255 
        GridEye = 0x13,  //65 byte temperature data 1byte ref+64byte external temp
        Pressure = 0x14,  //4 byte pressure data (hPa)
        Sound = 0x15,  //2 byte sound data (peak/avg)
        Pulse2 = 0x16,  //2 bytes 0-->0xFFFF
        Pulse2Abs = 0x17,  //4 bytes no 0->0xFFFFFFFF
        Analog2 = 0x18,  //2 bytes voltage in mV
        ExtTemp2 = 0x19,  //2 bytes -3276.5C-->3276.5C
    }
}
