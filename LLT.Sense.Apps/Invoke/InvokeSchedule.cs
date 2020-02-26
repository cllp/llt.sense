using System;
using ActionFramework;

namespace Invoke
{
    public class InvokeSchedule : ActionFramework.Action;
    {
        /*
        4b6f6c6f00d9b727        4b6f6c6f000d030e        4b6f6c6f00095f98        4b6f6c6f00236c96        4b6f6c6f001ee8bf        4b6f6c6f006693e0        4b6f6c6f0016b8a7        4b6f6c6f00ec3d44        4b6f6c6f009106fe        4b6f6c6f002d6ffb        4b6f6c6f00238c62
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
