using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WordGame.API.Domain.Enums;

namespace WordGame.API.Domain.Models
{
	public class Game
	{
		[JsonIgnore]
		public ObjectId Id { get; protected set; }

		[JsonIgnore]
		public DateTime CreatedDate { get; protected set; }

		public string GameCode { get; protected set; }

		public GameStatus Status { get; protected set; }

		public List<Player> Players { get; protected set; } = new List<Player>();

		public Game(
			string gameCode,
			string adminNickName,
			Team adminTeam,
			bool adminIsSpyMaster,
			DateTime? createdDate = null,
			ObjectId? id = null)
		{
			Id = id ?? ObjectId.GenerateNewId();
			CreatedDate = createdDate ?? DateTime.Now;
			GameCode = gameCode ?? throw new ArgumentNullException(nameof(gameCode));
			Status = GameStatus.Lobby;
			Players.Add(new Player(
				adminNickName,
				true,
				adminIsSpyMaster,
				adminTeam,
				createdDate: CreatedDate));
		}
	}
}
