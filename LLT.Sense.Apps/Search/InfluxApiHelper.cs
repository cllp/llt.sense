using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Search
{
    public class InfluxApiHelper
    {
        private static readonly RestClient client = new RestClient("https://timeseriesdata.lummelunda.tech/api/v2/");
        private static readonly string _jsonContentType = "application/vnd.flux";
        private static readonly string auth_header = "Token b40peA1RHh8onR2lLF3bSeRUweXsl1ckDCY9Vcf1b8tFswbLF3xXyfIScO7guWD5kMXvD6W4pxARAV9TSLauAA==";
        public string Token { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="devEui"></param>
        /// <returns></returns>
        public List<dynamic> GetDeviceMessages(string body)
        {

            var request = new RestRequest("query", Method.POST);
            request.AddHeader("ContentType", _jsonContentType);
            request.AddHeader("Authorization", auth_header);
            request.AddHeader("Accept", "application/csv");

            request.AddParameter(new Parameter("org", "llt", ParameterType.QueryString));

            Parameter p = new Parameter("text/plain", body, ParameterType.RequestBody);
            p.ContentType = "text/plain";
            request.AddParameter(p);

            IRestResponse response = client.Execute(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                try
                {
                    dynamic obj = System.Text.Json.JsonSerializer.Deserialize<List<dynamic>>(response.Content.ToString());
                    return obj;
                }
                catch (Exception ex)
                {
                    throw new Exception($"BlinkAPIHelper.GetDeviceMessages: could not deserialize response content {response.Content.ToString()}", ex);
                }
            }
            else
            {
                throw new Exception($"BlinkAPIHelper.GetDeviceMessages: statuscode {response.StatusCode}. error message {response.ErrorMessage}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<dynamic> GetDevices()
        {
            var devices = new List<dynamic>();
            var request = new RestRequest("rest/net/sensors", Method.GET);
            request.AddHeader("ContentType", _jsonContentType);
            request.AddParameter("Authorization", string.Format("Bearer " + Token), ParameterType.HttpHeader);

            IRestResponse response = client.Execute(request);
            devices = System.Text.Json.JsonSerializer.Deserialize<List<dynamic>>(response.Content.ToString());
            return devices;
        }
    }
}
