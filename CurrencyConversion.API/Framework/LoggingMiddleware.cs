using Serilog;
// LoggingMiddleware.cs
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using System;
using static System.Net.WebRequestMethods;


namespace CurrencyConversion.API.Framework
{
    /// <summary>
    /// Middleware to  Log the following details for each request: 
    /// Client IP
    /// ClientId from the JWT token 
    /// HTTP Method & Target Endpoint
    /// Response Code & Response Time
    /// </summary>

    public class LoggingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<LoggingMiddleware> logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var clientIp = context.Connection.RemoteIpAddress?.ToString();
            var clientId = context.User.Claims.FirstOrDefault(c => c.Type == "clientthis.id")?.Value;
            var requestMethod = context.Request.Method;
            var requestPath = context.Request.Path;

            await this.next(context);

            stopwatch.Stop();
            var responseCode = context.Response.StatusCode;
            var responseTime = stopwatch.ElapsedMilliseconds;

            this.logger.LogInformation("Request Info: Client IP: {ClientIp}, ClientId: {ClientId}, Method: {Method}, Path: {Path}, Status: {Status}, ResponseTime: {ResponseTime}ms",
                clientIp, clientId, requestMethod, requestPath, responseCode, responseTime);
        }
    }
}
