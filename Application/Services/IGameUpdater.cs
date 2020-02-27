using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using WordGame.API.Application.Configuration;
using WordGame.API.Application.Resources;
using WordGame.API.Domain;
using WordGame.API.Domain.Enums;
using WordGame.API.Domain.Models;
using WordGame.API.Domain.Repositories;
using WordGame.API.Extensions;
using WordGame.API.Hubs;

namespace WordGame.API.Application.Services
{
	public interface IGameUpdater
	{
		Task DeleteGame(Game game);

		Task UpdateGame(Game game);

		Task TryRunBackgroundGameTask(Game game);
	}

	public class GameUpdater : IGameUpdater
	{
		protected readonly IGameRepository _repository;
		protected readonly INameGenerator _nameGenerator;
		protected readonly IGameBoardGenerator _gameBoardGenerator;
		protected readonly IRandomAccessor _randomAccessor;
		protected readonly IHubContext<GameHub, IGameClient> _gameContext;
		protected readonly int _botDelay;

		public GameUpdater(
			IGameRepository repository,
			INameGenerator nameGenerator,
			IGameBoardGenerator gameBoardGenerator,
			IRandomAccessor randomAccessor,
			IHubContext<GameHub, IGameClient> gameContext,
			IOptions<BotSettings> botSettings)
		{
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
			_nameGenerator = nameGenerator ?? throw new ArgumentNullException(nameof(nameGenerator));
			_gameBoardGenerator = gameBoardGenerator ?? throw new ArgumentNullException(nameof(gameBoardGenerator));
			_randomAccessor = randomAccessor ?? throw new ArgumentNullException(nameof(randomAccessor));
			_gameContext = gameContext ?? throw new ArgumentNullException(nameof(gameContext));
			_botDelay = botSettings.Value.BotDelay;
		}

		public async Task DeleteGame(Game game)
		{
			await _repository.DeleteGame(game.Code);

			await _gameContext.Clients.Players(game.Players).GameDeleted();
		}

		public Task UpdateGame(Game game)
		{
			var tasks = new List<Task> { _repository.UpdateGame(game.Code, game) };

			tasks.AddRange(_gameContext.Clients.SendToPlayers(
					game,
					(client, model) => client.GameUpdated(model)));

			foreach (var @event in game.DispatchPublicEvents())
				tasks.Add(_gameContext.Clients.Players(game.Players).GameEvent(@event));

			foreach (var @event in game.DispatchCultistEvents())
				tasks.Add(_gameContext.Clients.Players(game.Cultists).GameEvent(@event));

			Task.Run(() => TryRunBackgroundGameTask(game));

			return Task.WhenAll(tasks);
		}

		public async Task TryRunBackgroundGameTask(Game game)
		{
			if (game.Status != GameStatus.InProgress)
				return;

			if (GetBackgroundGameTask(game) is (_, int delay))
			{
				await Task.Delay(delay);
				game = await _repository.GetGameByCode(game.Code);

				if (GetBackgroundGameTask(game) is (BackgroundGameTask backgroundTask, _))
					await backgroundTask(game);
			}
		}

		protected (BackgroundGameTask?, int?) GetBackgroundGameTask(Game game)
		{
			if (game.CurrentTurn.Status == TurnStatus.Tallying)
				return (ExecuteTallyVotesJob, 3500);

			if (game.CurrentTurn.Status == TurnStatus.Planning)
			{
				if (game.Cultists.FilterByRole(UserRole.Bot)
					.FilterByTeam(game.CurrentTurn.Team)
					.SingleOrDefault() is Player p)
					return ((game) => ExecutePlanningCultistBotJob(game, p), _botDelay);
			}

			if (game.CurrentTurn.Status == TurnStatus.PendingApproval)
			{
				if (game.Cultists.FilterByRole(UserRole.Bot)
					.FilterByTeam(game.CurrentTurn.Team.GetOpposingTeam())
					.SingleOrDefault() is Player p)
					return ((game) => ExecuteApprovingCultistBotJob(game, p), _botDelay);
			}

			if (game.CurrentTurn.Status == TurnStatus.Guessing)
			{
				var guessingPlayers = game.Researchers.Where(p => p.Team == game.CurrentTurn.Team);
				var botPlayers = guessingPlayers.FilterByRole(UserRole.Bot);

				if (game.WordTiles.FirstOrDefault(wt => wt.Votes.Any()) is WordTile tile)
				{
					if (botPlayers.FirstOrDefault(p => !tile.Votes.Select(x => x.Number).Contains(p.Number)) is Player p)
						return ((game) => ExecuteConformingResearcherBotJob(game, p), _botDelay);
				}

				if (game.CurrentTurn.EndTurnVotes.Any())
				{
					if (botPlayers.FirstOrDefault(p => !game.CurrentTurn.EndTurnVotes.Select(x => x.Number).Contains(p.Number)) is Player p)
						return ((game) => ExecuteConformingResearcherBotJob(game, p), _botDelay);
				}

				if (guessingPlayers.All(p => p.Role == UserRole.Bot))
					return ((game) => ExecuteGuessingResearcherBotJob(game, guessingPlayers.First()), _botDelay);
			}

			return (null, null);
		}

		protected delegate Task BackgroundGameTask(Game game);

		protected Task ExecuteTallyVotesJob(Game game)
		{
			game.TallyVotes();

			return UpdateGame(game);
		}

		protected async Task ExecutePlanningCultistBotJob(Game game, Player player)
		{
			string word = WordList.Words[_randomAccessor.Random.Next(0, WordList.Words.Length)];
			int number = _randomAccessor.Random.Next(1, 4);

			number = Math.Min(number, game.GetTilesRemaining(player.Team));

			game.GiveHint(player, word, number);

			await UpdateGame(game);
		}

		protected Task ExecuteApprovingCultistBotJob(Game game, Player player)
		{
			if (game.WordTiles.Any(x => x.Word.Contains(game.CurrentTurn.HintWord!, StringComparison.OrdinalIgnoreCase)))
				game.RefuseHint(player);
			else
				game.ApproveHint(player);

			return UpdateGame(game);
		}

		protected Task ExecuteConformingResearcherBotJob(Game game, Player player)
		{
			var availableTiles = game.WordTiles.Where(x => !x.IsRevealed).ToList();

			if (game.WordTiles.FirstOrDefault(wt => wt.Votes.Any()) is WordTile tile)
			{
				game.SetPlayerVote(player, tile);
			}
			else
			{
				game.VoteEndTurn(player);
			}

			return UpdateGame(game);
		}

		protected Task ExecuteGuessingResearcherBotJob(Game game, Player player)
		{
			var availableTiles = game.WordTiles.Where(x => !x.IsRevealed).ToList();

			if (_randomAccessor.Random.Next(0, 100) > 10)
			{
				//Random Guess
				var word = availableTiles[_randomAccessor.Random.Next(0, availableTiles.Count)];

				game.SetPlayerVote(player, word);
			}
			else
			{
				game.VoteEndTurn(player);
			}

			return UpdateGame(game);
		}
	}
}
