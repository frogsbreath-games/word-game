using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordGame.API.Domain.Models;
using WordGame.API.Extensions;

namespace WordGame.API.Hubs
{
	[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
	public class LobbyHub : Hub<ILobbyClient>
	{
		protected string GroupName => $"{Context.User.GetGameCode()}-lobby";

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

		public Task SendMessage(string message)
		{
			return Clients.Group(GroupName).MessageSent(
				Context.User.Identity.Name, message);
		}
	}

	public interface ILobbyClient
	{
		Task MessageSent(string sender, string message);

		Task PlayerAdded(Player newPlayer);

		Task PlayerUpdated(Player updatedPlayer);

		Task PlayerLeft(Player leavingPlayer);

		Task GameStarted();

		Task GameDeleted();
	}
}
