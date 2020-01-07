using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordGame.API.Domain.Models;

namespace WordGame.API.Domain.Repositories
{
	public interface IGameRepository
	{
		Task<List<Game>> GetGames(int skip, int take);

		Task AddGame(Game game);

		Task<Game> GetGameByCode(string code);

		Task<ReplaceOneResult> UpdateGame(string code, Game game);

		Task<DeleteResult> DeleteGame(string code);
	}
}
