using System;
using System.IO;
using System.Collections.Generic;
using System.Dynamic;
using LLT.Sense.Decoder;
//using Newtonsoft.Json;

namespace DecentlabDecoder
{
    public class DecentlabDecoder : IDeviceDecoder
    {
        private delegate float conversion(float x);

        private class SensorValue
        {
            internal string name { get; set; }
            internal string unit { get; set; }
            internal conversion convert;

            internal SensorValue(string name, string unit, conversion convert)
            {
                this.name = name;
                this.unit = unit;
                this.convert = convert;
            }
        }

        private static readonly List<List<SensorValue>> INDOOR_AMBIANCE_MONITOR = new List<List<SensorValue>>()
        {
            new List<SensorValue>() {
                new SensorValue("battery-voltage", "V", x => x / 1000),
            },
            new List<SensorValue>() {
                new SensorValue("air-temperature", "°C", x => 175 * x / 65535 - 45),
                new SensorValue("air-humidity", "%", x => 100 * x / 65535),
            },
            new List<SensorValue>() {
                new SensorValue("barometric-pressure", "Pa", x => x * 2),
            },
            new List<SensorValue>() {
                new SensorValue("ambient-light-visible-infrared", null, x => x),
                new SensorValue("ambient-light-infrared", null, x => x),
            },
            new List<SensorValue>() {
                new SensorValue("co2-concentration", "ppm", x => x - 32768),
                new SensorValue("co2-sensor-status", null, x => x),
                new SensorValue("co2-raw-reading", null, x => x),
            },
            new List<SensorValue>() {
                new SensorValue("pir-activity-counter", null, x => x),
            },
            new List<SensorValue>() {
                new SensorValue("total-voc", "ppb", x => x),
            },
        };

        private static int ReadInt(Stream stream)
        {
            return (stream.ReadByte() << 8) + stream.ReadByte();
        }

        private Dictionary<string, object> Decode(Stream msg)
        {
            var version = msg.ReadByte();

            if (version != 2)
            {
                throw new InvalidDataException("protocol version " + version + " doesn't match v2");
            }

            var result = new Dictionary<string, object>();
            var deviceId = ReadInt(msg);
            result["device"] = new { value = deviceId, unit = "" };
            var flags = ReadInt(msg);
            var sensors = INDOOR_AMBIANCE_MONITOR;
            foreach (var sensor in sensors)
            {
                if ((flags & 1) == 1)
                {
                    foreach (var val in sensor)
                    {
                        int rawValue = ReadInt(msg);
                        if (val.convert != null)
                        {
                            result[val.name] = new { value = (float)Math.Round((double)val.convert(rawValue), 2), unit = val.unit ?? "" };
                        }
                    }
                }
                flags >>= 1;
            }
            return result;
        }

        /*
        public object Decode(byte[] bytes, out List<DeviceData> deviceData)
        {
            deviceData = new List<DeviceData>();

            //Dictionary<string, Tuple<float, string>> result =  Decode(new MemoryStream(bytes));
            Dictionary<string, object> result = Decode(new MemoryStream(bytes));
            var obj = new ExpandoObject() as IDictionary<string, Object>;

            foreach (var k in result.Keys)
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(result[k]);
                obj.Add(k, json);
                dynamic resultobj = result[k];//JsonConvert.DeserializeObject(result[k].ToString());
                deviceData.Add(new DeviceData(k, resultobj.value.ToString(), resultobj.unit.ToString()));
                //Console.WriteLine(k + ": " + result[k]);
            }

            return result;
        }
        */

        public object Decode(byte[] bytes)
        {
            //Dictionary<string, Tuple<float, string>> result =  Decode(new MemoryStream(bytes));
            Dictionary<string, object> result = Decode(new MemoryStream(bytes));
            var obj = new ExpandoObject() as IDictionary<string, Object>;

            foreach (var k in result.Keys)
            {
                //TODO: VERIFY THAT THIS WORKS TO REMOVE THE JSON REFERENCE
                //var json = Newtonsoft.Json.JsonConvert.SerializeObject(result[k]);
                obj.Add(k, result[k]);
                //Console.WriteLine(k + ": " + result[k]);
            }

            return result;
        }
    }
}