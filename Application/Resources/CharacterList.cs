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
			new Character("Azami", CharacterType.Cultist, 0, Team.Red),
			new Character("Z'arri", CharacterType.Cultist, 1, Team.Blue),
			new Character("Dmitry", CharacterType.Researcher, 2, Team.Red),
			new Character("Moore", CharacterType.Researcher, 3, Team.Blue),
			new Character("Belinsky", CharacterType.Researcher, 4, Team.Red),
			new Character("Bernard", CharacterType.Researcher, 5, Team.Blue),
			new Character("Tatyana", CharacterType.Researcher, 6, Team.Red),
			new Character("Womack", CharacterType.Researcher, 7, Team.Blue),
			new Character("Yeltsin", CharacterType.Researcher, 8, Team.Red),
			new Character("Winthrop", CharacterType.Researcher, 9, Team.Blue),
		};
	}
}
