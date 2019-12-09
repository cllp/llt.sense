using System;
using Action = ActionFramework.Action;
using ActionFramework.Logger;
using ActionFramework.Helpers.Data.Interface;
using System.Collections.Generic;
using ActionFramework.Helpers.Data;

namespace LLT.API.Device
{
    public class GetDevices : Action
    {
        //public string SenseConnectionString { get; set; }
        private IDataService _dataService;

        public override object Run(dynamic obj)
        {
            _dataService = DataFactory.GetDataService(SenseConnectionString);
            var result = _dataService.GetSingle<dynamic>("spGetDevices", null);

            try
            {
                return result;
            }
            catch (Exception ex)
            {
                LogFactory.File.Error(ex, $"Action Search caused an exception");
                throw ex;
            }
        }

    }
}
