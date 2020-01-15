using Microsoft.AspNetCore.SignalR;
using WordGame.API.Extensions;

namespace WordGame.API.Application.Services
{
	public class PlayerIdProvider : IUserIdProvider
	{
		public string GetUserId(HubConnectionContext connection)
		{
			return connection.User.GetPlayerId().ToString();
		}
	}
}
