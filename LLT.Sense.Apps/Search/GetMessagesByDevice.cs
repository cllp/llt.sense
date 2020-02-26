using System;
using Action = ActionFramework.Action;
using ActionFramework.Helpers.Data.Interface;
using ActionFramework.Helpers.Data;
using System.Text.Json;
using ActionFramework.Configuration;

namespace Search
{
    public class GetMessagesByDevice : Action
    {
        //public string SenseConnectionString { get; set; }
        public string SearchProcedure { get; set; }
        private IDataService _dataService;
       
        public override object Run(dynamic obj)
        {

            _dataService = DataFactory.GetDataService(ConfigurationManager.Settings["AgentSettings:SenseConnectionString"]);

            var jsonobj = (JsonElement)obj;

            var devEui = jsonobj.GetProperty("DevEui").GetString();
            var fromDate = jsonobj.GetProperty("FromDate").GetString();
            var toDate = jsonobj.GetProperty("ToDate").GetString();

            DateTime startQuery = DateTime.UtcNow;

            try
            {
                var body = @"from(bucket: 'lltsense')
                          |> range(start: 2020 - 02 - 20T18: 15:43.185557726Z, stop: 2020 - 02 - 20T21: 15:43.185557726Z)
                          |> filter(fn: (r) => r._measurement == 'metrics')
                          |> filter(fn: (r) => r._field == 'co2' or r._field == 'temperature')
                          |> filter(fn: (r) => r.devEui == '70B3D57BA0000BE5');";

                InfluxApiHelper apiHelper = new InfluxApiHelper();
                var results = apiHelper.GetDeviceMessages(body);

                /*
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
                */

                return results;
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
