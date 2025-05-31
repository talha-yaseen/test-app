using System;
using System.Globalization;

namespace TestApp.Middleware
{
	public class MainMiddleware
	{

        private readonly RequestDelegate _next;

        public MainMiddleware(RequestDelegate next)
		{
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);
            if (context.Response.ContentType.Contains("application/json"))
            {
                Console.WriteLine("Is JSON");
            }
        }
    }

    public static class MainMiddlewareExtensions
    {
        public static IApplicationBuilder UseMainMiddleware(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<MainMiddleware>();
        }
    }
}

