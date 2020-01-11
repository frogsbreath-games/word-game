﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WordGame.API.Application.Resources;
using WordGame.API.Domain.Enums;
using WordGame.API.Domain.Models;
using WordGame.API.Extensions;

namespace WordGame.API.Application.Services
{
	public interface IGameBoardGenerator
	{
		public List<WordTile> GenerateGameBoard(Team startingTeam);
	}

	public class GameBoardGenerator : IGameBoardGenerator
	{
		public List<WordTile> GenerateGameBoard(Team startingTeam)
		{
			if (startingTeam != Team.Red && startingTeam != Team.Blue)
				throw new Exception();

			List<Team> teams = new List<Team> { startingTeam };

			teams.AddRange(Enumerable.Repeat(Team.Red, 8));
			teams.AddRange(Enumerable.Repeat(Team.Blue, 8));
			teams.AddRange(Enumerable.Repeat(Team.Neutral, 7));
			teams.Add(Team.Black);

			var random = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId));

			teams.Shuffle(random);

			var ret = new List<WordTile>();

			string word;
			foreach (var team in teams)
			{
				do
				{
					word = WordList.Words[random.Next(0, WordList.Words.Length)];
				}
				while (ret.Any(x => x.Word == word));

				ret.Add(new WordTile(word, team));
			}

			return ret;
		}
	}
}