using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using WordGame.API.Application.Exceptions;

namespace WordGame.API.Middleware
{
	public class HttpExceptionMiddleware
	{
		private readonly RequestDelegate _next;

		public HttpExceptionMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			try
			{
				await _next(context);
			}
			catch (HttpException ex)
			{
				context.Response.ContentType = "application/json";
				context.Response.StatusCode = (int)ex.StatusCode;

				await context.Response.WriteAsync(
					JsonConvert.SerializeObject(new ApiResponse(ex.Message)));
			}
		}
	}
}
