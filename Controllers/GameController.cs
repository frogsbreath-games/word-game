using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WordGame.API.Domain.Enums;
using WordGame.API.Domain.Models;
using WordGame.API.Domain.Repositories;

namespace WordGame.API.Controllers
{
    [ApiController]
    [Route("api/games")]
    public class GameController : ControllerBase
    {
        protected IGameRepository _repository;

        public GameController(IGameRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        [HttpPost]
        public async Task<ApiResponse<Game>> CreateGame()
        {
            var game = new Game(
                Guid.NewGuid().ToString().Substring(0, 6).ToUpper(),
                "Boss Man",
                Team.Red,
                true);

            await _repository.AddGame(game);

            return await _repository.GetGameByCode(game.Code);
        }

        [HttpGet]
        public async Task<ApiResponse<List<Game>>> GetAll(
            [FromQuery] int skip = 0,
            [FromQuery] int take = 100)
        {
            return await _repository.GetGames(skip, take);
        }

        [HttpGet("{code}")]
        public async Task<ApiResponse<Game>> GetByCode([FromRoute] string code)
        {
            return await _repository.GetGameByCode(code);
        }

        [HttpDelete("{code}")]
        public async Task<ApiResponse> DeleteGame([FromRoute] string code)
        {
            await _repository.DeleteGame(code);

            return new ApiResponse("Game deleted");
        }
    }
}