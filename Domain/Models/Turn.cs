using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordGame.API.Domain.Enums;

namespace WordGame.API.Domain.Models
{
	public class Turn
	{
		public Turn(Team team, int turnNumber)
		{
			Team = team;
			TurnNumber = turnNumber;
		}

		public Team Team { get; protected set; }

		public int TurnNumber { get; protected set; }

		public TurnStatus Status { get; protected set; }

		public string HintWord { get; protected set; }

		public int? WordCount { get; protected set; }

		public List<Guess> Guesses { get; protected set; } = new List<Guess>();

		public void GiveHint(
			string hintWord,
			int wordCount)
		{
			if (Status != TurnStatus.Planning)
				throw new InvalidOperationException();

			HintWord = hintWord ?? throw new ArgumentNullException(nameof(hintWord));
			WordCount = wordCount;
			Status = TurnStatus.Guessing;
		}
	}
}
