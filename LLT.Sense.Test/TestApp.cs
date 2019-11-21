using System;
using System.Collections.Generic;
using ActionFramework;
using Xunit;

namespace LLT.Sense.Test
{
    public class TestApp
    {
        private static List<App> apps;
        public TestApp()
        {
            //apps = ActionFramework.AppContext.Current;

        }

        [Fact]
        public void TestAction01()
        {
            var input = "This will be output";
            var action = ActionFramework.AppContext.Action("TestAction01");
            var result = action.Run(input);
            Assert.Equal(input, result);
        }
    }
}
