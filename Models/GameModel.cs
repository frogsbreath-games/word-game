using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordGame.API.Domain.Enums;
using WordGame.API.Domain.Models;

namespace WordGame.API.Models
{
	public class GameModel
	{
		public GameModel(Game game, Guid localPlayerId)
		{
			if (game is null)
				throw new ArgumentNullException(nameof(game));

			Code = game.Code;
			Status = game.Status;
			LocalPlayer = game.Players.Single(p => p.Id == localPlayerId);
			Players = game.Players;
			WordTiles = game.WordTiles.Select(wt => new WordTileModel(
				wt.Word,
				LocalPlayer.IsSpyMaster || wt.IsRevealed
					? wt.Team
					: Team.Unknown,
				wt.IsRevealed,
				wt.Votes)).ToList();

			BlueTilesRemaining = game.BlueTilesRemaining;
			RedTilesRemaining = game.RedTilesRemaining;
			WinningTeam = game.GetWinningTeam();
			if (game.CurrentTurn is Turn currentTurn)
			{
				CurrentTurn = new TurnModel(
					currentTurn.Team,
					currentTurn.TurnNumber,
					currentTurn.Status,
					LocalPlayer.IsSpyMaster || currentTurn.Status != TurnStatus.PendingApproval
						? currentTurn.HintWord
						: null,
					LocalPlayer.IsSpyMaster || currentTurn.Status != TurnStatus.PendingApproval
						? currentTurn.WordCount
						: null,
					currentTurn.EndTurnVotes,
					currentTurn.GuessesRemaining);
			}
			Actions = new GameActionsModel(game, LocalPlayer);
			Descriptions = new DescriptionModel(game, LocalPlayer);
		}

		public string Code { get; }

		public GameStatus Status { get; }

		public Player LocalPlayer { get; }

		public List<Player> Players { get; }

		public List<WordTileModel> WordTiles { get; }

		public int BlueTilesRemaining { get; }

		public int RedTilesRemaining { get; }

		public Team? WinningTeam { get; }

		public TurnModel CurrentTurn { get; }

		public GameActionsModel Actions { get; }
		public DescriptionModel Descriptions { get; }
	}
}
