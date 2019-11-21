using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPOnly.Helpers
{
    public class IPOnlyApiHelper
    {
        public string Token { get; private set; }

        public bool IsAuthenticated
        {
            get
            {
                return !string.IsNullOrEmpty(this.Token);
            }
        }

        public IPOnlyApiHelper(string token)
        {
            this.Token = token;
        }

        public dynamic GetDeviceMessages(string devEui)
        {
            return GetDeviceMessages(devEui, 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="devEui"></param>
        /// <returns></returns>
        public dynamic GetDeviceMessages(string devEui, int page)
        {
            var client = new RestClient($"https://kerlink.wanesy.com/gms/application/endDevices/{devEui}/messages");
            client.AddDefaultHeader("Authorization", string.Format("Bearer {0}", Token));

            
            var request = new RestRequest();
            request.Method = Method.GET;
            request.AddHeader("Accept", "*/*");
            //request.AddHeader("Content-Type", "vnd.kerlink.iot-v1+json");

            //request.Parameters.Clear();
            request.AddParameter(new Parameter("pageSize", 1000, ParameterType.QueryString));
            request.AddParameter(new Parameter("page", page, ParameterType.QueryString));
            request.AddParameter(new Parameter("fields", "devEui,date,phyPayload,fCnt", ParameterType.QueryStringWithoutEncode));
            request.AddParameter(new Parameter("search", "{ \"operand\": \"direction\", \"operation\": \"EQ\", \"values\": [\"Downlink\"]}", ParameterType.QueryStringWithoutEncode));

            var response = client.Execute(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                try
                {
                    dynamic obj = JsonConvert.DeserializeObject(response.Content.ToString());
                    return obj;
                }
                catch (Exception ex)
                {
                    throw new Exception($"IPOnlyApiHelper: could not deserialize response content {response.Content.ToString()}", ex);
                }
            }
            else
            {
                throw new Exception($"IPOnlyApiHelper: statuscode {response.StatusCode}. error message {response.ErrorMessage}");
            }
        }
    }
}
