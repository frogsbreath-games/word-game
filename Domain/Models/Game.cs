using MongoDB.Bson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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

		public string Code { get; protected set; }

		public GameStatus Status { get; protected set; }

		public List<Player> Players { get; protected set; } = new List<Player>();

		public Game(
			string code,
			string adminName,
			Team adminTeam,
			bool adminIsSpyMaster,
			DateTime? createdDate = null,
			ObjectId? id = null)
		{
			Id = id ?? ObjectId.GenerateNewId();
			CreatedDate = createdDate ?? DateTime.Now;
			Code = code ?? throw new ArgumentNullException(nameof(code));
			Status = GameStatus.Lobby;
			AddPlayer(new Player(
				adminName,
				true,
				adminIsSpyMaster,
				0,
				adminTeam,
				createdDate: CreatedDate));
		}

		public void AddPlayer(Player player)
		{
			Players.Add(player);
		}
	}
}
