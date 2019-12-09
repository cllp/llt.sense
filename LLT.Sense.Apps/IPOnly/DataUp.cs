using System;
using Action = ActionFramework.Action;
using LLT.Sense.Decoder;
using ActionFramework.Helpers.Data.Interface;
using ActionFramework.Helpers.Data;
using System.Collections.Generic;
using ActionFramework.Configuration;
using System.Text.Json;

namespace IPOnly
{
    public class DataUp : Action
    {
        private IDataService _dataService;
        private ITableService _tableService;

        //public string SenseConnectionString { get; set; }

        public override object Run(dynamic obj)
        {
            //parse dynamic as JsonElement
            var jsonobj = (JsonElement)obj;

            DateTime commTimestamp = DateTime.UtcNow;

            //init the message service
            _dataService = DataFactory.GetDataService(ConfigurationManager.Settings["AgentSettings:SenseConnectionString"]);
            _tableService = DataFactory.GetTableService(ConfigurationManager.Settings["AgentSettings:TableStorageConnectionstring"]);

            try
            {
                var endDevice = jsonobj.GetProperty("endDevice");
                string fcntDown = jsonobj.GetProperty("fCntDown").ToString();
                string fcntUp = jsonobj.GetProperty("fCntUp").ToString();
                string payload = jsonobj.GetProperty("payload").ToString();
                string devEui = endDevice.GetProperty("devEui").ToString();

                var devicePar = new Dictionary<string, string>();
                devicePar.Add("DevEui", devEui);

                var device = _dataService.GetSingle<dynamic>("spGetDeviceByDevEui", devicePar);

                if (device == null)
                    throw new Exception($"Can not find device by DevEui '{devEui}', device is null. 'procedure spGetDeviceByDevEui'");

                if (device.LatestCommTimestamp == null)
                    Log.Warning($"device.LatestCommTimestamp is null for device {device.DevEui}");

                device.LatestCommTimestamp = DateTime.MinValue.ToString();

                string payloaddata = null;

                //if no decoder, don´t decode
                if (!string.IsNullOrEmpty(device.DecodeClassName))
                {
                    var decoder = new DeviceDecoder(device.DecodeClassName);

                    var decode_result = decoder.DecodeSensorData(payload);
                    var json_result = Newtonsoft.Json.JsonConvert.SerializeObject(decode_result);

                    dynamic payloadobj = JsonSerializer.Deserialize<dynamic>(json_result.ToString());
                    payloaddata = payloadobj.ToString();

                    var message = new
                    {
                        //device.DevEui,
                        Payload = payload,
                        PayloadData = payloadobj.ToString(),
                        FcntUp = fcntUp,
                        FcntDown = fcntDown,
                        CommTimestamp = commTimestamp
                    };

                    var parameters = new Dictionary<string, string>();
                    parameters.Add("DeviceId", device.DeviceId.ToString());
                    parameters.Add("FcntUp", message.FcntUp.ToString());
                    parameters.Add("FcntDown", message.FcntDown.ToString());
                    parameters.Add("Payload", message.Payload);
                    parameters.Add("PayloadData", message.PayloadData);
                    parameters.Add("CommTimestamp", message.CommTimestamp.ToString());

                    string result = _dataService.Insert("spInsertMessage", parameters);
                    var tableresult = _tableService.InsertSingle<dynamic>(message, "DeviceMessage", device.DevEui, System.Guid.NewGuid().ToString());

                    //LogFactory.File.Information(device.LatestCommTimestamp == null ? "LatestCommTimestamp is null" : $"DevEui: {device.DevEui} LatestCommtimestamp: {device.LatestCommTimestamp.ToString()}");

                    return true;
                }
                else
                {
                    throw new Exception($"Device code class name is null or empty. DevEui: {device.DevEui}");
                }
            }
            catch (Exception ex)
            {
              
                throw ex;
            }
        }
    }
}
