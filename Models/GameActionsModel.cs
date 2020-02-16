using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordGame.API.Domain;
using WordGame.API.Domain.Enums;
using WordGame.API.Domain.Models;

namespace WordGame.API.Models
{
	public class GameActionsModel
	{
		public GameActionsModel(Game game, Player localPlayer)
		{
			CanGenerateBoard = game.CanGenerateBoard(localPlayer);
			CanReplaceWord = game.CanReplaceWord(localPlayer);
			CanStart = game.CanStart(localPlayer);
			CanRestart = game.CanRestart(localPlayer);
			CanDelete = game.CanDelete(localPlayer);
			CanAddBot = game.CanAddBot(localPlayer);
			CanDeleteBot = game.CanDeleteBot(localPlayer);
			CanGiveHint = game.CanGiveHint(localPlayer);
			CanApproveHint = game.CanReviewHint(localPlayer);
			CanVote = game.CanVote(localPlayer);
		}

		public bool CanGenerateBoard { get; }
		public bool CanReplaceWord { get; }
		public bool CanStart { get; }
		public bool CanRestart { get; }
		public bool CanDelete { get; }
		public bool CanAddBot { get; }
		public bool CanDeleteBot { get; }
		public bool CanGiveHint { get; }
		public bool CanApproveHint { get; }
		public bool CanVote { get; }
	}
}
