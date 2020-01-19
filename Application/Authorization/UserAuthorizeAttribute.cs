﻿using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WordGame.API.Application.Authorization
{
	public class UserAuthorizeAttribute : AuthorizeAttribute
	{
		public UserAuthorizeAttribute(UserRole role)
			: base()
		{
			AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme;
			Roles = role.ToString();
		}

		public UserAuthorizeAttribute()
			: base()
		{
			AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme;
		}
	}
}