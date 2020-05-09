using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordGame.API.Application.Configuration;
using WordGame.API.Domain.Enums;
using WordGame.API.Domain.Models;
using WordGame.API.Domain.Repositories;

namespace WordGame.API.Data.Repositories
{
	public class GameRepository : IGameRepository
	{
		protected readonly IMongoClient _mongoClient;
		protected readonly IOptions<MongoDbSettings> _settings;

		protected IMongoCollection<Game> Games
			=> _mongoClient
				.GetDatabase(_settings.Value.GameDatabaseName)
				.GetCollection<Game>("game");

		public GameRepository(IMongoClient mongoClient, IOptions<MongoDbSettings> settings)
		{
			_mongoClient = mongoClient ?? throw new ArgumentNullException(nameof(mongoClient));
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));
		}

		public async Task AddGame(Game game)
		{
			await Games.InsertOneAsync(game);
		}

		public async Task<List<Game>> GetGames(int skip, int take)
		{
			return await Games.Find(x => true)
				.SortByDescending(x => x.CreatedDate)
				.Skip(skip)
				.Limit(take)
				.ToListAsync();
		}

		public async Task<Game> GetGameByCode(string code)
		{
			return await Games.Find(x => x.Code == code.ToUpper()
					&& x.Status != GameStatus.Archived)
				.SingleOrDefaultAsync();
		}

		public async Task<DeleteResult> DeleteGame(string code)
		{
			return await Games.DeleteOneAsync(x => x.Code == code.ToUpper());
		}

		public async Task<ReplaceOneResult> UpdateGame(string code, Game game)
		{
			return await Games.ReplaceOneAsync(
				x => x.Code == code.ToUpper(),
				game);
		}
	}
}
