using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordGame.API.Domain.Enums;
using WordGame.API.Domain.Models;

namespace WordGame.API.Models
{
	public class TurnModel
	{
		public TurnModel(
			Team team,
			int turnNumber,
			TurnStatus status,
			string? hintWord,
			int? wordCount,
			List<PlayerVote> endTurnVotes,
			int? guessesRemaining)
		{
			Team = team;
			TurnNumber = turnNumber;
			Status = status;
			HintWord = hintWord;
			WordCount = wordCount;
			EndTurnVotes = endTurnVotes ?? throw new ArgumentNullException(nameof(endTurnVotes));
			GuessesRemaining = guessesRemaining;
		}

		public Team Team { get; }

		public int TurnNumber { get; }

		public TurnStatus Status { get; }

		public string? HintWord { get; }

		public int? WordCount { get; }

		public List<PlayerVote> EndTurnVotes { get; }

		public int? GuessesRemaining { get; }
	}
}
