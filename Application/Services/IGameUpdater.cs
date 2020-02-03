using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
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
	}

	public class GameUpdater : IGameUpdater
	{
		protected IGameRepository _repository;
		protected INameGenerator _nameGenerator;
		protected IGameBoardGenerator _gameBoardGenerator;
		protected IRandomAccessor _randomAccessor;
		protected IHubContext<GameHub, IGameClient> _gameContext;

		public GameUpdater(
			IGameRepository repository,
			INameGenerator nameGenerator,
			IGameBoardGenerator gameBoardGenerator,
			IRandomAccessor randomAccessor,
			IHubContext<GameHub, IGameClient> gameContext)
		{
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
			_nameGenerator = nameGenerator ?? throw new ArgumentNullException(nameof(nameGenerator));
			_gameBoardGenerator = gameBoardGenerator ?? throw new ArgumentNullException(nameof(gameBoardGenerator));
			_randomAccessor = randomAccessor ?? throw new ArgumentNullException(nameof(randomAccessor));
			_gameContext = gameContext ?? throw new ArgumentNullException(nameof(gameContext));
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

			foreach (var @event in game.DispatchSpyMasterEvents())
				tasks.Add(_gameContext.Clients.Players(game.SpyMasters).GameEvent(@event));

			Task.Run(() => BotTask(game));

			return Task.WhenAll(tasks);
		}

		protected async Task BotTask(Game game)
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

		protected (BackgroundGameTask, int?) GetBackgroundGameTask(Game game)
		{
			if (game.CurrentTurn.Status == TurnStatus.Tallying)
				return (ExecuteTallyVotesJob, 3500);

			if (game.CurrentTurn.Status == TurnStatus.Planning)
			{
				if (game.SpyMasters.SingleOrDefault(p => p.IsBot
					&& p.Team == game.CurrentTurn.Team) is Player p)
					return ((game) => ExecutePlanningSpyMasterBotJob(game, p), 2000);
			}

			if (game.CurrentTurn.Status == TurnStatus.PendingApproval)
			{
				if (game.SpyMasters.SingleOrDefault(p => p.IsBot
					&& p.Team != game.CurrentTurn.Team) is Player p)
					return ((game) => ExecuteApprovingSpyMasterBotJob(game, p), 2000);
			}

			if (game.CurrentTurn.Status == TurnStatus.Guessing)
			{
				var guessingPlayers = game.Agents.Where(p => p.Team == game.CurrentTurn.Team);
				var botPlayers = guessingPlayers.Where(p => p.IsBot);

				if (game.WordTiles.FirstOrDefault(wt => wt.Votes.Any()) is WordTile tile)
				{
					if (botPlayers.FirstOrDefault(p => !tile.Votes.Select(x => x.Number).Contains(p.Number)) is Player p)
						return ((game) => ExecuteConformingAgentBotJob(game, p), 2000);
				}

				if (game.CurrentTurn.EndTurnVotes.Any())
				{
					if (botPlayers.FirstOrDefault(p => !game.CurrentTurn.EndTurnVotes.Select(x => x.Number).Contains(p.Number)) is Player p)
						return ((game) => ExecuteConformingAgentBotJob(game, p), 2000);
				}

				if (guessingPlayers.All(p => p.IsBot))
					return ((game) => ExecuteGuessingAgentBotJob(game, guessingPlayers.First()), 2000);
			}

			return (null, null);
		}

		protected delegate Task BackgroundGameTask(Game game);

		protected Task ExecuteTallyVotesJob(Game game)
		{
			game.TallyVotes();

			return UpdateGame(game);
		}

		protected async Task ExecutePlanningSpyMasterBotJob(Game game, Player player)
		{
			string word = WordList.Words[_randomAccessor.Random.Next(0, WordList.Words.Length)];
			int number = _randomAccessor.Random.Next(1, 4);

			game.GiveHint(player, word, number);

			await UpdateGame(game);
		}

		protected Task ExecuteApprovingSpyMasterBotJob(Game game, Player player)
		{
			if (game.WordTiles.Any(x => x.Word.Contains(game.CurrentTurn.HintWord, StringComparison.OrdinalIgnoreCase)))
				game.RefuseHint(player);
			else
				game.ApproveHint(player);

			return UpdateGame(game);
		}

		protected Task ExecuteConformingAgentBotJob(Game game, Player player)
		{
			var availableTiles = game.WordTiles.Where(x => !x.IsRevealed).ToList();

			if (game.WordTiles.FirstOrDefault(wt => wt.Votes.Any()) is WordTile tile)
			{
				game.SetPlayerVote(player, tile.Word);
			}
			else
			{
				game.VoteEndTurn(player);
			}

			return UpdateGame(game);
		}

		protected Task ExecuteGuessingAgentBotJob(Game game, Player player)
		{
			var availableTiles = game.WordTiles.Where(x => !x.IsRevealed).ToList();

			if (_randomAccessor.Random.Next(0, 100) > 10)
			{
				//Random Guess
				string word = availableTiles[_randomAccessor.Random.Next(0, availableTiles.Count)].Word;

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
