using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordGame.API.Domain.Enums;
using WordGame.API.Domain.Models;

namespace WordGame.API.Models
{
	public class GameActionsModel
	{
		public GameActionsModel(Game game, Player localPlayer)
		{
			if (localPlayer.IsOrganizer)
			{
				CanStart = game.GameCanStart();
				CanDelete = true;
				CanAddBot = game.Status == GameStatus.Lobby && game.Players.Count < 10;
				CanDeleteBot = game.Status == GameStatus.Lobby;
			}

			if (game.Status == GameStatus.InProgress)
			{
				if (localPlayer.IsSpyMaster)
				{
					CanGiveHint = game.CurrentTurn.Status == TurnStatus.Planning
						&& game.CurrentTurn.Team == localPlayer.Team;
					CanApproveHint = game.CurrentTurn.Status == TurnStatus.PendingApproval
						&& game.CurrentTurn.Team != localPlayer.Team;
				}
				else
				{
					CanVote = game.CurrentTurn.Status == TurnStatus.Guessing
						&& game.CurrentTurn.Team == localPlayer.Team;
				}
			}
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
