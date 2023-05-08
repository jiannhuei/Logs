using Newtonsoft.Json;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using System.Text.RegularExpressions;

namespace Db2.Middleware.Logging
{
    public class SeqLogger : ICustomLogger
    {
        private static readonly string _messageTemplate =
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

        public void LogRequest(LogDetail detail)
        {
            LogEventLevel logLevel;
            var logger = CreateContext(detail);
            if (detail.Exception != null)
            {
                logLevel = LogEventLevel.Error;
                LogContext.PushProperty("Exception", detail.Exception);
            }
            else
            {
                logLevel = detail.StatusCode > 499 ? LogEventLevel.Error : LogEventLevel.Information;
            }

            logger.Write(logLevel, _messageTemplate, detail.HttpMethod, detail.HttpPath, detail.StatusCode,
                detail.ElapsedMs);
        }

        private static ILogger CreateContext(LogDetail detail)
        {
            return Log
                .ForContext("RequestHeaders", detail.RequestHeaders, true)
                .ForContext("RequestHost", detail.RequestHost)
                .ForContext("ServerName", detail.ServerName)
                .ForContext("RequestProtocol", detail.RequestProtocol)
                .ForContext("RequestBody", Beautify(detail.RequestBody))
                .ForContext("ResponseBody", Beautify(detail.ResponseBody))
                .ForContext("User", Beautify(detail.User))
                .ForContext("RemoteServerIp", detail.RemoteServerIp);
        }

        private static string Beautify(string json)
        {
            const string regex = @"{[^}]+}";
            json ??= "";
            return Regex.Match(json, regex, RegexOptions.IgnoreCase).Success
                ? JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented)
                : json;
        }

        public void LogInformation(string message)
        {
            throw new System.NotImplementedException();
        }

        public void LogDebug(string message)
        {
            throw new System.NotImplementedException();
        }

        public void LogError(string message)
        {
            throw new System.NotImplementedException();
        }
    }
}
