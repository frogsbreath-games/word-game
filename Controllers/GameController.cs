using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using WordGame.API.Application.Authorization;
using WordGame.API.Application.Resources;
using WordGame.API.Application.Services;
using WordGame.API.Attributes;
using WordGame.API.Domain.Enums;
using WordGame.API.Domain.Models;
using WordGame.API.Domain.Repositories;
using WordGame.API.Extensions;
using WordGame.API.Hubs;
using WordGame.API.Models;

namespace WordGame.API.Controllers
{
	[ApiController]
	[Route("api/games")]
	[Produces("application/json"), Consumes("application/json")]
	public class GameController : ControllerBase
	{
		protected IGameRepository _repository;
		protected INameGenerator _nameGenerator;
		protected IGameBoardGenerator _gameBoardGenerator;
		protected IRandomAccessor _randomAccessor;
		protected IHubContext<GameHub, IGameClient> _gameContext;

		public GameController(
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

		protected ApiResponse NotFound(string message)
		{
			Response.StatusCode = (int)HttpStatusCode.NotFound;

			return new ApiResponse(message);
		}

		protected ApiResponse Forbidden(string message)
		{
			Response.StatusCode = (int)HttpStatusCode.Forbidden;

			return new ApiResponse(message);
		}

		protected ApiResponse BadRequest(string message)
		{
			Response.StatusCode = (int)HttpStatusCode.BadRequest;

			return new ApiResponse(message);
		}

		protected new ApiResponse Accepted(string message)
		{
			Response.StatusCode = (int)HttpStatusCode.Accepted;

			return new ApiResponse(message);
		}

		protected ApiResponse<T> Created<T>(string pathPart, T obj)
		{
			Response.StatusCode = (int)HttpStatusCode.Created;
			Response.Headers["Location"] = $"{Request.Path}/{pathPart}";

			return obj;
		}

		protected ApiResponse<T> Ok<T>(T obj)
		{
			Response.StatusCode = (int)HttpStatusCode.OK;

			return obj;
		}

		//This should only be used if your game got deleted without you somehow.
		[HttpPost("forceSignOut"), UserAuthorize]
		public async Task<ApiResponse> ForceSignOut()
		{
			var currentGame = await GetCurrentGame();
			if (currentGame.Data is GameModel game)
			{
				if (User.IsInRole("Organizer"))
				{
					await DeleteGame(game.Code);
				}
				else
				{
					await QuitGame(game.Code);
				}
			}
			else
			{
				await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			}

			Response.StatusCode = (int)HttpStatusCode.OK;
			return new ApiResponse("logged out.");
		}

		[HttpPost]
		[Creates(typeof(GameModel))]
		public async Task<ApiResponse<GameModel>> CreateGame()
		{
			if (User.Identity.IsAuthenticated)
				return BadRequest("User is already in a game.");

			var game = new Game(
				Guid.NewGuid().ToString().Substring(0, 6).ToUpper(),
				_nameGenerator.GetRandomName(),
				Team.Red,
				true);

			await _repository.AddGame(game);

			var player = game.Players.First();

			await SignInAsPlayer(player, game.Code);

			return Created(game.Code, new GameModel(game, player.Id));
		}

		[HttpGet, UserAuthorize]
		[Returns(typeof(List<Game>))]
		public async Task<ApiResponse<List<Game>>> GetAll(
			[FromQuery] int skip = 0,
			[FromQuery] int take = 100)
		{
			return Ok(await _repository.GetGames(skip, take));
		}

		[HttpGet("{code}"), UserAuthorize]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[Returns(typeof(GameModel))]
		public async Task<ApiResponse<GameModel>> GetGame([FromRoute] string code)
		{
			Game game = await _repository.GetGameByCode(code);

			if (game is null)
				return NotFound($"Cannot find game with code: [{code}]");

			var player = User.GetPlayer(game);

			if (player is null)
				return BadRequest("Cannot get game player is not in.");

			return Ok(new GameModel(game, player));
		}

		[HttpGet("current"), UserAuthorize]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[Returns(typeof(GameModel))]
		public Task<ApiResponse<GameModel>> GetCurrentGame()
			=> GetGame(User.GetGameCode());

		[HttpDelete("{code}"), UserAuthorize(UserRole.Organizer)]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[ReturnsStatus(HttpStatusCode.Accepted)]
		public async Task<ApiResponse> DeleteGame([FromRoute] string code)
		{
			Game game = await _repository.GetGameByCode(code);

			if (game is null)
				return NotFound($"Cannot find game with code: [{code}]");

			var player = User.GetPlayer(game);

			if (player is null)
				return BadRequest("Cannot delete game player is not in.");

			await _repository.DeleteGame(code);

			await _gameContext.Clients.Players(game.Players).GameDeleted();

			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

			return Accepted("Game deleted");
		}

		[HttpDelete("current"), UserAuthorize(UserRole.Organizer)]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[ReturnsStatus(HttpStatusCode.Accepted)]
		public Task<ApiResponse> DeleteCurrentGame()
			=> DeleteGame(User.GetGameCode());

		[HttpGet("{code}/players"), UserAuthorize]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[Returns(typeof(List<Player>))]
		public async Task<ApiResponse<List<Player>>> GetGamePlayers(
			[FromRoute] string code,
			[FromQuery] Team? team = null,
			[FromQuery] bool? isSpyMaster = null)
		{
			Game game = await _repository.GetGameByCode(code);

			if (game is null)
				return NotFound($"Cannot find game with code: [{code}]");

			IEnumerable<Player> players = game.Players;

			if (team.HasValue)
				players = players.Where(p => p.Team == team.Value);

			if (isSpyMaster.HasValue)
				players = players.Where(p => p.IsSpyMaster == isSpyMaster.Value);

			return Ok(players.ToList());
		}

		[HttpGet("current/players"), UserAuthorize]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[Returns(typeof(List<Player>))]
		public Task<ApiResponse<List<Player>>> GetCurrentGamePlayers(
			[FromQuery] Team? team = null,
			[FromQuery] bool? isSpyMaster = null)
			=> GetGamePlayers(
				User.GetGameCode(),
				team,
				isSpyMaster);

		[HttpGet("{code}/players/self"), UserAuthorize]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[Returns(typeof(Player))]
		public async Task<ApiResponse<Player>> GetSelfGamePlayer(
			[FromRoute] string code)
		{
			Game game = await _repository.GetGameByCode(code);

			if (game is null)
				return NotFound($"Cannot find game with code: [{code}]");

			var player = User.GetPlayer(game);

			if (player is null)
				return NotFound($"Current user is not a player in game with code: [{code}]");

			return Ok(player);
		}

		[HttpGet("current/players/self"), UserAuthorize]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[Returns(typeof(Player))]
		public Task<ApiResponse<Player>> GetCurrentGameSelf()
			=> GetSelfGamePlayer(User.GetGameCode());

		[HttpPost("{code}/quit"), UserAuthorize(UserRole.Player)]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.OK)]
		public async Task<ApiResponse> QuitGame(
			[FromRoute] string code)
		{
			Game game = await _repository.GetGameByCode(code);

			if (game is null)
				return NotFound($"Cannot find game with code: [{code}]");

			var player = User.GetPlayer(game);

			if (player is null)
				return BadRequest("Cannot quit game player is not in.");

			game.Players.Remove(player);

			await UpdateGame(game);

			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

			return new ApiResponse("Disconnected");
		}

