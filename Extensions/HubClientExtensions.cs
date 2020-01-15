using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordGame.API.Domain.Models;

namespace WordGame.API.Extensions
{
	public static class HubClientExtensions
	{
		public static T Players<T>(this IHubClients<T> clients, IEnumerable<Guid> playerIds)
		{
			return clients.Users(playerIds.Select(x => x.ToString()).ToList());
		}

		public static T Players<T>(this IHubClients<T> clients, IEnumerable<Player> players)
		{
			return clients.Players(players.Where(p => !p.IsBot).Select(p => p.Id));
		}
	}
}
