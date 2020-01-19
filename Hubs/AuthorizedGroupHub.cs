using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordGame.API.Application.Authorization;

namespace WordGame.API.Hubs
{
	[UserAuthorize]
	public abstract class AuthorizedGroupHub<TClient> : Hub<TClient>
		where TClient : class
	{
		protected abstract string GroupName { get; }

		public override async Task OnConnectedAsync()
		{
			await Groups.AddToGroupAsync(Context.ConnectionId, GroupName);
			await base.OnConnectedAsync();
		}

		public override async Task OnDisconnectedAsync(Exception exception)
		{
			await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName);
			await base.OnDisconnectedAsync(exception);
		}
	}
}