		[HttpPost("current/quit"), UserAuthorize(UserRole.Player)]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.OK)]
		public Task<ApiResponse> QuitCurrentGame()
			=> QuitGame(User.GetGameCode());

		[HttpPost("{code}/start"), UserAuthorize(UserRole.Organizer)]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[ReturnsStatus(HttpStatusCode.Accepted)]
		public async Task<ApiResponse> StartGame(
			[FromRoute] string code)
		{
			Game game = await _repository.GetGameByCode(code);

			if (game is null)
				return NotFound($"Cannot find game with code: [{code}]");

			if (!game.GameCanStart())
				return BadRequest($"Cannot start game!");

			var player = User.GetPlayer(game);

			if (player is null)
				return NotFound($"Cannot find player in game with code: [{code}]");

			//Red always goes first (for now)
			var board = _gameBoardGenerator.GenerateGameBoard(Team.Red);

			game.StartGame(player, board, Team.Red);

			await UpdateGame(game);

			return Accepted("Game Started");
		}

		[HttpPost("current/start"), UserAuthorize(UserRole.Organizer)]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[ReturnsStatus(HttpStatusCode.Accepted)]
		public Task<ApiResponse> StartCurrentGame()
			=> StartGame(User.GetGameCode());

		[HttpPost("{code}/join")]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[ProducesResponseType(typeof(ApiResponse<GameModel>), (int)HttpStatusCode.OK)]
		public async Task<ApiResponse<GameModel>> JoinGame(
			[FromRoute] string code)
		{
			if (User.Identity.IsAuthenticated)
				return BadRequest("User is already in a game.");

			Game game = await _repository.GetGameByCode(code);

			if (game is null)
				return NotFound($"Cannot find game with code: [{code}]");

			if (game.Status != GameStatus.Lobby)
				return BadRequest($"Cannot join game in status: [{game.Status}]");

			if (game.Players.Count >= 10)
				return BadRequest("Game cannot have more than 10 players");

			var player = game.AddNewPlayer(_nameGenerator.GetRandomName());

			await UpdateGame(game);

			await SignInAsPlayer(player, code);

			return Ok(new GameModel(game, player.Id));
		}

		[HttpPost("{code}/players"), UserAuthorize(UserRole.Organizer)]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[ReturnsStatus(HttpStatusCode.Accepted)]
		public async Task<ApiResponse> AddGameBot(
			[FromRoute] string code)
		{
			Game game = await _repository.GetGameByCode(code);

			if (game is null)
				return NotFound($"Cannot find game with code: [{code}]");

			if (game.Status != GameStatus.Lobby)
				return BadRequest($"Cannot add bot to game in status: [{game.Status}]");

			if (game.Players.Count >= 10)
				return BadRequest("$Game cannot have more than 10 players.");

			game.AddNewPlayer(_nameGenerator.GetRandomName(), isBot: true);

			await UpdateGame(game);

			return Accepted("Bot Added.");
		}

		[HttpPost("current/players"), UserAuthorize(UserRole.Organizer)]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[ReturnsStatus(HttpStatusCode.Accepted)]
		public Task<ApiResponse> AddCurrentGameBot()
			=> AddGameBot(User.GetGameCode());

		[HttpGet("{code}/players/{number}"), UserAuthorize]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[Returns(typeof(Player))]
		public async Task<ApiResponse<Player>> GetGamePlayer(
			[FromRoute] string code,
			[FromRoute] int number)
		{
			Game game = await _repository.GetGameByCode(code);

			if (game is null)
				return NotFound($"Cannot find game with code: [{code}]");

			Player player = game.Players.SingleOrDefault(x => x.Number == number);

			if (player is null)
				return NotFound($"Cannot find player with number: [{number}] in game with code: [{code}]");

			return Ok(player);
		}

		[HttpGet("current/players/{number}"), UserAuthorize]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[Returns(typeof(Player))]
		public Task<ApiResponse<Player>> GetCurrentGamePlayer(
			[FromRoute] int number)
			=> GetGamePlayer(User.GetGameCode(), number);

		[HttpDelete("{code}/players/{number}"), UserAuthorize(UserRole.Organizer)]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[ReturnsStatus(HttpStatusCode.Accepted)]
		public async Task<ApiResponse> DeleteGameBot(
			[FromRoute] string code,
			[FromRoute] int number)
		{
			Game game = await _repository.GetGameByCode(code);

			if (game is null)
				return NotFound($"Cannot find game with code: [{code}]");

			if (game.Status != GameStatus.Lobby)
				return BadRequest($"Cannot delete bot from game in status: [{game.Status}]");

			Player player = game.Players.SingleOrDefault(x => x.Number == number);

			if (player is null || !player.IsBot)
				return NotFound($"Cannot find bot with number: [{number}] in game with code: [{code}]");

			game.Players.Remove(player);

			await UpdateGame(game);

			return Accepted("Bot Deleted");
		}

		[HttpDelete("current/players/{number}"), UserAuthorize(UserRole.Organizer)]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[ReturnsStatus(HttpStatusCode.Accepted)]
		public Task<ApiResponse> DeleteCurrentGameBot(
			[FromRoute] int number)
			=> DeleteGameBot(User.GetGameCode(), number);

		[HttpPost("{code}/players/{number}"), UserAuthorize]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[ReturnsStatus(HttpStatusCode.Accepted)]
		public async Task<ApiResponse> UpdatePlayer(
			[FromRoute] string code,
			[FromRoute] int number,
			[FromBody] PlayerModel playerModel)
		{
			Game game = await _repository.GetGameByCode(code);

			if (game is null)
				return NotFound($"Cannot find game with code: [{code}]");

			Player player = game.Players.SingleOrDefault(x => x.Number == number);

			if (player is null)
				return NotFound($"Cannot find player with number: [{number}] in game with code: [{code}]");

			player.UpdatePlayer(
				playerModel.Team,
				playerModel.Name,
				playerModel.IsSpyMaster);

			await UpdateGame(game);

			return Accepted("Player Updated.");
		}

		[HttpPost("current/players/{number}"), UserAuthorize]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[ReturnsStatus(HttpStatusCode.Accepted)]
		public Task<ApiResponse> UpdatePlayerInCurrentGame(
			[FromRoute] int number,
			[FromBody] PlayerModel playerModel)
			=> UpdatePlayer(User.GetGameCode(), number, playerModel);

		[HttpPost("{code}/giveHint"), UserAuthorize]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[ReturnsStatus(HttpStatusCode.Accepted)]
		public async Task<ApiResponse> GiveHint(
			[FromRoute] string code,
			[FromBody] HintModel hintModel)
		{
			Game game = await _repository.GetGameByCode(code);

			if (game is null)
				return NotFound($"Cannot find game with code: [{code}]");

			if (game.Status != GameStatus.InProgress)
				return BadRequest($"Cannot give hint in game that isn't in progress.");

			if (game.CurrentTurn.Status != TurnStatus.Planning)
				return BadRequest($"Cannot give hint outside the planning stage of the current turn.");

			var player = User.GetPlayer(game);

			if (player is null)
				return NotFound($"Cannot find player in game with code: [{code}]");

			if (!player.IsSpyMaster || player.Team != game.CurrentTurn.Team)
				return BadRequest("This player cannot give a hint!");

			game.GiveHint(player, hintModel.HintWord, hintModel.WordCount);

			await UpdateGame(game);

			return Accepted("Hint submitted.");
		}

		[HttpPost("current/giveHint"), UserAuthorize]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[ReturnsStatus(HttpStatusCode.Accepted)]
		public Task<ApiResponse> GiveCurrentGameHint(
			[FromBody] HintModel hintModel)
			=> GiveHint(User.GetGameCode(), hintModel);

		[HttpPost("{code}/approveHint"), UserAuthorize]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[ReturnsStatus(HttpStatusCode.Accepted)]
		public async Task<ApiResponse> ApproveHint(
			[FromRoute] string code)
		{
			Game game = await _repository.GetGameByCode(code);

			if (game is null)
				return NotFound($"Cannot find game with code: [{code}]");

			if (game.Status != GameStatus.InProgress)
				return BadRequest($"Cannot approve hint in game that isn't in progress.");

			if (game.CurrentTurn.Status != TurnStatus.PendingApproval)
				return BadRequest($"Cannot approve hint outside the pending approval stage of the current turn.");

			var player = User.GetPlayer(game);

			if (player is null)
				return NotFound($"Cannot find player in game with code: [{code}]");

			if (!player.IsSpyMaster || player.Team == game.CurrentTurn.Team)
				return BadRequest("This player cannot approve a hint!");

			game.ApproveHint(player);

			await UpdateGame(game);

			return Accepted("Hint approved.");
		}

		[HttpPost("current/approveHint"), UserAuthorize]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[ReturnsStatus(HttpStatusCode.Accepted)]
		public Task<ApiResponse> ApproveCurrentGameHint()
			=> ApproveHint(User.GetGameCode());

		[HttpPost("{code}/refuseHint"), UserAuthorize]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[ReturnsStatus(HttpStatusCode.Accepted)]
		public async Task<ApiResponse> RefuseHint(
				[FromRoute] string code)
		{
			Game game = await _repository.GetGameByCode(code);

			if (game is null)
				return NotFound($"Cannot find game with code: [{code}]");

			if (game.Status != GameStatus.InProgress)
				return BadRequest($"Cannot refuse hint in game that isn't in progress.");

			if (game.CurrentTurn.Status != TurnStatus.PendingApproval)
				return BadRequest($"Cannot refuse hint outside the pending approval stage of the current turn.");

			var player = User.GetPlayer(game);

			if (player is null)
				return NotFound($"Cannot find player in game with code: [{code}]");

			if (!player.IsSpyMaster || player.Team == game.CurrentTurn.Team)
				return BadRequest("This player cannot approve a hint!");

			game.CurrentTurn.RefuseHint();

			await UpdateGame(game);

			return Accepted("Hint refused.");
		}

		[HttpPost("current/refuseHint"), UserAuthorize]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[ReturnsStatus(HttpStatusCode.Accepted)]
		public Task<ApiResponse> RefustCurrentGameHint()
			=> RefuseHint(User.GetGameCode());


		[HttpPost("{code}/voteWord"), UserAuthorize]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[ReturnsStatus(HttpStatusCode.Accepted)]
		public async Task<ApiResponse> VoteWord(
			[FromRoute] string code,
			[FromBody] VoteModel voteModel)
		{
			Game game = await _repository.GetGameByCode(code);

			if (game is null)
				return NotFound($"Cannot find game with code: [{code}]");

			if (game.Status != GameStatus.InProgress)
				return BadRequest($"Cannot cast word vote in game that isn't in progress.");

			if (game.CurrentTurn.Status != TurnStatus.Guessing)
				return BadRequest($"Cannot cast word vote outside the guessing stage of the current turn.");

			var player = User.GetPlayer(game);

			if (player is null)
				return NotFound($"Cannot find player in game with code: [{code}]");

			if (player.IsSpyMaster || player.Team != game.CurrentTurn.Team)
				return BadRequest("This player cannot vote for a word!");

			game.SetPlayerVote(player, voteModel.Word);

			await UpdateGame(game);

			return Accepted("Player Word Vote Set.");
		}

		[HttpPost("current/voteWord"), UserAuthorize]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[ReturnsStatus(HttpStatusCode.Accepted)]
		public Task<ApiResponse> CurrentGameVoteWord(
			[FromBody] VoteModel voteModel)
			=> VoteWord(User.GetGameCode(), voteModel);

		[HttpPost("{code}/voteEndTurn"), UserAuthorize]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[ReturnsStatus(HttpStatusCode.Accepted)]
		public async Task<ApiResponse> VoteEndTurn(
			[FromRoute] string code)
		{
			Game game = await _repository.GetGameByCode(code);

			if (game is null)
				return NotFound($"Cannot find game with code: [{code}]");

			if (game.Status != GameStatus.InProgress)
				return BadRequest($"Cannot vote end turn in game that isn't in progress.");

			if (game.CurrentTurn.Status != TurnStatus.Guessing)
				return BadRequest($"Cannot vote end turn outside the guessing stage of the current turn.");

			var player = User.GetPlayer(game);

			if (player is null)
				return NotFound($"Cannot find player in game with code: [{code}]");

			if (player.IsSpyMaster || player.Team != game.CurrentTurn.Team)
				return BadRequest("This player cannot vote to end turn!");

			game.VoteEndTurn(player);

			await UpdateGame(game);

			return Accepted("Player voted to end turn.");
		}

		[HttpPost("current/voteEndTurn"), UserAuthorize]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[ReturnsStatus(HttpStatusCode.Accepted)]
		public Task<ApiResponse> CurrentGameVoteEndTurn()
			=> VoteEndTurn(User.GetGameCode());

		protected Task SignInAsPlayer(Player player, string code)
		{
			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, player.Id.ToString()),
				new Claim(ClaimTypes.Role, player.IsOrganizer ? nameof(UserRole.Organizer) : nameof(UserRole.Player)),
				new Claim(ClaimTypes.Name, player.Name),
				new Claim("Game", code)
			};

			return HttpContext.SignInAsync(
				CookieAuthenticationDefaults.AuthenticationScheme,
				new ClaimsPrincipal(new ClaimsIdentity(
					claims,
					CookieAuthenticationDefaults.AuthenticationScheme)),
				new AuthenticationProperties
				{
					IsPersistent = true
				});
		}

		protected Task UpdateGame(Game game)
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
