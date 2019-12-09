using System;
using System.Data;
using System.Data.SqlClient;
using System.Text.Json;
using ActionFramework.Configuration;
using ActionFramework.Helpers.Data;
using ActionFramework.Helpers.Data.Interface;
using LLT.Sense.Decoder;

namespace Ingest
{
    public class IngestMessage : ActionFramework.Action
    {
        private IDataService _dataService;
        private ITableService _tableService;
        public string StorageTableName { get; set; }

        public override object Run(dynamic obj)
        {
            try
            {
                _tableService = DataFactory.GetTableService(ConfigurationManager.Settings["AgentSettings:TableStorageConnectionstring"]);

                var jsonobj = (JsonElement)obj;
                var payloadProperty = jsonobj.GetProperty("payload");

                var payloadobj = jsonobj;

                var deviceTypeId = payloadobj.GetProperty("deviceTypeId").GetInt32();
                var payload = payloadobj.GetProperty("payload").GetString();
                var payloadData = payloadobj.GetProperty("payloadData").GetString();

                //get the decode class from deviceTypeId
                var decodeClassName = GetDecodeClassName(deviceTypeId);

                //decode if decodeclass is not undefined
                if (!string.Equals(decodeClassName, "undefined"))
                {
                    var decoder = new DeviceDecoder(decodeClassName);
                    var decode_result = decoder.DecodeSensorData(payload);
                    payloadData = Newtonsoft.Json.JsonConvert.SerializeObject(decode_result);
                }

                //create the message object
                var message = new
                {
                    DeviceId = payloadobj.GetProperty("deviceId").GetInt32(),
                    DeviceTypeId = deviceTypeId,
                    DevEui = payloadobj.GetProperty("devEui").GetString(),
                    Payload = payload,
                    PayloadData = payloadData,
                    FcntUp = payloadobj.GetProperty("fCntUp").GetInt32(),
                    FcntDown = "0",
                    CommTimestamp = payloadobj.GetProperty("commTimestamp").GetDateTime(),
                    TransactionId = payloadobj.GetProperty("transactionId").GetString()
                };

                Log.Information($"Ingest device {message.DevEui}");

                //debug the messageobject
                Log.Debug(System.Text.Json.JsonSerializer.Serialize(message));

                //sql service
                string sqlresult = InsertMessage(message);

                //declare the table storage message
                var tblMessage = new {
                    message.DeviceId,
                    message.Payload,
                    message.PayloadData,
                    message.FcntUp,
                    message.CommTimestamp
                };

                //table service
                var tableresult = _tableService.InsertSingle<dynamic>(tblMessage, StorageTableName, message.DevEui, message.TransactionId);

                Log.Information($"SQL Result {sqlresult}");

                return new
                {
                    message.DeviceId,
                    message.TransactionId,
                    sqlresult
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occured for while ingest message");
                throw ex;
            }
        }

        private static string GetDecodeClassName(int deviceTypeId)
        {

            /*
            1	Undefined	NULL	NULL
            2	Elsys-ERS-1	Elsys	NULL
            3	Elsys-ERSCO2-1	Elsys	NULL
            4	Tracknet-TABS-HHS-1	Tabs	NULL
            5	Decentlab-DL-IAM-1	Decentlab	NULL
            6	Invoke-TVOC-1	NULL	NULL
            7	Yabby	JavaScript	NULL
            8	Oyster	JavaScript	NULL
            */
            switch (deviceTypeId)
            {
                case 2:
                case 3:
                    {
                        return "Elsys";
                    }
                case 5:
                    {
                        return "Decentlab";
                    }
                default:
                    {
                        return "undefined";
                    }
            }
        }

        private string InsertMessage(dynamic message)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.Settings["AgentSettings:SenseConnectionString"]);//DBConnect.myCon;
            SqlCommand cmd = con.CreateCommand();
            cmd.CommandText = "Execute spInsertNodeMessage @DeviceId,@FcntUp,@FcntDown,@Payload,@PayloadData,@CommTimestamp,@TransactionId";

            cmd.Parameters.Add("@DeviceId", SqlDbType.Int, 50).Value = message.DeviceId;
            cmd.Parameters.Add("@FcntUp", SqlDbType.BigInt, 50).Value = message.FcntUp;
            cmd.Parameters.Add("@FcntDown", SqlDbType.BigInt, 50).Value = message.FcntDown;
            cmd.Parameters.Add("@Payload", SqlDbType.NVarChar, 255).Value = message.Payload;
            cmd.Parameters.Add("@PayloadData", SqlDbType.NVarChar, 5000).Value = message.PayloadData;
            cmd.Parameters.Add("@CommTimestamp", SqlDbType.DateTime2).Value = message.CommTimestamp;
            cmd.Parameters.Add("@TransactionId", SqlDbType.NVarChar, 50).Value = message.TransactionId;

            con.Open();
            int result = cmd.ExecuteNonQuery();
            con.Close();
            return result.ToString();
        }
    }
}
