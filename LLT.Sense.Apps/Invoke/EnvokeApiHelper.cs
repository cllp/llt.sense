using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Invoke
{
    public class EnvokeApiHelper
    {
        private static readonly RestClient client = new RestClient("https://lorasensorsapi.azurewebsites.net/api/");
        private static readonly string _jsonContentType = "application/json";
        private static readonly string apiUsername = "vasakronantest@reqs.se";
        private static readonly string apiPassword = "spring2019!";
        private static readonly string companyId = "a3bfffb2-d112-44e7-96a9-6a4cf44c9444";
        private static readonly string apiKey = "dbc50598bc2652b26791223381178252ccc02b2e4e9a809d51f6ae6052838c00";
        //Vasakronan(a3bfffb2-d112-44e7-96a9-6a4cf44c9444)


        public string Token { get; private set; }

        public bool IsAuthenticated
        {
            get
            {
                return !string.IsNullOrEmpty(this.Token);
            }
        }

        public LoraSensorsApiHelper()
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
            var request = new RestRequest("users/login", Method.POST);
            request.AddHeader("ContentType", _jsonContentType);

            request.AddJsonBody(new
            {
                username = apiUsername,
                password = apiPassword
            });

            IRestResponse response = client.Execute(request);
            var content = response.Content;
            dynamic obj = System.Text.Json.JsonSerializer.Deserialize<dynamic>(content);
            Token = obj["token"];
            //Token = content["token"].ToString(); //response.Headers.ToList().Find(x => x.Name == "token").Value.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="devEui"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public dynamic GetDeviceMessages(string devEui, DateTime fromDate, DateTime toDate)
        {
            //?fromDate=2019-04-10&toDate=2019-04-11&expandAdditionalInfo=false&grainSize=hour
            var request = new RestRequest(string.Format("devicedata/{0}", devEui), Method.GET);
            request.AddHeader("ContentType", _jsonContentType);
            request.AddParameter("Authorization", string.Format("Bearer " + Token), ParameterType.HttpHeader);
            request.AddParameter(new Parameter("fromDate", fromDate, ParameterType.QueryString));
            request.AddParameter(new Parameter("toDate", toDate, ParameterType.QueryString));
            request.AddParameter(new Parameter("expandAdditionalInfo", false, ParameterType.QueryString));
            request.AddParameter(new Parameter("grainSize", "hour", ParameterType.QueryString));

            IRestResponse response = client.Execute(request);
            dynamic obj = System.Text.Json.JsonSerializer.Deserialize<dynamic>(response.Content.ToString());
            return obj;
        }

        /*
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public JArray GetDevices()
        {
            var devices = new List<Device>();
            var request = new RestRequest(string.Format("companies/{0}/sensors", companyId), Method.GET);
            request.AddHeader("ContentType", _jsonContentType);
            request.AddParameter("Authorization", string.Format("Bearer " + Token), ParameterType.HttpHeader);

            IRestResponse response = client.Execute(request);
            dynamic obj = JsonConvert.DeserializeObject(response.Content.ToString());
            return (JArray)obj;
        }
        */

        private RestClient InitializeAndGetClient()
        {
            var cookieJar = new CookieContainer();
            var client = new RestClient("https://xxxxxxx")
            {
                Authenticator = new HttpBasicAuthenticator(apiUsername, apiPassword),
                CookieContainer = cookieJar
            };

            return client;
        }
    }
}

