using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace WordGame.API.Attributes
{
	public class ReturnsStatusAttribute : ProducesResponseTypeAttribute
	{
		public ReturnsStatusAttribute(HttpStatusCode statusCode)
			: base(typeof(ApiResponse), (int)statusCode) { }
	}
}
