using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CarModelManagement.Middlewares
{
	public class ExceptionHandlingMiddleware
	{

		private readonly RequestDelegate _next;
		private readonly ILogger<ExceptionHandlingMiddleware> _logger;

		public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
		{
			_next = next;
			_logger = logger;
		}

		public async Task Invoke(HttpContext context)
		{
			try
			{
				await _next(context);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An unhandled exception occurred.");
				await HandleExceptionAsync(context, ex);
			}
		}

		private static Task HandleExceptionAsync(HttpContext context, Exception exception)
		{
			var response = context.Response;
			response.ContentType = "application/json";
			response.StatusCode = (int)HttpStatusCode.InternalServerError;

			var errorResponse = new
			{
				StatusCode = response.StatusCode,
				Message = "An unexpected error occurred. Please try again later.",
				Detailed = exception.Message // Remove in production for security reasons
			};

			var jsonResponse = JsonSerializer.Serialize(errorResponse);
			return response.WriteAsync(jsonResponse);
		}
	}
}
