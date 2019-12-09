using System;
using Action = ActionFramework.Action;
using ActionFramework.Logger;
using System.Collections.Generic;
using System.Linq;
using ActionFramework.Helpers.Data.Interface;
using ActionFramework.Helpers.Data;

namespace LLT.API.Device
{
    public class GetDeviceLogs : Action
    {
        //public string SenseConnectionString { get; set; }
        private IDataService _dataService;


        public override object Run(dynamic obj)
        {
            _dataService = DataFactory.GetDataService(SenseConnectionString);

            try
            {
                var parameters = new Dictionary<string, string>();
                parameters.Add("DevEui", obj);

                var result = _dataService.GetMany<dynamic>("spGetDeviceLogs", parameters);
                //var result = _messageService.GetDeviceInfo(devEui);

                return new
                {
                    Query = new
                    {
                        DevEui = obj,
                        Timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                        Count = result.Count()
                    },
                    Logs = result //JsonConvert.SerializeObject(result, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() })
                };
            }
            catch (Exception ex)
            {
                LogFactory.File.Error(ex, $"Action Search caused an exception");
                throw ex;//return StatusCode(500, ex);
            }
        }

    }
}
