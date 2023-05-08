using Db2.Exceptions;
using Db2.Middleware.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Db2.Middleware.RequestHandling
{
    public class RequestHandlingMiddleware
    {
        private static readonly HashSet<string> HeaderWhitelist = new HashSet<string> { "Content-Type", "Content-Length", "User-Agent", "Authorization", "X-Forwarded-For" };

        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        private LogDetail _logDetail;
        private string _requestBody;

        public RequestHandlingMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task Invoke(HttpContext httpContext, ICustomLogger logger)
        {
            var startTime = Stopwatch.GetTimestamp();
            var originalBodyStream = httpContext.Response.Body;
            try
            {
                var httpPath = GetPath(httpContext);
                if (!httpPath.Contains("swagger"))
                {
                    InstantiateLogDetail(httpContext);
                    _requestBody = _logDetail.RequestBody = _configuration.GetSection("CustomLogging")["EnableLogRequest"] == "1" ? await GetRequestBody(httpContext) : "Disable Request Log";

                    await using var memStream = new MemoryStream();
                    httpContext.Response.Body = memStream;

                    await _next(httpContext);

                    memStream.Position = 0;
                    _logDetail.ResponseBody = _configuration.GetSection("CustomLogging")["EnableLogResponse"] == "1" ? await new StreamReader(memStream).ReadToEndAsync() : "Disable Response Log";

                    memStream.Position = 0;
                    await memStream.CopyToAsync(originalBodyStream);
                    httpContext.Response.Body = originalBodyStream;
                }
            }
            catch (Exception e)
            {
                _logDetail.LogLevel = LoggingConstants.LogLevel.Error;
                _logDetail.Exception = e;
                httpContext.Response.Body = originalBodyStream;
                await HandleException(httpContext, e);
            }
            finally
            {
                if (_logDetail != null)
                {
                    _logDetail.StatusCode = httpContext.Response.StatusCode;
                    _logDetail.ElapsedMs = GetElapsedMilliseconds(startTime, Stopwatch.GetTimestamp());
                    logger.LogRequest(_logDetail);
                }
            }
        }

        private Task HandleException(HttpContext httpContext, Exception e)
        {
            if (e is NotFoundException)
                httpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
            else if (e is BadRequestException)
                httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            else
                httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var errorResponse = JsonConvert.SerializeObject(new
            {
                requestUrl = httpContext.Request.PathBase + httpContext.Request.Path,
                method = httpContext.Request.Method,
                requestBody = _requestBody,
                error = e.ToString()
            });

            _logDetail.ResponseBody = errorResponse;
            httpContext.Response.ContentType = "application/json";
            return httpContext.Response.WriteAsync(errorResponse);
        }


        private static async Task<string> GetRequestBody(HttpContext httpContext)
        {
            // Getting the request body is a little tricky because it's a stream
            // So, we need to read the stream and then rewind it back to the beginning
            httpContext.Request.EnableBuffering();
            var body = httpContext.Request.Body;
            var buffer = new byte[Convert.ToInt32(httpContext.Request.ContentLength)];
            await httpContext.Request.Body.ReadAsync(buffer.AsMemory(0, buffer.Length));
            var requestBody = Encoding.UTF8.GetString(buffer);
            body.Seek(0, SeekOrigin.Begin);
            httpContext.Request.Body = body;

            return requestBody;
        }

        private void InstantiateLogDetail(HttpContext httpContext)
        {
            _logDetail = new LogDetail(
                GetRequestHeader(httpContext.Request.Headers),
                httpContext.Request.Method,
                GetPath(httpContext),
                httpContext.Request.Host,
                httpContext.Request.Protocol,
                "",
                httpContext.Connection.RemoteIpAddress?.ToString() ?? "");
        }

        private static Dictionary<string, string> GetRequestHeader(IHeaderDictionary headers)
        {
            return headers
                .Where(h => HeaderWhitelist.Contains(h.Key))
                .ToDictionary(h => h.Key, h => h.Value.ToString());
        }

        private static double GetElapsedMilliseconds(long start, long stop)
        {
            return (stop - start) * 1000 / (double)Stopwatch.Frequency;
        }

        private static string GetPath(HttpContext httpContext)
        {
            return httpContext.Features.Get<IHttpRequestFeature>()?.RawTarget ?? httpContext.Request.Path.ToString();
        }
    }
}
