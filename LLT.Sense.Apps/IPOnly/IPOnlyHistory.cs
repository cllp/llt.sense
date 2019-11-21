using System;
using Action = ActionFramework.Action;
using Newtonsoft.Json;
using LLT.Sense.Decoder;
using ActionFramework.Helpers.Data.Interface;
using ActionFramework.Helpers.Data;
using System.Collections.Generic;
using ActionFramework.Logger;
using IPOnly.Helpers;
using Newtonsoft.Json.Linq;
using System.Dynamic;
using System.Web;
using System.Linq;
using ActionFramework.Configuration;

namespace LLT.API.Ingest.IPOnly
{
    public class IPOnlyHistory : Action
    {
        private IDataService _dataService;
        private ITableService _tableService;
        private static readonly string Token = "eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJtLndlc3Rlcmx1bmQiLCJyb2xlIjoiQURNSU4iLCJjdXN0b21lcklkIjoyLCJpc3MiOiJnbXMiLCJleHAiOjE1Njk5MTQ2NDh9.NL8rG77lAeFeoMGsUtsKlvGGqIa6TZIITvx9HmiPd28";
        private static readonly string DeviceQuery = "SELECT d.DeviceId, d.DevEui, dt.DecodeClassName FROM device d INNER JOIN DeviceType dt ON d.DeviceTypeId = dt.DeviceTypeId WHERE NetworkProvider = 'IPOnly' AND LatestCommTimestamp > '2019-09-24'";
        public string SenseConnectionString { get; set; }
        public string TableStorageConnectionString { get; set; }
        private DateTime fromDate = DateTime.Parse("2019-09-18 08:00:00");
        private DateTime toDate = DateTime.Parse("2019-09-24 15:00:00");

        public override object Run(dynamic obj)
        {
            var TableStorageConnectionString = ConfigurationManager.Settings["AgentSettings:TableStorageConnectionstring"];

            _dataService = DataFactory.GetDataService(SenseConnectionString);
            _tableService = DataFactory.GetTableService(TableStorageConnectionString);

            try
            {
                //get all devices that needs fixing
                var devices = _dataService.GetMany<dynamic>(DeviceQuery);

                foreach (var device in devices)
                {
                    //declare the deviceMessages object
                    dynamic deviceMessages = new ExpandoObject();
                    deviceMessages.DeviceId = device.DeviceId;
                    deviceMessages.DevEui = device.DevEui;
                    deviceMessages.LatestCommTimestamp = device.LatestCommTimestamp;
                    deviceMessages.Messages = new List<dynamic>();

                    var decoder = new DeviceDecoder(device.DecodeClassName);

                    //get device data
                    IPOnlyApiHelper apiHelper = new IPOnlyApiHelper(Token);

                    var responseobj = apiHelper.GetDeviceMessages(device.DevEui);

                    int numberOfPages = int.Parse(responseobj.nbPages.Value.ToString());
                    int messageCount = int.Parse(responseobj.count.Value.ToString());

                    //get the messages from the initial reponse object
                    var msglist1 = ReadMessages(deviceMessages.DevEui, decoder, responseobj.list);
                    deviceMessages.Messages.AddRange(msglist1);

                    if (numberOfPages > 1)
                    {
                        for (int i = 2; i <= numberOfPages; i++) //start with page 2 as page 1 is already fetched
                        {
                            //start get messages of other pages
                            responseobj = apiHelper.GetDeviceMessages(device.DevEui, i);

                            //add the range of messages to the deviceMessages.Messages
                            var msglist2 = ReadMessages(deviceMessages.DevEui, decoder, responseobj.list);
                            deviceMessages.Messages.AddRange(msglist2);
                        }
                    }

                    var parameters = new Dictionary<string, string>();
                    parameters.Add("DevEui", device.DevEui);
                }

                return "Ok";
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error in action '{this.ActionName}'");
                throw ex;
            }
        }

        private List<dynamic> ReadMessages(string devEui, DeviceDecoder decoder, JArray messages)
        {
            List<dynamic> deviceMessages = new List<dynamic>();
            foreach (var message in messages)
            {
                var datenumber = message["date"].ToString();
                long unixnumber = long.Parse(datenumber);
                var date = FromUnixTime(unixnumber);

                //check if date falls between the two dates
                if (date.Ticks > fromDate.Ticks && date.Ticks < toDate.Ticks)
                {
                    //ActionFramework.Logger.LogFactory.File.Information($"Insert IPOnly Lost Message");

                    string payload = message["phyPayload"].ToString();
                    var fcntUp = message["fCnt"].ToString();
                    object decode_result;

                    try
                    {
                        decode_result = decoder.DecodeSensorData(payload);
                    }
                    catch (Exception ex)
                    {
                        var logmsg = $"Could not decode payload '{payload}' for device: {devEui}. DevEui: '{ex.Message}'. Stacktrace: {ex.StackTrace}";
                        //ActionFramework.Logger.LogFactory.File.Error(ex, logmsg);
                        throw new Exception(logmsg, ex);
                    }

                    deviceMessages.Add(new
                    {
                        Payload = payload,
                        PayloadData = JsonConvert.SerializeObject(decode_result),
                        FcntUp = fcntUp,
                        CommTimestamp = date
                    });
                }
                else
                {
                    //LogFactory.File.Information($"Message out of date-range {date}. DevEui {devEui}");
                }
            }

            return deviceMessages;
        }

        private DateTime FromUnixTime(long epoch)
        {
            var dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(epoch);
            return dt;
        }

    }
}
