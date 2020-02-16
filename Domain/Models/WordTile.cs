using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordGame.API.Domain.Enums;

namespace WordGame.API.Domain.Models
{
	public class WordTile
	{
		public WordTile(string word, Team team, bool isRevealed = false)
		{
			Word = word ?? throw new ArgumentNullException(nameof(word));
			Team = team;
			IsRevealed = isRevealed;
		}

		public string Word { get; protected set; }
		public Team Team { get; set; }
		public bool IsRevealed { get; set; }
		public List<PlayerVote> Votes { get; set; } = new List<PlayerVote>();

		public void ReplaceWord(string word)
		{
			Word = word;
		}
	}
}
