using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SenseServiceBus;
using Serilog;
using Serilog.Sinks.Graylog;

[assembly: WebJobsStartup(typeof(StartUp))]
namespace SenseServiceBus
{
    public class StartUp : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {

            builder.Services.AddLogging(
                lb => lb//.ClearProviders()
                    .AddSerilog(
                        new LoggerConfiguration()
                            .WriteTo.Console()
                            .WriteTo.Graylog(new GraylogSinkOptions()
                            {
                                HostnameOrAddress = "3lkjk6.stackhero-network.com", //3lkjk6.stackhero-network.com
                                Port = 12201,
                                MinimumLogEventLevel = Serilog.Events.LogEventLevel.Information,
                                Facility = "SenseServiceBus",
                                TransportType = Serilog.Sinks.Graylog.Core.Transport.TransportType.Udp
                            })
                            .CreateLogger(),
                        dispose: true));
        }
    }
}
