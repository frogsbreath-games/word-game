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
			new Character("Azami D'aathess", CharacterType.Cultist, 0, Team.Red),
			new Character("Z'arri Zuibberh", CharacterType.Cultist, 1, Team.Blue),
			new Character("Dmitry Koshkin", CharacterType.Researcher, 2, Team.Red),
			new Character("Father Moore", CharacterType.Researcher, 3, Team.Blue),
			new Character("Osip Belinsky", CharacterType.Researcher, 4, Team.Red),
			new Character("Inspector Bernard", CharacterType.Researcher, 5, Team.Blue),
			new Character("Tatyana Ulanov", CharacterType.Researcher, 6, Team.Red),
			new Character("Professor Womack", CharacterType.Researcher, 7, Team.Blue),
			new Character("Komandarm Yeltsin", CharacterType.Researcher, 8, Team.Red),
			new Character("Dr Eloise Winthrop", CharacterType.Researcher, 9, Team.Blue),
		};
	}
}
