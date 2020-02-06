using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordGame.API.Domain.Enums;
using WordGame.API.Domain.Models;
using WordGame.API.Models;

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
			return clients.Players(players.Where(p => p.Role != UserRole.Bot).Select(p => p.Id));
		}

		public static IEnumerable<Task> SendToPlayers<T>(this IHubClients<T> clients, Game game, Func<T, GameModel, Task> func)
		{
			return game.Players
				.Where(p => p.Role != UserRole.Bot)
				.Select(p => p.Id)
				.Select(id => func(
					clients.User(id.ToString()),
					new GameModel(game, id)));
		}

		public static IEnumerable<Task> SendToPlayers<T>(this IHubClients<T> clients, Game game, Func<T, Task> func)
		{
			return game.Players
				.Where(p => p.Role != UserRole.Bot)
				.Select(p => p.Id)
				.Select(id => func(
					clients.User(id.ToString())));
		}
	}
}
