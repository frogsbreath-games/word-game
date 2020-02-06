using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordGame.API.Domain.Enums;

namespace WordGame.API.Domain.Models
{
	public class Player
	{
		public Player(
			string name,
			PlayerType type,
			UserRole role,
			int number,
			Team team,
			Guid? id = null,
			DateTime? createdDate = null)
		{
			Id = id ?? Guid.NewGuid();
			CreatedDate = createdDate ?? DateTime.Now;
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Type = type;
			Role = role;
			Team = team;
			Number = number;
		}

		[JsonIgnore]
		public Guid Id { get; protected set; }

		[JsonIgnore]
		public DateTime CreatedDate { get; protected set; }

		public int Number { get; protected set; }

		public string Name { get; protected set; }

		public PlayerType Type { get; protected set; }

		public UserRole Role { get; protected set; }

		public Team Team { get; protected set; }

		public void UpdatePlayer(
			Team? team = null,
			string name = null,
			PlayerType? type = null)
		{
			if (team.HasValue)
				Team = team.Value;

			if (name != null)
				Name = name;

			if (type.HasValue)
				Type = type.Value;
		}
	}
}
