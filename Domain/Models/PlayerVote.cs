using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordGame.API.Domain.Enums;

namespace WordGame.API.Domain.Models
{
	public class PlayerVote
	{
		public PlayerVote(Team team, int number, string name)
		{
			Team = team;
			Number = number;
			Name = name ?? throw new ArgumentNullException(nameof(name));
		}

		public Team Team { get; protected set; }
		public int Number { get; protected set; }
		public string Name { get; protected set; }
	}
}
