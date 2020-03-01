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
using WordGame.API.Application.Exceptions;
using WordGame.API.Application.Resources;
using WordGame.API.Application.Services;
using WordGame.API.Attributes;
using WordGame.API.Domain;
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
		protected IGameUpdater _gameUpdater;

		public GameController(
			IGameRepository repository,
			INameGenerator nameGenerator,
			IGameBoardGenerator gameBoardGenerator,
			IRandomAccessor randomAccessor,
			IGameUpdater gameUpdater)
		{
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
			_nameGenerator = nameGenerator ?? throw new ArgumentNullException(nameof(nameGenerator));
			_gameBoardGenerator = gameBoardGenerator ?? throw new ArgumentNullException(nameof(gameBoardGenerator));
			_randomAccessor = randomAccessor ?? throw new ArgumentNullException(nameof(randomAccessor));
			_gameUpdater = gameUpdater ?? throw new ArgumentNullException(nameof(gameUpdater));
		}

		protected async Task<Game> GetGame(string gameCode)
		{
			return await _repository.GetGameByCode(gameCode)
				?? throw new HttpException(HttpStatusCode.NotFound, $"Cannot find game with code: [{gameCode}]");
		}

		protected async Task<(Game, Player)> GetGameAndLocalPlayer(string gameCode)
		{
			var game = await GetGame(gameCode);

			var player = User.GetPlayer(game)
				?? throw new HttpException(HttpStatusCode.NotFound, "Cannot get game player is not in.");

			return (game, player);
		}

		protected async Task<Player> GetLocalPlayer(string gameCode)
		{
			(_, var localPlayer) = await GetGameAndLocalPlayer(gameCode);
			return localPlayer;
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
			where T : class
		{
			Response.StatusCode = (int)HttpStatusCode.Created;
			Response.Headers["Location"] = $"{Request.Path}/{pathPart}";

			return obj;
		}

		protected ApiResponse<T> Ok<T>(T obj)
			where T : class
		{
			Response.StatusCode = (int)HttpStatusCode.OK;

			return obj;
		}

		//This should only be used if your game got deleted without you somehow.
		[HttpPost("forceSignOut"), UserAuthorize]
		public async Task<ApiResponse> ForceSignOut()
		{
			try
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
			}
			catch (HttpException)
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
				Team.Red);

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
		public async Task<ApiResponse<GameModel>> GetGameModel([FromRoute] string code)
		{
			(var game, var localPlayer) = await GetGameAndLocalPlayer(code);

			return Ok(new GameModel(game, localPlayer));
		}

		[HttpGet("current"), UserAuthorize]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[Returns(typeof(GameModel))]
		public Task<ApiResponse<GameModel>> GetCurrentGame()
			=> GetGameModel(User.GetGameCode());

		[HttpDelete("{code}"), UserAuthorize(UserRole.Organizer)]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[ReturnsStatus(HttpStatusCode.Accepted)]
		public async Task<ApiResponse> DeleteGame([FromRoute] string code)
		{
			var game = await GetGame(code);

			await _gameUpdater.DeleteGame(game);

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
		[Returns(typeof(List<PlayerModel>))]
		public async Task<ApiResponse<List<PlayerModel>>> GetGamePlayers(
			[FromRoute] string code,
			[FromQuery] Team? team = null,
			[FromQuery] CharacterType? type = null)
		{
			(var game, _) = await GetGameAndLocalPlayer(code);

			IEnumerable<Player> players = game.Players;

			if (team.HasValue)
				players = players.Where(p => p.Team == team.Value);

			if (type.HasValue)
				players = players.FilterByType(type.Value);

			return Ok(players.Select(p => new PlayerModel(p)).ToList());
		}

		[HttpGet("current/players"), UserAuthorize]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[Returns(typeof(List<PlayerModel>))]
		public Task<ApiResponse<List<PlayerModel>>> GetCurrentGamePlayers(
			[FromQuery] Team? team = null,
			[FromQuery] CharacterType? type = null)
			=> GetGamePlayers(
				User.GetGameCode(),
				team,
				type);

		[HttpGet("{code}/players/self"), UserAuthorize]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[Returns(typeof(PlayerModel))]
		public async Task<ApiResponse<PlayerModel>> GetSelfGamePlayer(
			[FromRoute] string code)
		{
			var localPlayer = await GetLocalPlayer(code);

			return Ok(new PlayerModel(localPlayer));
		}

		[HttpGet("current/players/self"), UserAuthorize]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[Returns(typeof(PlayerModel))]
		public Task<ApiResponse<PlayerModel>> GetCurrentGameSelf()
			=> GetSelfGamePlayer(User.GetGameCode());

		[HttpPost("{code}/quit"), UserAuthorize(UserRole.Player)]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.OK)]
		public async Task<ApiResponse> QuitGame(
			[FromRoute] string code)
		{
			(var game, var localPlayer) = await GetGameAndLocalPlayer(code);

			game.Players.Remove(localPlayer);

			await _gameUpdater.UpdateGame(game);

			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

			return new ApiResponse("Disconnected");
		}

		[HttpPost("current/quit"), UserAuthorize(UserRole.Player)]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.OK)]
		public Task<ApiResponse> QuitCurrentGame()
			=> QuitGame(User.GetGameCode());

		[HttpPost("{code}/generateBoard"), UserAuthorize(UserRole.Organizer)]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[ReturnsStatus(HttpStatusCode.Accepted)]
		public async Task<ApiResponse> GenerateGameBoard(
			[FromRoute] string code)
		{
			(var game, var localPlayer) = await GetGameAndLocalPlayer(code);

			if (game.CanGenerateBoard(localPlayer).IsFailure(out string message))
				return BadRequest(message);

			var startingTeam = _randomAccessor.Random.Next(2) == 0 ? Team.Red : Team.Blue;

			var board = _gameBoardGenerator.GenerateGameBoard(startingTeam);

			game.GenerateBoard(localPlayer, board);

			await _gameUpdater.UpdateGame(game);

			return Accepted("Board Generated");
		}

		[HttpPost("current/generateBoard"), UserAuthorize(UserRole.Organizer)]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[ReturnsStatus(HttpStatusCode.Accepted)]
		public Task<ApiResponse> GenerateCurrentGameBoard()
			=> GenerateGameBoard(User.GetGameCode());

		[HttpPost("{code}/replaceWord"), UserAuthorize(UserRole.Organizer)]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[ReturnsStatus(HttpStatusCode.Accepted)]
		public async Task<ApiResponse> ReplaceWord(
			[FromRoute] string code,
			[FromBody] ReplaceWordModel replaceWordModel)
		{
			(var game, var localPlayer) = await GetGameAndLocalPlayer(code);

			if (game.CanReplaceWord(localPlayer).IsFailure(out string message))
				return BadRequest(message);

			if (!(game.WordTiles.SingleOrDefault(t => t.Word == replaceWordModel.Word) is WordTile tile))
				return NotFound($"{replaceWordModel.Word} is not on the game board.");

			string word = string.Empty;
			do
			{
				word = WordList.Words[_randomAccessor.Random.Next(0, WordList.Words.Length)];
			}
			while (game.WordTiles.Any(wt => wt.Word == word));

			game.ReplaceWord(localPlayer, tile, word);

			await _gameUpdater.UpdateGame(game);

			return Accepted("Organizer Replaced Word.");
		}

		[HttpPost("current/replaceWord"), UserAuthorize(UserRole.Organizer)]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[ReturnsStatus(HttpStatusCode.Accepted)]
		public Task<ApiResponse> CurrentGameReplaceWord(
			[FromBody] ReplaceWordModel replaceWordModel)
			=> ReplaceWord(User.GetGameCode(), replaceWordModel);

		[HttpPost("{code}/start"), UserAuthorize(UserRole.Organizer)]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[ReturnsStatus(HttpStatusCode.Accepted)]
		public async Task<ApiResponse> StartGame(
			[FromRoute] string code)
		{
			(var game, var localPlayer) = await GetGameAndLocalPlayer(code);

			if (game.CanStart(localPlayer).IsFailure(out string message))
				return BadRequest(message);

			game.StartGame(localPlayer);

			await _gameUpdater.UpdateGame(game);

			return Accepted("Game Started");
		}

		[HttpPost("current/start"), UserAuthorize(UserRole.Organizer)]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[ReturnsStatus(HttpStatusCode.Accepted)]
		public Task<ApiResponse> StartCurrentGame()
			=> StartGame(User.GetGameCode());

		[HttpPost("{code}/backToLobby"), UserAuthorize(UserRole.Organizer)]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[ReturnsStatus(HttpStatusCode.Accepted)]
		public async Task<ApiResponse> BackToLobby(
			[FromRoute] string code)
		{
			(var game, var localPlayer) = await GetGameAndLocalPlayer(code);

			if (game.CanRestart(localPlayer).IsFailure(out string message))
				return BadRequest(message);

			game.BackToLobby(localPlayer);

			await _gameUpdater.UpdateGame(game);

			return Accepted("Back to lobby");
		}

		[HttpPost("current/backToLobby"), UserAuthorize(UserRole.Organizer)]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[ReturnsStatus(HttpStatusCode.Accepted)]
		public Task<ApiResponse> CurrentGameBackToLobby()
			=> BackToLobby(User.GetGameCode());

		[HttpPost("{code}/join")]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[ProducesResponseType(typeof(ApiResponse<GameModel>), (int)HttpStatusCode.OK)]
		public async Task<ApiResponse<GameModel>> JoinGame(
			[FromRoute] string code)
		{
			if (User.Identity.IsAuthenticated)
				return BadRequest("User is already in a game.");

			var game = await GetGame(code);

			if (game.Status != GameStatus.Lobby)
				return BadRequest($"Cannot join game in status: [{game.Status}]");

			if (game.Players.Count >= 10)
				return BadRequest("Game cannot have more than 10 players");

			var player = game.AddNewPlayer(_nameGenerator.GetRandomName());

			await _gameUpdater.UpdateGame(game);

			await SignInAsPlayer(player, code);

			return Ok(new GameModel(game, player.Id));
		}

		[HttpPost("{code}/players"), UserAuthorize(UserRole.Organizer)]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[ReturnsStatus(HttpStatusCode.Accepted)]
		public async Task<ApiResponse> AddGameBot(
			[FromRoute] string code,
			[FromBody] Team team)
		{
			(var game, var localPlayer) = await GetGameAndLocalPlayer(code);

			if (game.CanAddBot(localPlayer).IsFailure(out string message))
				return BadRequest(message);

			game.AddNewPlayer(_nameGenerator.GetRandomName(), UserRole.Bot, team);

			await _gameUpdater.UpdateGame(game);

			return Accepted("Bot Added.");
		}

		[HttpPost("current/players"), UserAuthorize(UserRole.Organizer)]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[ReturnsStatus(HttpStatusCode.Accepted)]
		public Task<ApiResponse> AddCurrentGameBot([FromBody] Team team)
			=> AddGameBot(User.GetGameCode(), team);

		[HttpGet("{code}/players/{number}"), UserAuthorize]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[Returns(typeof(PlayerModel))]
		public async Task<ApiResponse<PlayerModel>> GetGamePlayer(
			[FromRoute] string code,
			[FromRoute] int number)
		{
			var game = await GetGame(code);

			var player = game.Players.SingleOrDefault(x => x.Number == number);

			if (player is null)
				return NotFound($"Cannot find player with number: [{number}] in game with code: [{code}]");

			return Ok(new PlayerModel(player));
		}

		[HttpGet("current/players/{number}"), UserAuthorize]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[Returns(typeof(PlayerModel))]
		public Task<ApiResponse<PlayerModel>> GetCurrentGamePlayer(
			[FromRoute] int number)
			=> GetGamePlayer(User.GetGameCode(), number);

		[HttpDelete("{code}/players/{number}"), UserAuthorize(UserRole.Organizer)]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[ReturnsStatus(HttpStatusCode.Accepted)]
		public async Task<ApiResponse> DeleteGameBot(
			[FromRoute] string code,
			[FromRoute] int number)
		{
			(var game, var localPlayer) = await GetGameAndLocalPlayer(code);

			if (game.CanDeleteBot(localPlayer).IsFailure(out string message))
				return BadRequest(message);

			var player = game.Players.SingleOrDefault(x => x.Number == number);

			if (player is null || player.Role != UserRole.Bot)
				return NotFound($"Cannot find bot with number: [{number}] in game with code: [{code}]");

			game.Players.Remove(player);

			await _gameUpdater.UpdateGame(game);

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
			[FromBody] UpdatePlayerModel playerModel)
		{
			var game = await GetGame(code);

			var player = game.Players.SingleOrDefault(x => x.Number == number);

			if (player is null)
				return NotFound($"Cannot find player with number: [{number}] in game with code: [{code}]");
			
			if (playerModel.Team is Team team)
			{
				game.UpdatePlayerTeam(player, team);
			}

			if (playerModel.CharacterNumber is int characterNumber)
			{
				if (characterNumber < 0)
				{
					game.ClearPlayerCharacter(player);
				}
				else
				{
					if (game.Players.Any(p => p.Character?.Number == characterNumber))
						return BadRequest("Character is not available");

					game.UpdatePlayerCharacter(player, CharacterList.Characters.Single(c => c.Number == characterNumber));
				}
			}

			await _gameUpdater.UpdateGame(game);

			return Accepted("Player Updated.");
		}

		[HttpPost("current/players/{number}"), UserAuthorize]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[ReturnsStatus(HttpStatusCode.Accepted)]
		public Task<ApiResponse> UpdatePlayerInCurrentGame(
			[FromRoute] int number,
			[FromBody] UpdatePlayerModel playerModel)
			=> UpdatePlayer(User.GetGameCode(), number, playerModel);

		[HttpPost("{code}/giveHint"), UserAuthorize]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[ReturnsStatus(HttpStatusCode.Accepted)]
		public async Task<ApiResponse> GiveHint(
			[FromRoute] string code,
			[FromBody] HintModel hintModel)
		{
			(var game, var localPlayer) = await GetGameAndLocalPlayer(code);

			if (game.CanGiveHint(localPlayer).IsFailure(out string message))
				return BadRequest(message);

			game.GiveHint(localPlayer, hintModel.HintWord, hintModel.WordCount);

			await _gameUpdater.UpdateGame(game);

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
			(var game, var localPlayer) = await GetGameAndLocalPlayer(code);

			if (game.CanReviewHint(localPlayer).IsFailure(out string message))
				return BadRequest(message);

			game.ApproveHint(localPlayer);

			await _gameUpdater.UpdateGame(game);

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
			(var game, var localPlayer) = await GetGameAndLocalPlayer(code);

			if (game.CanReviewHint(localPlayer).IsFailure(out string message))
				return BadRequest(message);

			game.CurrentTurn.RefuseHint();

			await _gameUpdater.UpdateGame(game);

			return Accepted("Hint refused.");
		}

		[HttpPost("current/refuseHint"), UserAuthorize]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[ReturnsStatus(HttpStatusCode.Accepted)]
		public Task<ApiResponse> RefuseCurrentGameHint()
			=> RefuseHint(User.GetGameCode());

		[HttpPost("{code}/voteWord"), UserAuthorize]
		[ReturnsStatus(HttpStatusCode.NotFound)]
		[ReturnsStatus(HttpStatusCode.Accepted)]
		public async Task<ApiResponse> VoteWord(
			[FromRoute] string code,
			[FromBody] VoteModel voteModel)
		{
			(var game, var localPlayer) = await GetGameAndLocalPlayer(code);

			if (game.CanVote(localPlayer).IsFailure(out string message))
				return BadRequest(message);

			if (!(game.WordTiles.SingleOrDefault(t => t.Word == voteModel.Word) is WordTile tile))
				return NotFound($"{voteModel.Word} is not on the game board.");

			if (tile.IsRevealed)
				return BadRequest($"{voteModel.Word} is already revealed.");

			game.SetPlayerVote(localPlayer, tile);

			await _gameUpdater.UpdateGame(game);

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
			(var game, var localPlayer) = await GetGameAndLocalPlayer(code);

			if (game.CanVote(localPlayer).IsFailure(out string message))
				return BadRequest(message);

			game.VoteEndTurn(localPlayer);

			await _gameUpdater.UpdateGame(game);

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
				new Claim(ClaimTypes.Role, player.Role.ToString()),
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
