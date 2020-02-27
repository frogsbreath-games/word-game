using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordGame.API.Domain.Enums;
using WordGame.API.Domain.Models;
using WordGame.API.Functional;

namespace WordGame.API.Domain
{
	public static class GameExtensions
	{
		public static Try CanGenerateBoard(this Game game, Player player)
		{
			if (!game.BoardCanBeGenerated())
				return Try.Failure("Cannot generated board.");

			if (player.Role != UserRole.Organizer)
				return Try.Failure("Only the organizer can generate the board.");

			return Try.Success;
		}

		public static Try CanReplaceWord(this Game game, Player player)
		{
			if (game.Status != GameStatus.BoardReview)
				return Try.Failure("Words cannot be replaced.");

			if (player.Role != UserRole.Organizer)
				return Try.Failure("Only the organizer can replace words.");

			return Try.Success;
		}

		public static Try CanStart(this Game game, Player player)
		{
			if (game.Status != GameStatus.BoardReview)
				return Try.Failure("Game cannot start");

			if (player.Role != UserRole.Organizer)
				return Try.Failure("Only the organizer start the game");

			return Try.Success;
		}

		public static Try CanRestart(this Game game, Player player)
		{
			if (!game.GetWinningTeam().HasValue)
				return Try.Failure("Cannot restart the game until it is over");

			if (game.Status != GameStatus.InProgress)
				return Try.Failure("Cannot restart game that has been archived");

			if (player.Role != UserRole.Organizer)
				return Try.Failure("Only the organizer restart the game");

			return Try.Success;
		}

		public static Try CanDelete(this Game game, Player player)
		{
			if (player.Role != UserRole.Organizer)
				return Try.Failure("Only the organizer can delete game.");

			return Try.Success;
		}

		public static Try CanAddBot(this Game game, Player player)
		{
			if (game.Status != GameStatus.Lobby)
				return Try.Failure("Cannot add bot to game that is not in the lobby.");

			if (game.Players.Count >= 10)
				return Try.Failure("Cannot add bot to game with 10 or more players.");

			if (player.Role != UserRole.Organizer)
				return Try.Failure("Only the organizer can add bots");

			return Try.Success;
		}

		public static Try CanDeleteBot(this Game game, Player player)
		{
			if (game.Status != GameStatus.Lobby)
				return Try.Failure($"Cannot delete bot from game that is not in the lobby.");

			if (player.Role != UserRole.Organizer)
				return Try.Failure("Only the organizer can add bots");

			return Try.Success;
		}

		public static Try CanGiveHint(this Game game, Player player)
		{
			if (game.Status != GameStatus.InProgress)
				return Try.Failure($"Cannot give hint in game that isn't in progress.");

			if (game.CurrentTurn.Status != TurnStatus.Planning)
				return Try.Failure($"Cannot give hint outside the planning stage of the current turn.");

			if (player.Character?.Type != CharacterType.Cultist || player.Team != game.CurrentTurn.Team)
				return Try.Failure("This player cannot give a hint!");

			return Try.Success;
		}

		public static Try CanReviewHint(this Game game, Player player)
		{
			if (game.Status != GameStatus.InProgress)
				return Try.Failure($"Cannot review hint in game that isn't in progress.");

			if (game.CurrentTurn.Status != TurnStatus.PendingApproval)
				return Try.Failure($"Cannot review hint outside the pending approval stage of the current turn.");

			if (player.Character?.Type != CharacterType.Cultist || player.Team == game.CurrentTurn.Team)
				return Try.Failure("This player cannot review a hint!");

			return Try.Success;
		}

		public static Try CanVote(this Game game, Player player)
		{
			if (game.Status != GameStatus.InProgress)
				return Try.Failure($"Cannot cast word vote in game that isn't in progress.");

			if (game.CurrentTurn.Status != TurnStatus.Guessing)
				return Try.Failure($"Cannot cast word vote outside the guessing stage of the current turn.");

			if (player.Character?.Type != CharacterType.Researcher || player.Team != game.CurrentTurn.Team)
				return Try.Failure("This player cannot vote for a word!");

			return Try.Success;
		}

		public static IEnumerable<Player> FilterByTeam(this IEnumerable<Player> players, Team team)
			=> players.Where(p => p.Team == team);

		public static IEnumerable<Player> FilterByRole(this IEnumerable<Player> players, UserRole role)
			=> players.Where(p => p.Role == role);

		public static IEnumerable<Player> FilterByType(this IEnumerable<Player> players, CharacterType type)
			=> players.Where(p => p.Character?.Type == type);
	}
}
