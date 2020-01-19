using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;

namespace WordGame.API.Attributes
{
	public class ReturnsAttribute : ProducesResponseTypeAttribute
	{
		public ReturnsAttribute(Type type)
			: base(typeof(ApiResponse<>).MakeGenericType(type), (int)HttpStatusCode.OK) { }
	}
}
