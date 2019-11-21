using System;
using ActionFramework.Helpers.Data.Interface;
using ActionFramework.Helpers.Data;
using System.Collections.Generic;
using LLT.Sense.Decoder;
using ActionFramework.Configuration;
using System.Text.Json;
using static System.Text.Json.JsonElement;

namespace Netmore
{
    public class NetmorePush : ActionFramework.Action
    {
        private static NetmoreApiHelper netmoreApiHelper = new NetmoreApiHelper();
        private IDataService _dataService;
        private ITableService _tableService;
        public string SenseConnectionString { get; set; }

        public override object Run(dynamic obj)
        {


            Log.Information("Serilog Init");
            //declare the variables
            var devEui = "";
            var payload = "";
            int fcntUp = 0;
            DateTime timestamp = DateTime.MinValue;

            //iterate through the json array (only 1 object)
            foreach (var property in obj.EnumerateArray())
            {
                devEui = property.GetProperty("devEui").GetString();
                payload = property.GetProperty("payload").GetString();
                timestamp = property.GetProperty("timestamp").GetDateTime();
                fcntUp = property.GetProperty("fCntUp").GetInt32();
            }

            var TableStorageConnectionString = ConfigurationManager.Settings["AgentSettings:TableStorageConnectionstring"];

            //init the message service  
            _dataService = DataFactory.GetDataService(SenseConnectionString);
            _tableService = DataFactory.GetTableService(TableStorageConnectionString);

            var devicePar = new Dictionary<string, string>();
            devicePar.Add("DevEui", devEui);

            var device = _dataService.GetSingle<dynamic>("spGetDeviceByDevEui", devicePar);


            if (device != null)
            {
                //todo. stop process if wrong integration method
                string integrationMethodName = device.IntegrationMethodName;

                if (!integrationMethodName.Equals("MQTT"))
                {
                    var integrationWarningMessage = $"Skipped message, wrong integration method. Expected 'MQTT'. Method on device: '{integrationMethodName}'";
                    Log.Warning(integrationWarningMessage);
                    return integrationWarningMessage;
                }
                    
                var decoder = new DeviceDecoder(device.DecodeClassName);

                var decode_result = decoder.DecodeSensorData(payload);
                var json_result = Newtonsoft.Json.JsonConvert.SerializeObject(decode_result);

                var payloadobj = JsonSerializer.Deserialize<dynamic>(json_result.ToString());

                var message = new
                {
                    //device.DevEui,
                    Payload = payload,
                    PayloadData = payloadobj.ToString(),
                    FcntUp = fcntUp,
                    FcntDown = "0",
                    CommTimestamp = timestamp
                };

                if (device.LatestCommTimestamp == null)
                    device.LatestCommTimestamp = DateTime.MinValue.ToString();

                var parameters = new Dictionary<string, string>();
                parameters.Add("DeviceId", device.DeviceId.ToString());
                parameters.Add("FcntUp", message.FcntUp.ToString());
                parameters.Add("FcntDown", message.FcntDown.ToString());
                parameters.Add("Payload", message.Payload);
                parameters.Add("PayloadData", message.PayloadData);
                parameters.Add("CommTimestamp", message.CommTimestamp.ToString());

                string result = _dataService.Insert("spInsertMessage", parameters);

                //insert table storage TODO: Get the generated Id from DataTable and use as RowId in TableStorage
                var tableresult = _tableService.InsertSingle<dynamic>(message, "DeviceMessage", device.DevEui, System.Guid.NewGuid().ToString());

                return true;
            }
            else
            {
                var msg = $"Error device {devEui} coult not be found";
                Log.Error(msg);
                return msg;
            }
        }
    }
}
