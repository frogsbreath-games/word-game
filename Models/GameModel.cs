using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordGame.API.Application.Resources;
using WordGame.API.Domain.Enums;
using WordGame.API.Domain.Models;

namespace WordGame.API.Models
{
	public class GameModel
	{
		public GameModel(Game game, Player localPlayer)
			: this(game, localPlayer.Id) { }

		public GameModel(Game game, Guid localPlayerId)
		{
			if (game is null)
				throw new ArgumentNullException(nameof(game));

			var localPlayer = game.Players.Single(p => p.Id == localPlayerId);

			Code = game.Code;
			Status = game.Status;
			LocalPlayer = new PlayerModel(localPlayer);
			Players = game.Players.Select(p => new PlayerModel(p)).ToList();
			WordTiles = game.WordTiles.Select(wt => new WordTileModel(
				wt.Word,
				Status == GameStatus.InProgress && (localPlayer.Character?.Type == CharacterType.Cultist || wt.IsRevealed)
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
					localPlayer.Character?.Type == CharacterType.Cultist || currentTurn.Status != TurnStatus.PendingApproval
						? currentTurn.HintWord
						: null,
					localPlayer.Character?.Type == CharacterType.Cultist || currentTurn.Status != TurnStatus.PendingApproval
						? currentTurn.WordCount
						: null,
					currentTurn.EndTurnVotes,
					currentTurn.GuessesRemaining);
			}
			Actions = new GameActionsModel(game, localPlayer);
			Descriptions = new DescriptionModel(game, localPlayer);
			AvailableCharacters = CharacterList.Characters.Where(c => !game.Players.Any(p => p.Character?.Number == c.Number)).ToList();
		}

		public string Code { get; }

		public GameStatus Status { get; }

		public PlayerModel LocalPlayer { get; }

		public List<PlayerModel> Players { get; }

		public List<WordTileModel> WordTiles { get; }

		public int BlueTilesRemaining { get; }

		public int RedTilesRemaining { get; }

		public List<Character> AvailableCharacters { get; }

		public Team? WinningTeam { get; }

		public TurnModel? CurrentTurn { get; }

		public GameActionsModel Actions { get; }

		public DescriptionModel Descriptions { get; }
	}
}
