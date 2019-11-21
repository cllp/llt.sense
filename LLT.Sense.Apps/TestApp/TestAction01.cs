using System;
using System.Runtime.CompilerServices;

namespace TestApp
{
    public class TestAction01 : ActionFramework.Action
    {
        public override object Run(dynamic obj)
        {

            //var action = ActionFramework.AppContext.Action("TestAction01");
            //getaction.Run("");

            Log.Information($"This is the log from {this.ActionName}");
            return "Ok";
        }
    }
}
