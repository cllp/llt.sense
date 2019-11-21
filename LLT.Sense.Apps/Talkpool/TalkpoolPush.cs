using System;
using Action = ActionFramework.Action;
using LLT.Sense.Decoder;
using ActionFramework.Helpers.Data.Interface;
using ActionFramework.Helpers.Data;
using System.Collections.Generic;
using System.Text.Json;

namespace Talkpool
{
    public class Message
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class TalkpoolPush : Action
    {
        private IDataService _dataService;
        private ITableService _tableService;

        public string SenseConnectionString { get; set; }

        public override object Run(dynamic obj)
        {
            var jsonobj = (JsonElement)obj;


            var TableStorageConnectionString = ActionFramework.Configuration.ConfigurationManager.Settings["AgentSettings:TableStorageConnectionstring"];

            //init the message service
            _dataService = DataFactory.GetDataService(SenseConnectionString);
            _tableService = DataFactory.GetTableService(TableStorageConnectionString);

            try
            {
               
                int fcntUp = jsonobj.GetProperty("seqno").GetInt32();
                string devEui = jsonobj.GetProperty("deviceEui").GetString().Replace("-", "");
                DateTime commTimestamp = jsonobj.GetProperty("time").GetDateTime();
                string payload = jsonobj.GetProperty("data").GetString();

                //ulong timeStamp = jsonobj.GetProperty("date").GetUInt64();
                //DateTime commTimestamp = FromUnixTime(timeStamp);

                var devicePar = new Dictionary<string, string>();
                devicePar.Add("DevEui", devEui);

                var device = _dataService.GetSingle<dynamic>("spGetDeviceByDevEui", devicePar);

                if (device == null)
                    throw new Exception($"Can not find device by DevEui '{devEui}', device is null. 'procedure spGetDeviceByDevEui'");

                var decoder = new DeviceDecoder(device.DecodeClassName);
                var decoderesult = decoder.DecodeSensorData(payload);

                string payloaddata = JsonSerializer.Serialize(decoderesult);

                var message = new
                {
                    //device.DevEui,
                    PayloadData = payloaddata,
                    FcntUp = fcntUp,
                    FcntDown = 0,
                    CommTimestamp = commTimestamp
                };

                var parameters = new Dictionary<string, string>();
                parameters.Add("DeviceId", device.DeviceId.ToString());
                parameters.Add("FcntUp", fcntUp.ToString());
                parameters.Add("FcntDown", "0");
                parameters.Add("Payload", payload);
                parameters.Add("PayloadData", payloaddata);
                parameters.Add("CommTimestamp", commTimestamp.ToString());

                string result = _dataService.Insert("spInsertMessage", parameters);
                var tableresult = _tableService.InsertSingle<dynamic>(message, "DeviceMessage", device.DevEui, System.Guid.NewGuid().ToString());

                return true;
            }
            catch (Exception ex)
            {
               
                Log.Error(ex, $"Error in action '{this.ActionName}'");
                throw ex;
            }
        }

        private DateTime FromUnixTime(ulong epoch)
        {
            var dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(epoch);
            return dt;
        }
    }
}
