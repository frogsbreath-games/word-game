using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordGame.API.Domain.Enums;

namespace WordGame.API.Domain.Models
{
	public class Character
	{
		public Character(
			string name,
			CharacterType type,
			int number,
			Team team)
		{
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Type = type;
			Team = team;
			Number = number;
		}

		public int Number { get; protected set; }

		public string Name { get; protected set; }

		public CharacterType Type { get; protected set; }

		public Team Team { get; protected set; }
	}
}
