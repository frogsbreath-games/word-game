﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WordGame.API.Extensions
{
	public static class ClaimsPrincipalExtensions
	{
		public static Guid GetPlayerId(this ClaimsPrincipal principal)
		{
			return Guid.Parse(principal.Claims.SingleOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value);
		}

		public static string GetGameCode(this ClaimsPrincipal principal)
		{
			return principal.Claims.SingleOrDefault(x => x.Type == "Game").Value;
		}

	}
}
