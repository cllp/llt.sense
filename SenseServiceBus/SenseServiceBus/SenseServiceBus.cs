using System;
using System.Collections.Generic;
using System.Configuration;
using ActionFramework.Helpers.Data;
using ActionFramework.Helpers.Data.Interface;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace SenseServiceBus
{
    public static class SenseServiceBus
    {
        private static IDataService _dataService;
        private static ITableService _tableService;

        private static string _senseConnectionString;
        private static string _tableStorageConnectionstring;
        private static ILogger _log;

        [FunctionName("SenseServiceBus")]
        public static void Run([ServiceBusTrigger("devicemessage", Connection = "SBConnectionString")]string myQueueItem, ILogger log)
        {
            _log = log;

            //_senseConnectionString = ConfigurationManager.ConnectionStrings["SenseConnectionString"].ConnectionString;
            //_tableStorageConnectionstring = ConfigurationManager.ConnectionStrings["TableStorageConnectionstring"].ConnectionString;

            _log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");

            var payloadObj = JsonConvert.DeserializeObject<dynamic>(myQueueItem);

            //create the message
            var message = new
            {
                DeviceId = payloadObj.deviceId.ToString(),
                DevEui = payloadObj.devEui.ToString(),
                Payload = payloadObj.payload.ToString(),
                PayloadData = payloadObj.payloadData.ToString(),
                FcntUp = payloadObj.fCntUp.ToString(),
                FcntDown = "0",
                CommTimestamp = payloadObj.commTimestamp.ToString(),
                TransactionId = payloadObj.transactionId.ToString()
            };

            var parameters = new Dictionary<string, string>();
            parameters.Add("DeviceId", message.DeviceId.ToString());
            parameters.Add("FcntUp", message.FcntUp.ToString());
            parameters.Add("FcntDown", message.FcntDown);
            parameters.Add("Payload", message.Payload);
            parameters.Add("PayloadData", message.PayloadData);
            parameters.Add("CommTimestamp", message.CommTimestamp.ToString());
            parameters.Add("TransactionId", message.TransactionId);

            //sql service
            string sqlresult = _dataService.Insert("spInsertNodeMessage", parameters);

            //declare the table storage message
            var tblMessage = new
            {
                message.DeviceId,
                message.Payload,
                message.PayloadData,
                message.FcntUp,
                message.CommTimestamp
            };

            //table service
            //var storageTableName = ConfigurationManager.AppSettings["StorageTableName"];
            //var tableresult = _tableService.InsertSingle<dynamic>(tblMessage, storageTableName, message.DevEui, message.TransactionId);

            log.LogInformation(JsonConvert.SerializeObject(message));
        }

        private static dynamic GetPayloadFromBase64(string base64)
        {
            _log.LogInformation($"ConvertBase64: {base64}");

            byte[] data = System.Convert.FromBase64String(base64);
            var base64Decoded = System.Text.Encoding.ASCII.GetString(data);

            var payloadobj = JsonConvert.DeserializeObject<dynamic>(base64Decoded);
            return payloadobj;
        }
    }
}
