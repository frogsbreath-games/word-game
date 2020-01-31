using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace WordGame.API.Application.Exceptions
{
	public class HttpException : Exception
	{
		public HttpException(HttpStatusCode statusCode)
			=> StatusCode = statusCode;

		public HttpException(HttpStatusCode statusCode, string message)
			: base(message)
			=> StatusCode = statusCode;

		public HttpException(HttpStatusCode statusCode, string message, Exception innerException)
			: base(message, innerException)
			=> StatusCode = statusCode;

		public HttpStatusCode StatusCode { get; }
	}
}
