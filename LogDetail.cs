using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace Db2.Middleware.Logging
{
    public class LogDetail
    {
        public LogDetail(Dictionary<string, string> requestHeaders, string httpMethod, string httpPath,
            HostString requestHost, string requestProtocol, string jwtPayload, string remoteIpAddress)
        {
            RequestHeaders = requestHeaders;
            HttpMethod = httpMethod;
            HttpPath = httpPath;
            RequestHost = requestHost;
            RequestProtocol = requestProtocol;
            ServerName = Environment.MachineName;
            User = jwtPayload;
            RemoteServerIp = remoteIpAddress;
        }

        public Dictionary<string, string> RequestHeaders { get; set; }
        public HostString RequestHost { get; set; }
        public string User { get; set; }
        public string ServerName { get; set; }
        public string HttpPath { get; set; }
        public string RequestProtocol { get; set; }
        public string HttpMethod { get; set; }
        public string RequestBody { get; set; }
        public string ResponseBody { get; set; }
        public string RemoteServerIp { get; set; }
        public int StatusCode { get; set; }
        public double ElapsedMs { get; set; }
        public string MessageTemplate { get; set; }
        public LoggingConstants.LogLevel LogLevel { get; set; }
        public Exception Exception { get; set; }
    }
}
