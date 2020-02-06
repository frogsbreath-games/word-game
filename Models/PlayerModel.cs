using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordGame.API.Domain.Enums;
using WordGame.API.Domain.Models;

namespace WordGame.API.Models
{
	public class PlayerModel
	{
		public PlayerModel(Player player)
			: this(
				player.Number,
				player.Name,
				player.Type,
				player.Role,
				player.Team)
		{
		}

		public PlayerModel(int number, string name, PlayerType type, UserRole role, Team team)
		{
			Number = number;
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Type = type;
			Role = role;
			Team = team;
		}

		public int Number { get; }

		public string Name { get; }

		public PlayerType Type { get; }

		public UserRole Role { get; }

		public Team Team { get; }
	}
}
