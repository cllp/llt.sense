using System;
using System.Collections.Generic;
using LLT.Sense.Decoder;
using System.Linq;
using ActionFramework.Helpers.Data.Interface;
using ActionFramework.Helpers.Data;
using System.Dynamic;
using ActionFramework.Configuration;
using System.Text.Json;

namespace Netmore
{
    public class NetmoreSchedule : ActionFramework.Action
    {
        private static NetmoreApiHelper blinkApiHelper = new NetmoreApiHelper();

        private IDataService _dataService;
        private ITableService _tableService;

        //public string SenseConnectionString { get; set; }
        
        public override object Run(dynamic obj)
        {
            _dataService = DataFactory.GetDataService(ConfigurationManager.Settings["AgentSettings:SenseConnectionString"]);
            _tableService = DataFactory.GetTableService(ConfigurationManager.Settings["AgentSettings:TableStorageConnectionstring"]);

            //declare the output
            var output = new ActionOutput();
            output.DeviceOutputs = new List<Dictionary<string, string>>();

            //get devices TODO: enable some sort of cache
            var devicePar = new Dictionary<string, string>();
            devicePar.Add("NetworkProvider", "Blink");
            var devices = _dataService.GetMany<dynamic>("spGetDevicesByNetworkProvider", devicePar);

            Log.Debug($"Number of devices {devices.Count()}");

            //loop through each device
            foreach (var device in devices)
            {
                //set the device count
                output.Devices++;

                //get messages from Blink
                var messages = blinkApiHelper.GetDeviceMessages(device.DevEui);

                Log.Debug($"Number of messages for device {device.DevEui} Count {messages.Count} LatestcommTimestamp {device.LatestCommTimestamp}");

                //declare the output object
                Dictionary<string, string> deviceOutput = new Dictionary<string, string>();
                deviceOutput.Add("DevEui", device.DevEui);
                deviceOutput.Add("Messages", messages.Count.ToString());
                output.TotalMessages = output.TotalMessages + messages.Count;

                //if there is messages
                if (messages != null && messages.Count > 0)
                {

                    //declare the deviceMEssages Object
                    dynamic deviceMessages = new ExpandoObject();
                    deviceMessages.DeviceId = device.DeviceId;
                    deviceMessages.DevEui = device.DevEui;
                    deviceMessages.LatestCommTimestamp = device.LatestCommTimestamp;
                    deviceMessages.Messages = new List<dynamic>();

                    //declare the new messages variable
                    var newmessages = 0;

                    //add a variable to check hourly gap
                    DateTime currentCommTimestamp = device.LatestCommTimestamp;

                    //loop throught all messages for the device
                    foreach (var msg in messages) //testing to reverse to get the first message first
                    {
                        var commTimestamp = DateTime.MinValue;

                        try
                        {
                            commTimestamp = DateTime.Parse(msg.GetProperty("commTimestamp").GetString());
                            //get the commtimestamp
                            //commTimestamp = DateTime.Parse(msg["commTimestamp"].ToString());//.ValidateDateTimeRange(device.DevEui);//ValidationHelper.ValidateDateTimeRange();
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, $"Could not parse timestamp");
                        }

                        //get the frequencyUp variable
                        var fcntUp = msg.GetProperty("fCntUp").ToString();

                        //if the fcntUp = 0, skip message as it is a config-message
                        if (int.Parse(fcntUp) > 0) //TODO check if we should remove this
                        {
                            //check if the message timestamp is "later" than the latest commstamp that is saved on the device. When saving the message, this is saved on the device in the database
                            if (commTimestamp > device.LatestCommTimestamp)
                            {                              
                                //set the current currentCommTimestamp to commtimestamp for next message, check if the messages comes in order
                                currentCommTimestamp = commTimestamp;

                                //we have a new message, increase the counter
                                newmessages++;

                                try
                                {
                                    //get the payload
                                    var payloadobj = JsonSerializer.Deserialize<dynamic>(msg.GetProperty("payload").ToString());
                                    var payload = payloadobj.GetProperty("payload").GetString();

                                    if (!string.IsNullOrEmpty(payload))
                                    {
                                        var decoder = new DeviceDecoder(device.DecodeClassName);

                                        try
                                        {
                                            //decode
                                            var decode_result = decoder.DecodeSensorData(payload);
                                            string payloaddata = Newtonsoft.Json.JsonConvert.SerializeObject(decode_result);//JsonSerializer.Serialize<dynamic>(decode_result);

                                            //add the message to the messages list
                                            deviceMessages.Messages.Add(new
                                            {
                                                Payload = payload,
                                                PayloadData = payloaddata,//JsonSerializer.Serialize(decode_result),
                                                FcntUp = fcntUp,
                                                CommTimestamp = commTimestamp
                                            });
                                        }
                                        catch (Exception ex)
                                        {
                                            Log.Error(ex, $"Error while docoding payload '{payload}' for device device '{device.DevEui}'");
                                            throw ex;
                                        }
                                    }
                                    else
                                    {
                                        Log.Error($"Payload is null for device {device.DevEui}. FcntUp: '{fcntUp}'");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Log.Error(ex, $"An error occured when inserting message. DevEui: '{device.DevEui}'");
                                    throw ex;
                                }
                            }

                        }
                        else
                        {
                            Log.Information("fcntup = 0, the message is a config message, skipping message");
                        }
                    }

                    //insert messages
                    if (newmessages > 0)
                    {
                        try 
                        {
                            var parameters = new Dictionary<string, string>();
                            parameters.Add("DevEui", device.DevEui);
                            parameters.Add("LatestCommTimestamp", GetLatestCommTimestamp(deviceMessages.Messages).ToString());

                            //call the insert procedure
                            string insertresult = _dataService.Insert<dynamic>("spInsertBatchMessages", parameters, deviceMessages.Messages, "Keys", "dbo.BatchMessagesTableType");

                            Log.Debug($"{newmessages} new Messages for device {device.DevEui} Insert Result {insertresult}");

                            //insert table storage
                            var tableStorageResult = _tableService.InsertMany<dynamic>(deviceMessages.Messages, "DeviceMessage", device.DevEui);

                            deviceOutput.Add("InsertResult", insertresult);
                            device.LatestCommTimestamp = deviceMessages.LatestCommTimestamp;
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, $"Error while adding data for device {device.DevEui}");
                            //Dont throw error to break this session for all devices
                        }
                    }

                    //set the device latest commtimestamp after all messages is added to the list
                    deviceOutput.Add("NewMessages", newmessages.ToString());

                    if (newmessages > 0)
                    {
                        deviceOutput.Add("CommTimestampRange", $"{ GetFirstCommTimestamp(deviceMessages.Messages).ToString() } - { deviceMessages.LatestCommTimestamp}");
                        output.TotalNewMessages += newmessages;
                    }

                }
                else
                {
                    Log.Information($"DevEui {device.DevEui}. No messages. LatestCommTimestamp: {device.LatestCommTimestamp}");
                }

                output.DeviceOutputs.Add(deviceOutput);
            }

            Log.Information($"BlinkSchedule Total New Messages {output.TotalNewMessages.ToString()}");
            return output;
        }

        private DateTime GetLatestCommTimestamp(List<dynamic> messages)
        {
            if (messages.Count > 0)
            {
                var latest = messages.OrderByDescending(d => d.CommTimestamp).FirstOrDefault();
                return latest.CommTimestamp;
            }
            else
                return DateTime.MinValue;
        }

        private DateTime GetFirstCommTimestamp(List<dynamic> messages)
        {
            if (messages.Count > 0)
            {
                var latest = messages.OrderBy(d => d.CommTimestamp).FirstOrDefault();
                return latest.CommTimestamp;
            }
            else
                return DateTime.MinValue;
        }
    }
}
