using System;
using Action = ActionFramework.Action;
using System.Collections.Generic;
using System.Linq;
using ActionFramework.Helpers.Data.Interface;
using ActionFramework.Helpers.Data;
using System.Text.Json;
using ActionFramework.Configuration;

namespace Search
{
    public class SearchMessages : Action
    {
        //public string SenseConnectionString { get; set; }
        public string SearchProcedure { get; set; }
        private IDataService _dataService;
       
        public override object Run(dynamic obj)
        {

            _dataService = DataFactory.GetDataService(ConfigurationManager.Settings["AgentSettings:SenseConnectionString"]);

            var jsonobj = (JsonElement)obj;

            //declare parameter list
            var parameters = new Dictionary<string, string>();

            //looping through the input search object, add all items as parameters to the stored procedure
            foreach (var item in jsonobj.EnumerateObject())
            {
               // var parms = item.
                parameters.Add(item.Name, item.Value.ToString());
            }

            DateTime startQuery = DateTime.UtcNow;

            try
            {
                var result = _dataService.GetMany<dynamic>(SearchProcedure, parameters);

                var formattedResults = new List<dynamic>();

                foreach (var item in result)
                {
                    formattedResults.Add(new
                    {
                        item.devEui,
                        item.deviceStatus,
                        item.created,
                        item.sendFrequencySec,
                        item.littera,
                        item.deviceTypeName,
                        item.commTimestamp,
                        payloadData = JsonSerializer.Deserialize<dynamic>(item.payloadData),
                    });
                }

                var duration = String.Format("{0}ms", GetDuration(startQuery, DateTime.UtcNow));

                Log.Information($"Executed search. Duration: {duration}. Count: {result.Count()}");

                return new
                {
                    DeviceTypeName = jsonobj.GetProperty("DeviceTypeName").GetString(),
                    QueryTimestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    Duration = duration,
                    Count = result.Count(),
                    Messages = formattedResults //result
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Action Search caused an exception");
                throw new Exception($"Action Search caused an exception. Message: '{ex.Message}'");
            }
        }

        private int GetDuration(DateTime dt1, DateTime dt2)
        {
            TimeSpan span = dt2 - dt1;
            int ms = (int)span.TotalMilliseconds;
            return ms;
        }

    }
}
