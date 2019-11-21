using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LLT.Sense.Models;
using LLT.Sense.Decoder;
using LLT.Sense.Functions.Helpers;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LLT.Sense.Functions
{
    public static class Blink
    {
        //private static TableStorageService _storageService;
        private static List<Device> devices;
        private static BlinkApiHelper blinkApiHelper = new BlinkApiHelper();
        //private static SenseApiHelper senseApiHelper = new SenseApiHelper("http://localhost:4000/", "test", "test");
        //private static SenseApiHelper senseApiHelper = new SenseApiHelper("https://01-senseapi-dev.azurewebsites.net/", "test", "test");
        
        private static string newtworkProvider = "Blink";

        [FunctionName("Blink")]
        //public static void Run([TimerTrigger("0 */15 * * * *")]TimerInfo myTimer, TraceWriter log) //every 15 minutes
        public static async Task Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, ILogger log) //every 5 minutes
        //public static async Task Run([TimerTrigger("*/30 * * * * *")]TimerInfo myTimer, ILogger log)   //every 30 seconds
        //0 */15 * * * *
        {
            log.LogInformation($"C# Timer trigger function init");

            /*
            try
            {
                _storageService = new TableStorageService("SenseMessage");
            }
            catch (Exception ex)
            {
                var msg = string.Format("An error occured when instancing MessageService. '{0}'", ex.Message);
                log.LogError(msg, ex);
                throw ex;
            }
            */

            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}. Authenticated: {blinkApiHelper.IsAuthenticated}");

            //check if we have devices already todo: cache devices with expiry every day
            if (devices == null)
            {
                try
                {
                    devices = senseApiHelper.GetDevicesByNetworkProvider(newtworkProvider);
                }
                catch (Exception ex)
                {
                    var msg = string.Format("An error occured when getting devices. '{0}'", ex.Message);
                    log.LogError(msg, ex);
                    throw ex;
                }
            }

            foreach (var device in devices)
            {
                var deviceMessages = new DeviceMessages(device.DevEui);

                //get all todayś messages from Blink for the device
                var messages = blinkApiHelper.GetDeviceMessages(device.DevEui);

                //if there is messages
                if (messages != null && messages.Count > 0)
                {
                    var newmessages = 0;
                    foreach (var obj in messages.Reverse()) //testing to reverse to get the first message first
                    {
                        var commTimestamp = DateTime.Parse(obj["commTimestamp"].ToString());
                        //commTimestamp = commTimestamp.ToUniversalTime(); //Converting to UTC time
                        var fcntUp = obj["fCntUp"].ToString();

                        //check if the message timestamp is "later" than the latest commstamp that is saved on the device. When saving the message, this is saved on the device in the database
                        if (commTimestamp > device.LatestCommTimestamp)
                        {
                            //set a counter for new message
                            newmessages++;

                            try
                            {
                                //get the payload
                                dynamic payloadobj = JsonConvert.DeserializeObject(obj["payload"].ToString());
                                var payload = payloadobj["payload"].ToString();

                                //decode
                                var decoder = new DeviceDecoder(device.DecodeClassName);
                                var decode_result = decoder.DecodeSensorData(payload);

                                //add the message to the list
                                deviceMessages.Messages.Add(new Message()
                                {
                                    Payload = payload,
                                    FcntUp = fcntUp,
                                    CommTimestamp = commTimestamp,
                                    PayloadData = JsonConvert.SerializeObject(decode_result)
                                });
                            }
                            catch (Exception ex)
                            {
                                var msg = string.Format("An error occured when inserting message. DevEui: '{0}'. Exception: '{1}'", device.DevEui, ex.Message);
                                log.LogError(msg, ex);
                                throw ex;
                            }
                        }
                    }

                    //insert messages
                    string result = senseApiHelper.InsertMessages(deviceMessages);

                    //set the device latest commtimestamp after all messages
                    device.LatestCommTimestamp = deviceMessages.LatestCommstamp;

                    var logmsg1 = string.Format("DevEui {0}. Total messages {1}. New messages {2}. LatestCommTimestamp: {3}.", device.DevEui, messages.Count.ToString(), newmessages.ToString(), device.LatestCommTimestamp);
                    log.LogInformation(logmsg1);
                    //_messageService.Log(logmsg1);
                }
                else
                {
                    var logmsg1 = string.Format("DevEui {0}. No messages. LatestCommTimestamp: {1}.", device.DevEui, device.LatestCommTimestamp);
                    log.LogInformation(logmsg1);
                }
            }
        }
    }
}