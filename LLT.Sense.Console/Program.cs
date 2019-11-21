using System;
using System.Collections.Generic;

namespace LLT.Sense.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("Loading ActionFramework");

            foreach (var app in ActionFramework.AppContext.Current)
                System.Console.WriteLine($"App: {app.AppName}");

            System.Console.WriteLine("");

            System.Console.WriteLine("Running actions!");

            var actions = new List<string>() {
                 //"NetmorePush",
                //"TestAction01",
                //"NetmoreSchedule",
                "SearchMessages"
            };

            foreach (var action in actions)
            {
                System.Console.WriteLine($"");
                System.Console.WriteLine($" - - - - - - - - - -");
                System.Console.WriteLine($"Executing action {action}");
                var result = RunAction(action);
                System.Console.WriteLine($"Action {action} result: {result}");
            } 

            System.Console.ReadLine();
        }

        private static string RunAction(string actionname)
        {
            var input = ReadFile($"{actionname}.json");
            dynamic obj = null;

            if (!string.IsNullOrEmpty(input))
                obj = System.Text.Json.JsonSerializer.Deserialize<dynamic>(input);

            var action = ActionFramework.AppContext.Action(actionname);

            var result = action.Run(obj);

            if (result != null)
                return System.Text.Json.JsonSerializer.Serialize(result);
            else
                return "result is null";
        }

        private static string ReadFile(string file)
        {
            var path = $"{ActionFramework.Configuration.ConfigurationManager.AppRootDirectory}/json/{file}";

            if (System.IO.File.Exists(path))
                return System.IO.File.ReadAllText(path);
            else
                return "";
        }
    }
}
