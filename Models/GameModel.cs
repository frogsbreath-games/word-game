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
			CurrentTurn = game.CurrentTurn;
			Actions = new GameActionsModel(game, LocalPlayer);
		}

		public string Code { get; }

		public GameStatus Status { get; }

		public Player LocalPlayer { get; }

		public List<Player> Players { get; }

		public List<WordTileModel> WordTiles { get; }

		public int BlueTilesRemaining { get; }

		public int RedTilesRemaining { get; }

		public Team? WinningTeam { get; }

		public Turn CurrentTurn { get; }

		public GameActionsModel Actions { get; }
	}
}
