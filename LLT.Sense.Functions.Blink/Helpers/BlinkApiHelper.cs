using LLT.Sense.Models;
//using LLT.Sense.Data.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LLT.Sense.Functions.Helpers
{
    public class BlinkApiHelper
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

        public BlinkApiHelper()
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
        /// <param name="token"></param>
        /// <returns></returns>
        public JArray GetDeviceMessages(string devEui)
        {
            var request = new RestRequest(string.Format("rest/net/sensors/{0}/values", devEui), Method.GET);
            request.AddHeader("ContentType", _jsonContentType);
            request.AddParameter("Authorization", string.Format("Bearer " + Token), ParameterType.HttpHeader);
            request.AddParameter(new Parameter("fromDate", DateTime.Now.ToString("yyyy-MM-dd"), ParameterType.QueryString));
            request.AddParameter(new Parameter("toDate", DateTime.Now.ToString("yyyy-MM-dd"), ParameterType.QueryString));

            IRestResponse response = client.Execute(request);
            dynamic obj = JsonConvert.DeserializeObject(response.Content.ToString());
            return (JArray)obj;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public JArray GetDevices()
        {
            var devices = new List<Device>();
            var request = new RestRequest("rest/net/sensors", Method.GET);
            request.AddHeader("ContentType", _jsonContentType);
            request.AddParameter("Authorization", string.Format("Bearer " + Token), ParameterType.HttpHeader);

            IRestResponse response = client.Execute(request);
            dynamic obj = JsonConvert.DeserializeObject(response.Content.ToString());
            return (JArray)obj;
        }
    }
}
