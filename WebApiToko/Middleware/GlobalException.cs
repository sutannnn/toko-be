using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using WebApiToko.Data;
using WebApiToko.Dtos;
//using WebApiToko.Helper;
using WebApiToko.Interfaces.Services;

namespace WebApiToko.Middleware
{
    public class GlobalException : IExceptionHandler
    {
        private readonly ILogger<GlobalException> _logger;
        //private readonly IElasticLoggingService _elasticLoggingService;
        private readonly IConfiguration _configuration;

        public GlobalException(ILogger<GlobalException> logger, IConfiguration configuration)
        {
            _logger = logger;
            //_elasticLoggingService = elasticLoggingService;
            _configuration = configuration;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception ex, CancellationToken cancellationToken)
        {
            // Determine the HTTP status code based on the exception type
            var statusCode = ex switch
            {
                BadHttpRequestException => (int)StatusCodes.Status400BadRequest, // 400
                UnauthorizedAccessException => (int)StatusCodes.Status401Unauthorized, // 401
                KeyNotFoundException => (int)StatusCodes.Status404NotFound, // 404
                TimeoutException => (int)StatusCodes.Status408RequestTimeout, // 408
                _ => (int)StatusCodes.Status500InternalServerError // 500
            };

            // Set the response status code and content type
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            // Customize the error message based on the exception type
            var errorMessage = ex switch
            {
                BadHttpRequestException badRequestEx => badRequestEx.Message,
                UnauthorizedAccessException unauthorizedEx => "Access is denied.",
                _ => ex.Message ?? "Terjadi kesalahan tak terduga."
            };

            // Build and send log to Elasticsearch
            //var errorLog = await ExceptionLogBuilder.BuildAsync(context, ex);
            //errorLog.StatusMessage = ex.Message;

            // Insert log to Elasticsearch
            //InsertLogToElastic(errorLog);

            // Write the error response as JSON
            await context.Response.WriteAsJsonAsync(new ApiResponseError
            {
                statusCode = statusCode.ToString(),
                statusDesc = errorMessage
            });

            return true; // Indicate that the exception has been handled
        }

        //private void InsertLogToElastic(ExceptionLogDto errorLog)
        //{
        //    string isEnable = _configuration.GetSection("Elasticsearch")["Uri"];
        //    if (isEnable == null)
        //    {
        //        return;
        //    }

        //    _ = Task.Run(async () =>
        //    {
        //        try
        //        {
        //            await _elasticLoggingService.IndexErrorLogAsync(errorLog);
        //        }
        //        catch (Exception)
        //        {
        //            // Log to a local file or ignore
        //        }
        //    });
        //}
    }
}