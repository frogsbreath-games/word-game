using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordGame.API.Domain.Enums;

namespace WordGame.API.Domain.Models
{
	public class Guess
	{
		public Guess(string word, int guessNumber, Team team)
		{
			Word = word ?? throw new ArgumentNullException(nameof(word));
			GuessNumber = guessNumber;
			Team = team;
		}

		public string Word { get; protected set; }
		public int GuessNumber { get; protected set; }
		public Team Team { get; protected set; }
	}
}
