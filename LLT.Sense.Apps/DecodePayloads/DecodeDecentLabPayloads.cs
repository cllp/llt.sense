using ActionFramework.Helpers.Data;
using ActionFramework.Helpers.Data.Interface;
using LLT.Sense.Decoder;
using System;
using System.Collections.Generic;
using System.Dynamic;

namespace DecodePayloads
{
    public class DecodeDecentLabPayloads : ActionFramework.Action
    {
        private IDataService _dataService;
        public string SenseConnectionString { get; set; }
        private static string DecoderName = "Decentlab";

        public override object Run(dynamic obj)
        {
            _dataService = DataFactory.GetDataService(SenseConnectionString);

            /*string selectquery = @"SELECT dm.DeviceMessageId, dm.Payload
                                    FROM DeviceMessage dm
                                    INNER JOIN Device d ON dm.DeviceId = d.DeviceId
                                    INNER JOIN DeviceType dt ON d.DeviceTypeId = dt.DeviceTypeId
                                    WHERE d.NetworkProvider = 'NiotNet'
                                    AND d.DeviceTypeId = 5
                                    AND dm.PayloadData LIKE '%Protocol version%'
                                    ORDER BY dm.Received DESC";
                                    */

            string selectQuery = @"SELECT dm.DeviceMessageId, dm.Payload
                                    FROM DeviceMessage dm
                                    WHERE dm.DeviceMessageId = 'E457351C-76E9-42C4-9C31-E4140F709A6F'";

                                    //--AND dm.Received BETWEEN '2019-11-07 08:02:41.9333333' AND '2019-11-08 09:22:40.6166667'

            var messages = _dataService.GetMany<dynamic>(selectQuery);

            var decoder = new DeviceDecoder(DecoderName);

            var decodeObject = new List<dynamic>();

            foreach (var message in messages)
            {
                var payload = message.Payload;
                var deviceMessageId = message.DeviceMessageId;
                //decode
                var decode_result = decoder.DecodeSensorData(payload);
                string payloaddata = Newtonsoft.Json.JsonConvert.SerializeObject(decode_result);

                decodeObject.Add(new { deviceMessageId, payloaddata });

                var parameters = new Dictionary<string, string>();
                parameters.Add("DeviceMessageId", deviceMessageId.ToString());
                parameters.Add("PayloadData", payloaddata);
                
                string result = _dataService.Insert("spUpdateMessagePayloadData", parameters);
            }

            return decodeObject;

            //throw new NotImplementedException();
        }
    }
}
