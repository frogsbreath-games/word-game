using System.Threading.Tasks;
using WordGame.API.Domain.Models;
using WordGame.API.Extensions;
using WordGame.API.Models;
using WordGame.API.Domain.Enums;
using WordGame.API.Domain.Repositories;
using System.Linq;
using System;

namespace WordGame.API.Hubs
{
	public class GameHub : AuthorizedGroupHub<IGameClient>
	{
		protected readonly IGameRepository _gameRepository;

		public GameHub(IGameRepository gameRepository)
		{
			_gameRepository = gameRepository;
		}

		protected override string GroupName => Context.User.GetGameCode();

		public async Task SendMessage(string message)
		{
			var game = await _gameRepository.GetGameByCode(Context.User.GetGameCode());

			await Clients.Group(GroupName).GameEvent(
				GameEvent.PlayerMessage(Context.User.GetPlayer(game),
				DateTime.Now,
				message));
		}
	}

	public interface IGameClient
	{
		Task GameDeleted();

		Task GameUpdated(GameModel gameModel);

		Task GameEvent(GameEvent gameEvent);
	}
}
