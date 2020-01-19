using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;

namespace WordGame.API.Attributes
{
	public class CreatesAttribute : ProducesResponseTypeAttribute
	{
		public CreatesAttribute(Type type)
			: base(typeof(ApiResponse<>).MakeGenericType(type), (int)HttpStatusCode.Created) { }
	}
}
