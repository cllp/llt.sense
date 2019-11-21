using System;

using System.Collections.Generic;
using System.Text;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Messaging;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Build.Framework;
using Newtonsoft.Json;

namespace LLTSense.Actions
{
    public static class ExampleFunctions
    {
        [FunctionName("SimpleFunction")]
        public static void SimpleFunction(
            [MqttTrigger("my/topic/in", "connectionstring: Server=test.mosquitto.org:1883")] IMqttMessage message,
            [Mqtt] out IMqttMessage outMessage,
            Microsoft.Extensions.Logging.ILogger logger)
        {
            var body = message.GetMessage();
            var bodyString = Encoding.UTF8.GetString(body);
            logger.LogInformation($"{DateTime.Now:g} Message for topic {message.Topic}: {bodyString}");
            outMessage = new MqttMessage("testtopic/out", new byte[] { }, MqttQualityOfServiceLevel.AtLeastOnce, true);
        }
    }
}
