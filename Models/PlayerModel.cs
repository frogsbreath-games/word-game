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
				player.Character?.Number,
				player.Character?.Name,
				player.Character?.Type,
				player.Role,
				player.Team)
		{
		}

		public PlayerModel(int number, string name, int? characterNumber, string? characterName, CharacterType? type, UserRole role, Team team)
		{
			Number = number;
			Name = name ?? throw new ArgumentNullException(nameof(name));
			CharacterNumber = characterNumber;
			CharacterName = characterName;
			Type = type;
			Role = role;
			Team = team;
		}

		public int Number { get; }

		public string Name { get; }

		public int? CharacterNumber { get; }

		public string? CharacterName { get; }

		public CharacterType? Type { get; }

		public UserRole Role { get; }

		public Team Team { get; }
	}
}
