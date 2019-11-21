using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Netmore
{
    public class NetmoreApiHelper
    {
        private static readonly RestClient client = new RestClient("https://api.blink.services/");
        private static readonly string _jsonContentType = "application/json";
        private static readonly string blinkApiUsername = "ola.uden@gmail.com";
        private static readonly string blinkApiPassword = "Visby1234#";
        public string Token { get; private set; }

        public bool IsAuthenticated
        {
            get
            {
                return !string.IsNullOrEmpty(this.Token);
            }
        }

        public NetmoreApiHelper()
        {
            if (string.IsNullOrEmpty(Token))
                Authenticate();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private void Authenticate()
        {
            var request = new RestRequest(string.Format("rest/core/login/{0}", blinkApiUsername), Method.POST);
            request.AddHeader("ContentType", _jsonContentType);

            request.AddJsonBody(new
            {
                password = blinkApiPassword
            });

            IRestResponse response = client.Execute(request);
            var content = response.Content;
            Token = response.Headers.ToList().Find(x => x.Name == "token").Value.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="devEui"></param>
        /// <returns></returns>
        public List<dynamic> GetDeviceMessages(string devEui)
        {
            if (string.IsNullOrEmpty(devEui))
                throw new Exception($"BlinkAPIHelper.GetDeviceMessages: devEui is null or empty");

            var request = new RestRequest(string.Format("rest/net/sensors/{0}/values", devEui), Method.GET);
            request.AddHeader("ContentType", _jsonContentType);
            request.AddParameter("Authorization", string.Format("Bearer " + Token), ParameterType.HttpHeader);
            request.AddParameter(new Parameter("fromDate", DateTime.Now.ToString("yyyy-MM-dd"), ParameterType.QueryString));
            request.AddParameter(new Parameter("toDate", DateTime.Now.ToString("yyyy-MM-dd"), ParameterType.QueryString));

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
