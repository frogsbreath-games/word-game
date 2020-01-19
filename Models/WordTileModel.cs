using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordGame.API.Domain.Enums;
using WordGame.API.Domain.Models;

namespace WordGame.API.Models
{
	public class WordTileModel
	{
		public WordTileModel(string word, Team team, bool isRevealed, List<PlayerVote> votes)
		{
			Word = word ?? throw new ArgumentNullException(nameof(word));
			Team = team;
			IsRevealed = isRevealed;
			Votes = votes ?? throw new ArgumentNullException(nameof(votes));
		}

		public string Word { get; }
		public Team Team { get; }
		public bool IsRevealed { get; }
		public List<PlayerVote> Votes { get; }
	}
}
