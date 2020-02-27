using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordGame.API.Domain.Enums;
using WordGame.API.Domain.Models;

namespace WordGame.API.Application.Resources
{
	public static class CharacterList
	{
		public static Character[] Characters = new[]
		{
			//TODO - Names
			new Character("Cultist", CharacterType.Cultist, 0, Team.Red),
			new Character("Cultist", CharacterType.Cultist, 1, Team.Blue),
			new Character("Researcher 1", CharacterType.Researcher, 2, Team.Red),
			new Character("Researcher 1", CharacterType.Researcher, 3, Team.Blue),
			new Character("Researcher 2", CharacterType.Researcher, 4, Team.Red),
			new Character("Researcher 2", CharacterType.Researcher, 5, Team.Blue),
			new Character("Researcher 3", CharacterType.Researcher, 6, Team.Red),
			new Character("Researcher 3", CharacterType.Researcher, 7, Team.Blue),
			new Character("Researcher 4", CharacterType.Researcher, 8, Team.Red),
			new Character("Researcher 4", CharacterType.Researcher, 9, Team.Blue),
		};
	}
}
