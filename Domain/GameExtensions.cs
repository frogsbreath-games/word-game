using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordGame.API.Domain.Enums;
using WordGame.API.Domain.Models;

namespace WordGame.API.Domain
{
	public static class GameExtensions
	{
		public static Try CanStart(this Game game, Player player)
		{
			if (!game.GameCanStart())
				return Try.Failure("Game cannot start");

			if (!player.IsOrganizer)
				return Try.Failure("Only the organizer start game");

			return Try.Success;
		}

		public static Try CanDelete(this Game game, Player player)
		{
			if (!player.IsOrganizer)
				return Try.Failure("Only the organizer can delete game.");

			return Try.Success;
		}

		public static Try CanAddBot(this Game game, Player player)
		{
			if (game.Status != GameStatus.Lobby)
				return Try.Failure("Cannot add bot to game that is not in the lobby.");

			if (game.Players.Count >= 10)
				return Try.Failure("Cannot add bot to game with 10 or more players.");

			if (!player.IsOrganizer)
				return Try.Failure("Only the organizer can add bots");

			return Try.Success;
		}

		public static Try CanDeleteBot(this Game game, Player player)
		{
			if (game.Status != GameStatus.Lobby)
				return Try.Failure($"Cannot delete bot from game that is not in the lobby.");

			if (!player.IsOrganizer)
				return Try.Failure("Only the organizer can add bots");

			return Try.Success;
		}

		public static Try CanGiveHint(this Game game, Player player)
		{
			if (game.Status != GameStatus.InProgress)
				return Try.Failure($"Cannot give hint in game that isn't in progress.");

			if (game.CurrentTurn.Status != TurnStatus.Planning)
				return Try.Failure($"Cannot give hint outside the planning stage of the current turn.");

			if (!player.IsSpyMaster || player.Team != game.CurrentTurn.Team)
				return Try.Failure("This player cannot give a hint!");

			return Try.Success;
		}

		public static Try CanReviewHint(this Game game, Player player)
		{
			if (game.Status != GameStatus.InProgress)
				return Try.Failure($"Cannot review hint in game that isn't in progress.");

			if (game.CurrentTurn.Status != TurnStatus.PendingApproval)
				return Try.Failure($"Cannot review hint outside the pending approval stage of the current turn.");

			if (!player.IsSpyMaster || player.Team == game.CurrentTurn.Team)
				return Try.Failure("This player cannot review a hint!");

			return Try.Success;
		}

		public static Try CanVote(this Game game, Player player)
		{
			if (game.Status != GameStatus.InProgress)
				return Try.Failure($"Cannot cast word vote in game that isn't in progress.");

			if (game.CurrentTurn.Status != TurnStatus.Guessing)
				return Try.Failure($"Cannot cast word vote outside the guessing stage of the current turn.");

			if (player.IsSpyMaster || player.Team != game.CurrentTurn.Team)
				return Try.Failure("This player cannot vote for a word!");

			return Try.Success;
		}
	}

	public abstract class Try
	{
		public static Try Success => new TrySuccess();

		public static Try Failure(string message) => new TryFailure(message);

		public bool IsSuccess() => this is TrySuccess;

		public bool IsFailure(out string message)
		{
			message = null;
			if (this is TryFailure failure)
			{
				message = failure?.Message;
				return true;
			}
			return false;
		}

		public static implicit operator bool(Try t) => t.IsSuccess();
	}

	public class TrySuccess : Try { }

	public class TryFailure : Try
	{
		public string Message { get; }

		public TryFailure(string message) => Message = message;
	}
}
