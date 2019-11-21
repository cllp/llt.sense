using System;
using Action = ActionFramework.Action;
using ActionFramework.Logger;
using ActionFramework.Helpers.Data.Interface;
using System.Collections.Generic;
using ActionFramework.Helpers.Data;

namespace LLT.API.Device
{
    public class GetDailyReport : Action
    {
        public string SenseConnectionString { get; set; }
        private IDataService _dataService;

        public override object Run(dynamic obj)
        {
            try
            {
                var apiKey = String.Empty;
                var emailFrom = String.Empty;
                var mail = new ActionFramework.Helpers.Messaging.SendMail(apiKey, emailFrom);
                mail.Send("claes-philip@staiger.se", "text/plain", "Testing", "this is my body");

                _dataService = DataFactory.GetDataService(SenseConnectionString);

                //get realestates
                var realestates = _dataService.GetMany<dynamic>("SELECT * FROM RealEstate");

                var resultObject = new List<dynamic>();

                foreach (var realEstate in realestates)
                {
                    var parameters = new Dictionary<string, string>();
                    parameters.Add("RealEstateId", realEstate.RealEstateId.ToString());

                    resultObject.Add(new {
                        RealEstate = realEstate,
                        //Devices = _dataService.GetMany<dynamic>("spGetDevicesByRealEstate", parameters),
                        MessageStatistics = _dataService.GetMany<dynamic>("spGetYesterdayMessageCount", parameters)
                    });
                }
            
                return new {
                    Date = DateTime.Now.AddDays(-1).ToShortDateString(),
                    Statistics = resultObject
                };
            }
            catch (Exception ex)
            {
                LogFactory.File.Error(ex, $"Action Search caused an exception");
                throw ex;
            }
        }

    }
}
