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
		protected IHubContext<GameHub, IGameClient> _gameContext;

		public GameController(IGameRepository repository, INameGenerator nameGenerator, IGameBoardGenerator gameBoardGenerator, IHubContext<GameHub, IGameClient> gameContext)
		{
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
			_nameGenerator = nameGenerator ?? throw new ArgumentNullException(nameof(nameGenerator));
			_gameBoardGenerator = gameBoardGenerator ?? throw new ArgumentNullException(nameof(gameBoardGenerator));
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
			if (currentGame.Data is Game game)
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
		[Creates(typeof(Game))]
		public async Task<ApiResponse<Game>> CreateGame()
		{
			if (User.Identity.IsAuthenticated)
				return BadRequest("User is already in a game.");

			var game = new Game(
				Guid.NewGuid().ToString().Substring(0, 6).ToUpper(),
				_nameGenerator.GetRandomName(),
				Team.Red,
				true);

			await _repository.AddGame(game);

			await SignInAsPlayer(game.Players.First(), game.Code);

			return Created(game.Code, game);
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
		[Returns(typeof(Game))]
		public async Task<ApiResponse<Game>> GetGame([FromRoute] string code)
		{
			Game game = await _repository.GetGameByCode(code);

			if (game is null)
				return NotFound($"Cannot find game with code: [{code}]");

			return Ok(game);
		}

		[HttpGet("current"), UserAuthorize]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[Returns(typeof(Game))]
		public Task<ApiResponse<Game>> GetCurrentGame()
			=> GetGame(User.GetGameCode());

		[HttpDelete("{code}"), UserAuthorize(UserRole.Organizer)]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[ReturnsStatus(HttpStatusCode.Accepted)]
		public async Task<ApiResponse> DeleteGame([FromRoute] string code)
		{
			Game game = await _repository.GetGameByCode(code);

			if (game is null)
				return NotFound($"Cannot find game with code: [{code}]");

			var playerId = User.GetPlayerId();

			var player = game.Players.SingleOrDefault(x => x.Id == playerId);

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

			var id = User.GetPlayerId();

			var player = game.Players.SingleOrDefault(x => x.Id == id);

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

			var id = User.GetPlayerId();

			var player = game.Players.SingleOrDefault(x => x.Id == id);

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

			if (!game.CanStart)
				return BadRequest($"Cannot start game!");

			//Red always goes first (for now)
			var board = _gameBoardGenerator.GenerateGameBoard(Team.Red);

			game.StartGame(board, Team.Red);

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
		[ProducesResponseType(typeof(ApiResponse<Game>), (int)HttpStatusCode.OK)]
		public async Task<ApiResponse<Game>> JoinGame(
			[FromRoute] string code)
		{
			if (User.Identity.IsAuthenticated)
				return BadRequest("User is already in a game.");

			Game game = await _repository.GetGameByCode(code);

			if (game is null)
				return NotFound($"Cannot find game with code: [{code}]");

			if (game.Status != GameStatus.Lobby)
				return BadRequest($"Cannot join game in status: [{game.Status}]");

			var player = game.AddNewPlayer(_nameGenerator.GetRandomName());

			await UpdateGame(game);

			await SignInAsPlayer(player, code);

			return Ok(game);
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

			var id = User.GetPlayerId();

			var player = game.Players.SingleOrDefault(x => x.Id == id);

			if (player is null)
				return NotFound($"Cannot find player in game with code: [{code}]");

			if (!player.IsSpyMaster || player.Team != game.CurrentTurn.Team)
				return BadRequest("This player cannot give a hint!");

			game.CurrentTurn.GiveHint(hintModel.HintWord, hintModel.WordCount);

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

			var id = User.GetPlayerId();

			var player = game.Players.SingleOrDefault(x => x.Id == id);

			if (player is null)
				return NotFound($"Cannot find player in game with code: [{code}]");

			if (!player.IsSpyMaster || player.Team == game.CurrentTurn.Team)
				return BadRequest("This player cannot approve a hint!");

			game.CurrentTurn.ApproveHint();

			await UpdateGame(game);

			return Accepted("Hint approved.");
		}

		[HttpPost("current/approveHint"), UserAuthorize]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[ReturnsStatus(HttpStatusCode.Accepted)]
		public Task<ApiResponse> ApproveCurrentGameHint()
			=> ApproveHint(User.GetGameCode());

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

			var id = User.GetPlayerId();

			var player = game.Players.SingleOrDefault(x => x.Id == id);

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

			var id = User.GetPlayerId();

			var player = game.Players.SingleOrDefault(x => x.Id == id);

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
			return Task.WhenAll(
				_gameContext.Clients.Players(game.Players).GameUpdated(game),
				_repository.UpdateGame(game.Code, game));
		}
	}
}
