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
			CanStart = game.CanStart(localPlayer);
			CanDelete = game.CanDelete(localPlayer);
			CanAddBot = game.CanAddBot(localPlayer);
			CanDeleteBot = game.CanDeleteBot(localPlayer);
			CanGiveHint = game.CanGiveHint(localPlayer);
			CanApproveHint = game.CanReviewHint(localPlayer);
			CanVote = game.CanVote(localPlayer);
		}

		public bool CanStart { get; } = false;
		public bool CanDelete { get; } = false;
		public bool CanAddBot { get; } = false;
		public bool CanDeleteBot { get; } = false;
		public bool CanGiveHint { get; } = false;
		public bool CanApproveHint { get; } = false;
		public bool CanVote { get; } = false;
	}
}
