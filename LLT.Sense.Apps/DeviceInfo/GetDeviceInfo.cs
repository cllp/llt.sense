using System;
using Action = ActionFramework.Action;
using ActionFramework.Logger;
using System.Collections.Generic;
using System.Linq;
using ActionFramework.Helpers.Data.Interface;
using ActionFramework.Helpers.Data;

namespace LLT.API.Device
{
    public class GetDeviceInfo : Action
    {
        public string SenseConnectionString { get; set; }
        private IDataService _dataService;

        public override object Run(dynamic obj)
        {
            _dataService = DataFactory.GetDataService(SenseConnectionString);

            try
            {
                var parameters = new Dictionary<string, string>();
                parameters.Add("devEui", obj);

                var result = _dataService.GetSingle<dynamic>("spGetDeviceInfo", parameters);

                return result.FirstOrDefault();
            }
            catch (Exception ex)
            {
                LogFactory.File.Error(ex, $"Action Search caused an exception");
                throw ex;//return StatusCode(500, ex);
            }
        }

    }
}
