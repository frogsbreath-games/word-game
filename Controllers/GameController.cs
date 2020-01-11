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
using WordGame.API.Application.Services;
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
		protected IHubContext<LobbyHub, ILobbyClient> _lobbyContext;

		public GameController(IGameRepository repository, INameGenerator nameGenerator, IGameBoardGenerator gameBoardGenerator, IHubContext<LobbyHub, ILobbyClient> lobbyContext)
		{
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
			_nameGenerator = nameGenerator ?? throw new ArgumentNullException(nameof(nameGenerator));
			_gameBoardGenerator = gameBoardGenerator ?? throw new ArgumentNullException(nameof(gameBoardGenerator));
			_lobbyContext = lobbyContext ?? throw new ArgumentNullException(nameof(lobbyContext));
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
		[HttpPost("forceSignOut")]
		[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
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
		[ProducesResponseType(typeof(ApiResponse<Game>), (int)HttpStatusCode.Created)]
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

		[HttpGet]
		[ProducesResponseType(typeof(ApiResponse<List<Game>>), (int)HttpStatusCode.OK)]
		[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
		public async Task<ApiResponse<List<Game>>> GetAll(
			[FromQuery] int skip = 0,
			[FromQuery] int take = 100)
		{
			return Ok(await _repository.GetGames(skip, take));
		}

		[HttpGet("{code}")]
		[ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
		[ProducesResponseType(typeof(ApiResponse<Game>), (int)HttpStatusCode.OK)]
		[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
		public async Task<ApiResponse<Game>> GetGame([FromRoute] string code)
		{
			Game game = await _repository.GetGameByCode(code);

			if (game is null)
				return NotFound($"Cannot find game with code: [{code}]");

			return Ok(game);
		}

		[HttpGet("current")]
		[ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
		[ProducesResponseType(typeof(ApiResponse<Game>), (int)HttpStatusCode.OK)]
		[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
		public Task<ApiResponse<Game>> GetCurrentGame()
			=> GetGame(User.GetGameCode());

		[HttpDelete("{code}")]
		[ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
		[ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.OK)]
		[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "Organizer")]
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

			await _lobbyContext.Clients.Group($"{code}-lobby").GameDeleted();

			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

			return new ApiResponse("Game deleted");
		}

		[HttpDelete("current")]
		[ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
		[ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.OK)]
		[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "Organizer")]
		public Task<ApiResponse> DeleteCurrentGame()
			=> DeleteGame(User.GetGameCode());

		[HttpGet("{code}/players")]
		[ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
		[ProducesResponseType(typeof(ApiResponse<List<Player>>), (int)HttpStatusCode.OK)]
		[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
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

		[HttpGet("current/players")]
		[ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
		[ProducesResponseType(typeof(ApiResponse<List<Player>>), (int)HttpStatusCode.OK)]
		[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
		public Task<ApiResponse<List<Player>>> GetCurrentGamePlayers(
			[FromQuery] Team? team = null,
			[FromQuery] bool? isSpyMaster = null)
			=> GetGamePlayers(
				User.GetGameCode(),
				team,
				isSpyMaster);

		[HttpGet("{code}/players/self")]
		[ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
		[ProducesResponseType(typeof(ApiResponse<Player>), (int)HttpStatusCode.OK)]
		[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
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

		[HttpGet("current/players/self")]
		[ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
		[ProducesResponseType(typeof(ApiResponse<Player>), (int)HttpStatusCode.OK)]
		[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
		public Task<ApiResponse<Player>> GetCurrentGameSelf()
			=> GetSelfGamePlayer(User.GetGameCode());

		[HttpPost("{code}/quit")]
		[ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
		[ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.OK)]
		[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "Player")]
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

			await _repository.UpdateGame(code, game);

			await _lobbyContext.Clients.Group($"{code}-lobby").PlayerLeft(player);

			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

			return new ApiResponse("Disconnected");
		}

		[HttpPost("current/quit")]
		[ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
		[ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.OK)]
		[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "Player")]
		public Task<ApiResponse> QuitCurrentGame()
			=> QuitGame(User.GetGameCode());

		[HttpPost("{code}/start")]
		[ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
		[ProducesResponseType(typeof(ApiResponse<Game>), (int)HttpStatusCode.OK)]
		[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "Organizer")]
		public async Task<ApiResponse<Game>> StartGame(
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

			await _repository.UpdateGame(code, game);

			await _lobbyContext.Clients.Group($"{code}-lobby").GameStarted();

			return Ok(game);
		}

		[HttpPost("current/start")]
		[ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
		[ProducesResponseType(typeof(ApiResponse<Game>), (int)HttpStatusCode.OK)]
		[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "Organizer")]
		public Task<ApiResponse<Game>> StartCurrentGame()
			=> StartGame(User.GetGameCode());

		[HttpPost("{code}/join")]
		[ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
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

			await _repository.UpdateGame(code, game);

			await SignInAsPlayer(player, code);

			await _lobbyContext.Clients.Group($"{code}-lobby").PlayerAdded(player);

			return Ok(game);
		}

		[HttpPost("{code}/players")]
		[ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
		[ProducesResponseType(typeof(ApiResponse<Player>), (int)HttpStatusCode.OK)]
		[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "Organizer")]
		public async Task<ApiResponse<Player>> AddGameBot(
			[FromRoute] string code)
		{
			Game game = await _repository.GetGameByCode(code);

			if (game is null)
				return NotFound($"Cannot find game with code: [{code}]");

			if (game.Status != GameStatus.Lobby)
				return BadRequest($"Cannot add bot to game in status: [{game.Status}]");

			var player = game.AddNewPlayer(_nameGenerator.GetRandomName(), isBot: true);

			await _repository.UpdateGame(code, game);

			await _lobbyContext.Clients.Group($"{code}-lobby").PlayerAdded(player);

			return Ok(player);
		}

		[HttpPost("current/players")]
		[ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
		[ProducesResponseType(typeof(ApiResponse<Player>), (int)HttpStatusCode.OK)]
		[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "Organizer")]
		public Task<ApiResponse<Player>> AddCurrentGameBot()
			=> AddGameBot(User.GetGameCode());

		[HttpGet("{code}/players/{number}")]
		[ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
		[ProducesResponseType(typeof(ApiResponse<Player>), (int)HttpStatusCode.OK)]
		[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
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

		[HttpGet("current/players/{number}")]
		[ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
		[ProducesResponseType(typeof(ApiResponse<Player>), (int)HttpStatusCode.OK)]
		[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
		public Task<ApiResponse<Player>> GetCurrentGamePlayer(
			[FromRoute] int number)
			=> GetGamePlayer(User.GetGameCode(), number);

		[HttpPost("{code}/players/{number}")]
		[ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
		[ProducesResponseType(typeof(ApiResponse<Player>), (int)HttpStatusCode.OK)]
		[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
		public async Task<ApiResponse<Player>> UpdatePlayer(
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

			await _repository.UpdateGame(code, game);

			await _lobbyContext.Clients.Group($"{code}-lobby").PlayerUpdated(player);

			return Ok(player);
		}

		[HttpPost("current/players/{number}")]
		[ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
		[ProducesResponseType(typeof(ApiResponse<Player>), (int)HttpStatusCode.OK)]
		[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
		public Task<ApiResponse<Player>> UpdatePlayerInCurrentGame(
			[FromRoute] int number,
			[FromBody] PlayerModel playerModel)
			=> UpdatePlayer(User.GetGameCode(), number, playerModel);

		protected Task SignInAsPlayer(Player player, string code)
		{
			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, player.Id.ToString()),
				new Claim(ClaimTypes.Role, player.IsOrganizer ? "Organizer" : "Player"),
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
	}
}