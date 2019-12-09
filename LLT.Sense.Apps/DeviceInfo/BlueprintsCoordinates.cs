using System;
using Action = ActionFramework.Action;
using ActionFramework.Logger;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using ActionFramework.Helpers.Data.Interface;
using ActionFramework.Helpers.Data;

namespace LLT.API.Device
{
    public class BlueprintsCoordinates : Action
    {
        //public string SenseConnectionString { get; set; }
        private IDataService _dataService;

        public override object Run(dynamic jsonData)
        {
            AddLog(LogType.Debug, jsonData);

            _dataService = DataFactory.GetDataService(SenseConnectionString);

            try
            {
                JArray array = JArray.Parse(jsonData.ToString());

                foreach (JObject obj in array.Children<JObject>())
                {
                    var parameters = new Dictionary<string, string>();
                    parameters.Add("Littera", obj["Littera"] == null ? "" : obj["Littera"].ToString()); //obj["Littera"].ToString());
                    parameters.Add("x", obj["x"] == null ? "0" : obj["x"].ToString());//obj["x"].ToString());
                    parameters.Add("y", obj["y"] == null ? "0" : obj["y"].ToString()); //obj["y"].ToString());
                    parameters.Add("y2", obj["y2"] == null ? "0" : obj["y2"].ToString());//obj["y2"].ToString());
                    parameters.Add("xov", obj["x-ov"] == null ? "0" : obj["x-ov"].ToString());//obj["x-ov"].ToString());
                    parameters.Add("yov", obj["y-ov"] == null ? "0" : obj["y-ov"].ToString());//obj["y-ov"].ToString());
                    parameters.Add("xovp", obj["yx-ovp2"] == null ? "0" : obj["yx-ovp2"].ToString());// obj["yx-ovp2"].ToString());
                    parameters.Add("yovp", obj["y-ovp"] == null ? "0" : obj["y-ovp"].ToString());//obj["y-ovp"].ToString());

                    var result = _dataService.Insert("spInsertPBICoordinates", parameters);
                }

                return true;
            }
            catch (Exception ex)
            {
                var errormsg = $"Action BlueprintsCoordinates caused an exception. Input: {jsonData.ToString()}";
                LogFactory.File.Error(ex, errormsg); 
                throw new Exception(errormsg, ex);
            }
        }
    }
}
