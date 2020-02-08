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

		public string? HintWord { get; protected set; }

		public int? WordCount { get; protected set; }

		public List<Guess> Guesses { get; protected set; } = new List<Guess>();

		public List<PlayerVote> EndTurnVotes { get; protected set; } = new List<PlayerVote>();

		public int? GuessesRemaining
			=> (Status == TurnStatus.Guessing || Status == TurnStatus.Tallying) && WordCount.HasValue && WordCount.Value > 0
				? WordCount.Value - Guesses.Count + 1
				: (int?)null;

		public void GiveHint(
			string hintWord,
			int wordCount)
		{
			if (Status != TurnStatus.Planning)
				throw new InvalidOperationException();

			HintWord = hintWord ?? throw new ArgumentNullException(nameof(hintWord));
			WordCount = wordCount;
			Status = TurnStatus.PendingApproval;
		}

		public void ApproveHint()
		{
			if (Status != TurnStatus.PendingApproval)
				throw new InvalidOperationException();

			Status = TurnStatus.Guessing;
		}

		public void RefuseHint()
		{
			if (Status != TurnStatus.PendingApproval)
				throw new InvalidOperationException();

			HintWord = null;
			WordCount = null;
			Status = TurnStatus.Planning;
		}

		public void End()
		{
			if (Status != TurnStatus.Tallying)
				throw new InvalidOperationException();

			Status = TurnStatus.Over;
		}

		public void SetToTallying()
		{
			if (Status != TurnStatus.Guessing)
				throw new InvalidOperationException();

			Status = TurnStatus.Tallying;
		}

		public void GuessAgain()
		{
			if (Status != TurnStatus.Tallying)
				throw new InvalidOperationException();

			Status = TurnStatus.Guessing;
		}
	}
}
