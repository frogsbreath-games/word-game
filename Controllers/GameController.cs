﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WordGame.API.Application.Services;
using WordGame.API.Domain.Enums;
using WordGame.API.Domain.Models;
using WordGame.API.Domain.Repositories;
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

        public GameController(IGameRepository repository, INameGenerator nameGenerator)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _nameGenerator = nameGenerator ?? throw new ArgumentNullException(nameof(nameGenerator));
        }

        protected ApiResponse NotFound(string message)
        {
            Response.StatusCode = (int)HttpStatusCode.NotFound;

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

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<Game>), (int)HttpStatusCode.Created)]
        public async Task<ApiResponse<Game>> CreateGame()
        {
            var game = new Game(
                Guid.NewGuid().ToString().Substring(0, 6).ToUpper(),
                _nameGenerator.GetRandomName(),
                Team.Red,
                true);

            await _repository.AddGame(game);

            return Created(game.Code, game);
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<Game>>), (int)HttpStatusCode.OK)]
        public async Task<ApiResponse<List<Game>>> GetAll(
            [FromQuery] int skip = 0,
            [FromQuery] int take = 100)
        {
            return Ok(await _repository.GetGames(skip, take));
        }

        [HttpGet("{code}")]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponse<Game>), (int)HttpStatusCode.OK)]
        public async Task<ApiResponse<Game>> GetByCode([FromRoute] string code)
        {
            Game game = await _repository.GetGameByCode(code);

            if (game is null)
                return NotFound($"Cannot find game with code: [{code}]");

            return Ok(game);
        }

        [HttpDelete("{code}")]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.OK)]
        public async Task<ApiResponse> DeleteGame([FromRoute] string code)
        {
            Game game = await _repository.GetGameByCode(code);

            if (game is null)
                return NotFound($"Cannot find game with code: [{code}]");

            await _repository.DeleteGame(code);

            return new ApiResponse("Game deleted");
        }

        [HttpGet("{code}/players")]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponse<List<Player>>), (int)HttpStatusCode.OK)]
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

            return players.ToList();
        }

        [HttpPost("{code}/players")]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponse<Player>), (int)HttpStatusCode.Created)]
        public async Task<ApiResponse<Game>> JoinGame(
            [FromRoute] string code)
        {
            Game game = await _repository.GetGameByCode(code);

            if (game is null)
                return NotFound($"Cannot find game with code: [{code}]");

            IEnumerable<Player> players = game.Players;

            int maxNumber = players.Max(x => x.Number);

            var redPlayers = players.Where(x => x.Team == Team.Red);
            var bluePlayers = players.Where(x => x.Team == Team.Blue);

            var team = redPlayers.Count() > bluePlayers.Count()
                ? Team.Blue
                : Team.Red;

            var hasSpyMaster = team == Team.Red
                ? redPlayers.Any(x => x.IsSpyMaster)
                : bluePlayers.Any(x => x.IsSpyMaster);

            var player = new Player(
                _nameGenerator.GetRandomName(),
                false,
                !hasSpyMaster,
                maxNumber + 1,
                team);

            game.AddPlayer(player);

            await _repository.UpdateGame(code, game);

            return Ok(game);
        }

        [HttpGet("{code}/players/{number}")]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponse<Player>), (int)HttpStatusCode.OK)]
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

        [HttpPost("{code}/players/{number}")]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponse<Player>), (int)HttpStatusCode.OK)]
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

            return Ok(player);
        }
    }
}