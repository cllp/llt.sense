using System;
using Action = ActionFramework.Action;
using ActionFramework.Logger;
using System.Collections.Generic;
using System.Linq;
using ActionFramework.Helpers.Data.Interface;
using ActionFramework.Helpers.Data;

namespace LLT.API.Device
{
    public class GetDeviceFrequencyStatus : Action
    {
        public string SenseConnectionString { get; set; }
        private IDataService _dataService;

        public override object Run(dynamic obj)
        {
            _dataService = DataFactory.GetDataService(SenseConnectionString);

            try
            {
                var parameters = new Dictionary<string, string>();
                parameters.Add("DevEui", obj.devEui.ToString());
                parameters.Add("FromDate", obj.fromDate.ToString());
                parameters.Add("ToDate", obj.toDate.ToString());

                var result = _dataService.GetMany<dynamic>("spGetDeviceMessageFrequency", parameters);

                /*List<dynamic> result = new List<dynamic>
                {
                    new { FcntUp = 101, CommTimestamp = "2019-01-01 00:00:01" },
                    new { FcntUp = 102, CommTimestamp = "2019-01-01 00:00:02" },
                    new { FcntUp = 103, CommTimestamp = "2019-01-01 00:00:03" },

                    //restart of sequence
                    new { FcntUp = 1, CommTimestamp = "2019-01-01 00:00:04" },
                    new { FcntUp = 2, CommTimestamp = "2019-01-01 00:00:05" },
                    new { FcntUp = 3, CommTimestamp = "2019-01-01 00:00:06" },
                
                    //this scenario = several restart of sequence -> FcntUp.
                    new { FcntUp = 1, CommTimestamp = "2019-01-01 00:00:07" },
                    new { FcntUp = 2, CommTimestamp = "2019-01-01 00:00:08" },
                    new { FcntUp = 3, CommTimestamp = "2019-01-01 00:00:09" },
                    new { FcntUp = 5, CommTimestamp = "2019-01-01 00:00:10" }
                };*/

                //var check = result.Where(r => r.FcntUp.Equalt("435578"));

                var lists = SplitListOnIndexRestart(result.ToList());

                var missingFrequence = new List<int>();

                foreach (var list in lists)
                {
                    missingFrequence.AddRange(GetMissingFrequence(list));
                }

                return new {
                    devEui = obj.devEui.ToString(),
                    receivedMessages = result.Count(),
                    dateRange = $"{obj.fromDate.ToString()} - {obj.toDate.ToString()}",
                    fcntUpRange = result.Count() > 0 ? $"{int.Parse(result.First().FcntUp.ToString())} - {int.Parse(result.Last().FcntUp.ToString())}" : string.Empty,
                    restartCount = lists.Count() > 1 ? lists.Count() : 0,
                    missingInFrequenceCount = missingFrequence.Count(),
                    missingInFrequence = missingFrequence
                };
                    
            }
            catch (Exception ex)
            {
                LogFactory.File.Error(ex, $"Action Search caused an exception");
                throw ex;//return StatusCode(500, ex);
            }
        }

        public List<List<dynamic>> SplitListOnIndexRestart(List<dynamic> messages)
        {
            var arraylist = new List<List<dynamic>>();

            //group by FcntUp and CommTimestamp
            var query = messages.GroupBy(x => new { x.FcntUp, x.CommTimestamp });

            //declare the current item
            dynamic currentItem = null;

            //declare the list of ranges
            List<dynamic> range = null;

            //loop through the the sorted list
            foreach (var item in query)
            {
                //check if start of new range
                if (currentItem == null || item.Key.FcntUp < currentItem.Key.FcntUp)
                {
                    //create a new list if the FcntUp starts on a new range
                    range = new List<dynamic>();

                    //add the list to the parent list
                    arraylist.Add(range);
                }

                //add the item to the sublist
                range.Add(item);

                //set the current item
                currentItem = item;
            }

            return arraylist;
        }

        public IEnumerable<int> GetMissingFrequence(List<dynamic> messages)
        {
            var strings = messages.OrderBy(o => o.Key.CommTimestamp).Select(o => o.Key.FcntUp.ToString()).ToArray();

            var list = Array.ConvertAll(strings, s => Int32.Parse(s)).OrderBy(i => i).Cast<int>();

            int min = list.Min();
            int max = list.Max();

            var result = Enumerable.Range(min, max - min + 1).Except(list);

            return result;
        }

    }
}
