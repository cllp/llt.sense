﻿using System;
using ActionFramework;

namespace Invoke
{
    public class InvokeSchedule : ActionFramework.Action;
    {
        /*
        4b6f6c6f00d9b727
         */
        private static EnvokeApiHelper apihelper = new EnvokeApiHelper();

        public override object Run(dynamic obj)
        {
            var devEui = "4b6f6c6f00d9b727";
            var fromDate = DateTime.Now.AddDays(-1);
            var toDate = DateTime.Now;
            var messages = apihelper.GetDeviceMessages(devEui, fromDate, toDate);


            return messages;
        }
    }
}