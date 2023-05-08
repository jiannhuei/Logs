using Serilog;
using System;
namespace Db2.Middleware.Logging
{
    public class TextLogger : ICustomLogger
    {
        //public LogDetail Detail { get; set; }
        private string MessageTemplate { get; set; } = "\nRequest Headers: {RequestHeader}\n" +
                                                       "Request Host: {RequestHost}\n" +
                                                       "Request Protocol: {RequestProtocol}\n" +
                                                       "Request Method: {RequestMethod}\n" +
                                                       "Request Body: {RequestBody}\n" +
                                                       "Response Body: {ResponseBody}\n" +
                                                       "RemoteServerIp: {RemoteServerIp}\n" +
                                                       "Status Code: {StatusCode}\n" +
                                                       "Server Name: {ServerName}\n" +
                                                       "User: {User}\n" +
                                                       "Elapsed Time: {ElapsedMs}\n";

        public void LogDebug(string message)
        {
            Log.Debug(message);
        }

        public void LogError(string message)
        {
            Log.Error(message);
        }

        public void LogInformation(string message)
        {
            Log.Information(message);
        }

        public void LogRequest(LogDetail detail)
        {
            if (detail.LogLevel == LoggingConstants.LogLevel.Information)
            {
                Log.Information(MessageTemplate, detail.RequestHeaders, detail.RequestHost,
                    detail.RequestProtocol, detail.HttpMethod, detail.RequestBody, detail.ResponseBody,
                    detail.RemoteServerIp,
                    detail.StatusCode, detail.ServerName, detail.User, detail.ElapsedMs);
            }
            else
            {
                var exceptionMessage = new Exception();
                if (detail.Exception != null)
                {
                    MessageTemplate += $"Exception: {detail.Exception}\n";
                    exceptionMessage = detail.Exception;
                }

                Log.Error(MessageTemplate, detail.RequestHeaders, detail.RequestHost,
                    detail.RequestProtocol, detail.HttpMethod, detail.RequestBody, detail.ResponseBody,
                    detail.RemoteServerIp,
                    detail.StatusCode, detail.ServerName, detail.User, detail.ElapsedMs, exceptionMessage);
            }
        }

        //private void SetMessageTemplate()
        //{
        //    var responseBodySection =
        //        string.IsNullOrEmpty(Detail.ResponseBody) ? null : "Response Body: {ResponseBody}\n";
        //    Detail.MessageTemplate =
        //        "\nRequest Headers: {RequestHeader}\n" +
        //        "Request Host: {RequestHost}\n" +
        //        "Request Protocol: {RequestProtocol}\n" +
        //        "Request Method: {RequestMethod}\n" +
        //        "Request Body: {RequestBody}\n" +
        //        "Response Body: {ResponseBody}\n" +
        //        "Status Code: {StatusCode}\n" +
        //        "Elapsed Time: {ElapsedMs}\n";
        //}
    }
}
