using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordGame.API.Domain.Enums;

namespace WordGame.API.Application.Authorization
{
	public class UserAuthorizeAttribute : AuthorizeAttribute
	{
		public UserAuthorizeAttribute(params UserRole[] roles)
			: base()
		{
			AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme;
			Roles = string.Join(',', roles);
		}

		public UserAuthorizeAttribute()
			: base()
		{
			AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme;
		}
	}
}
