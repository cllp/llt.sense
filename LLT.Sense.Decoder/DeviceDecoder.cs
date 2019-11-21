using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
//using Newtonsoft.Json.Linq;
//using Serilog;

namespace LLT.Sense.Decoder
{
    /// <summary>
    /// Device data entity
    /// </summary>
    public class DeviceData
    {
        public string Key { get; private set; }
        public string Value { get; private set; }
        public string Unit { get; private set; }

        public DeviceData(string key, string value, string unit)
        {
            this.Key = key;
            this.Value = value;
            this.Unit = unit;
        }
    }

    /// <summary>
    /// Device decoder.
    /// </summary>
    public class DeviceDecoder
    {
        //private ILogger log;
        private string decodername;
        private IDeviceDecoder decoder;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Cllp.DeviceDecoder.DeviceDecoder"/> class.
        /// </summary>
        /// <param name="decodername">Decodername.</param>
        public DeviceDecoder(string decodername)
        {
            //log = new LoggerConfiguration().WriteTo.File(string.Format("Logs/{0}-decoder.log", DateTime.UtcNow.ToString("yyyy-MM-dd"))).CreateLogger();
            //log.Information(string.Format("Init decoder: {0}", decodername));
            this.decodername = decodername;

            try
            {
                this.decoder = GetInstalledDeviceDecoder(decodername);
            }
            catch (Exception ex)
            {
                var msg = string.Format("Could not get installed decoder '{0}' in the directory: {1}.", decodername, GetInstalledDecodersDirectory());
                throw new Exception(msg, ex);
                //log.Error(ex, msg);
            }

            if (decoder == null)
            {
                var msg = string.Format("Could not find decoder: '{0}' in the directory '{1}'. Decoder is null.", decodername, GetInstalledDecodersDirectory());
                //log.Error(msg);
                throw new Exception(msg);
            }
        }

        /// <summary>
        /// Decodes the sensor data.
        /// </summary>
        /// <returns>The sensor data.</returns>
        /// <param name="payload">Payload.</param>
        //public JToken DecodeSensorData(string payload)
        public object DecodeSensorData(string payload)
        {
            //log.Information(string.Format("Decoding payload: {0}", payload));

            try
            {
                var decodedPayload = Decode(payload);
                object decoded = decoder.Decode(decodedPayload);
                //log.Information(string.Format("Decoding successful. Returning object!"));
                //return JToken.FromObject(decoded);
                return decoded;
            }
            catch (Exception ex)
            {
                var msg = string.Format("Could not convert payload '{0}' to byte.", payload);
                throw new Exception(msg, ex);
            }
        }

        /*
        public JToken DecodeSensorData(string payload, out List<DeviceData> deviceData)
        {
            log.Information(string.Format("Decoding payload: {0}", payload));

            try
            {
                var decodedPayload = Decode(payload);
                object decoded = decoder.Decode(decodedPayload, out deviceData);
                log.Information(string.Format("Decoding successful. Returning object!"));
                return JToken.FromObject(decoded);
            }
            catch (Exception ex)
            {
                var msg = string.Format("Could not convert payload '{0}' to byte.", payload);
                log.Error(ex, msg);
                throw ex;
            }
        }
        */

        /// <summary>
        /// Gets the installed decoders directory.
        /// </summary>
        /// <returns>The installed decoders directory.</returns>
        private string GetInstalledDecodersDirectory()
        {
            return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="decoder"></param>
        /// <returns></returns>
        private IDeviceDecoder GetInstalledDeviceDecoder(string decoder)
        {
            if (decoder.Contains("-"))
                decoder = decoder.Split('-').First();

            var decoders = new List<IDeviceDecoder>();
            decoders.Add(new ElsysDecoder.ElsysDecoder());
            decoders.Add(new DecentlabDecoder.DecentlabDecoder());

            return decoders.Where(d => d.GetType().FullName.ToLower().Contains(decoder.ToLower())).FirstOrDefault();
        }

        /// <summary>
        /// Converts to byte.
        /// </summary>
        /// <returns>The to byte.</returns>
        /// <param name="payload">Payload.</param>
        private byte[] ConvertToByte(String payload)
        {
            return Decode(payload);
        }

        /// <summary>
        /// Decode the specified payload.
        /// </summary>
        /// <returns>The decode.</returns>
        /// <param name="payload">Payload.</param>
        private byte[] Decode(String payload)
        {
            byte[] output = new byte[payload.Length / 2];
            for (int i = 0, j = 0; i < payload.Length; i += 2, j++)
            {
                output[j] = (byte)int.Parse(payload.Substring(i, 2), System.Globalization.NumberStyles.HexNumber);
            }
            return output;
        }

        /// <summary>
        /// This might be replaced with Decode above
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        private byte[] HexStringToByteArray(string hex)
        {
            if (hex.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");

            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }
            return arr;
        }

        /// <summary>
        /// THis might also be replaced since above is replaced
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        private int GetHexVal(char hex)
        {
            int val = hex;
            //For uppercase A-F letters:
            //return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            //return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }
    }
}
