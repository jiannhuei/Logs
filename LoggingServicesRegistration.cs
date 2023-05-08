using Atmi.Logging.Constants;
using Atmi.Logging.Interface;
using Atmi.Logging.Logger;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atmi.Logging
{
    public static class LoggingServicesRegistration
    {
        public static IServiceCollection ConfigureLoggingServices(this IServiceCollection services, IConfiguration configuration)
        {
            var customLogger = configuration.GetSection("CustomLogging")["Logger"];
            var logLocation = configuration["CustomLogging:SeqConfig:Location"];

            if (customLogger.ToLower() == LoggingConstants.SeqLogger)
            {
                Log.Logger = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .WriteTo.Seq(logLocation, eventBodyLimitBytes: 1000000)
                    .WriteTo.Console()
                    .CreateLogger();
                services.AddScoped<ICustomLogger, SeqLogger>();
            }

            else
            {
                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(configuration)
                    .CreateLogger();
                services.AddScoped<ICustomLogger, TextLogger>();
            }

            return services;
        }
    }
}
