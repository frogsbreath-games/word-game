using MongoDB.Bson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordGame.API.Domain.Enums;

namespace WordGame.API.Domain.Models
{
	public class Game
	{
		[JsonIgnore]
		public ObjectId Id { get; protected set; }

		[JsonIgnore]
		public DateTime CreatedDate { get; protected set; }

		public string Code { get; protected set; }

		public GameStatus Status { get; protected set; }

		public List<Player> Players { get; protected set; } = new List<Player>();

		public List<WordTile> WordTiles { get; protected set; } = new List<WordTile>();

		public int BlueTilesRemaining => WordTiles.Count(x => !x.IsRevealed && x.Team == Team.Blue);

		public int RedTilesRemaining => WordTiles.Count(x => !x.IsRevealed && x.Team == Team.Red);

		//This is pretty stupid
		public Team? GetWinningTeam() => Status == GameStatus.Lobby
			? null
			: BlueTilesRemaining == 0
				? Team.Blue
				: RedTilesRemaining == 0
					? Team.Red
					: Turns.Where(t => t.Team == Team.Red).SelectMany(t => t.Guesses.Where(g => g.Team == Team.Black)).Any()
						? Team.Blue
						: Turns.Where(t => t.Team == Team.Blue).SelectMany(t => t.Guesses.Where(g => g.Team == Team.Black)).Any()
							? Team.Red
							: (Team?)null;

		[JsonIgnore]
		public List<Turn> Turns { get; protected set; } = new List<Turn>();

		public Turn CurrentTurn => Turns.OrderBy(x => x.TurnNumber).LastOrDefault();

		public Game(
			string code,
			string adminName,
			Team adminTeam,
			bool adminIsSpyMaster,
			DateTime? createdDate = null,
			ObjectId? id = null)
		{
			Id = id ?? ObjectId.GenerateNewId();
			CreatedDate = createdDate ?? DateTime.Now;
			Code = code ?? throw new ArgumentNullException(nameof(code));
			Status = GameStatus.Lobby;
			AddPlayer(new Player(
				adminName,
				true,
				adminIsSpyMaster,
				0,
				adminTeam,
				createdDate: CreatedDate));
		}

		protected void AddPlayer(Player player)
		{
			Players.Add(player);
		}

		public Player AddNewPlayer(
			string name,
			bool isBot = false,
			Team? team = null,
			bool? isSpyMaster = null,
			int? number = null)
		{
			if (!number.HasValue)
			{
				number = Players.Max(x => x.Number) + 1;
			}

			if (!team.HasValue)
			{
				team = RedPlayers.Count() > BluePlayers.Count()
					? Team.Blue
					: Team.Red;
			}

			if (!isSpyMaster.HasValue)
			{
				isSpyMaster = team == Team.Red
					? !RedPlayers.Any(x => x.IsSpyMaster)
					: !BluePlayers.Any(x => x.IsSpyMaster);
			}

			var player = new Player(
				name,
				false,
				isSpyMaster.Value,
				number.Value,
				team.Value,
				isBot);

			AddPlayer(player);

			return player;
		}

		public void StartGame(List<WordTile> tiles, Team startingTeam)
		{
			if (!GameCanStart())
				throw new InvalidOperationException("Cannot start game!");

			if (tiles.Count != 25)
				throw new InvalidOperationException("Must start game with 25 words!");

			Status = GameStatus.InProgress;
			WordTiles = tiles;
			Turns = new List<Turn>
			{
				new Turn(startingTeam, 1)
			};
		}

		private void EndCurrentTurn()
		{
			if (Status != GameStatus.InProgress)
				throw new InvalidOperationException();

			CurrentTurn.End();

			if (!GetWinningTeam().HasValue)
				Turns.Add(new Turn(CurrentTurn.Team.GetOpposingTeam(), CurrentTurn.TurnNumber + 1));
		}

		public void SetPlayerVote(Player player, string word)
		{
			RemovePlayerVote(player);

			if (!(WordTiles.SingleOrDefault(t => t.Word == word) is WordTile tile))
				throw new InvalidOperationException();

			if (tile.IsRevealed)
				throw new InvalidOperationException();

			tile.Votes.Add(new PlayerVote(player.Team, player.Number, player.Name));

			if (tile.Votes.Count == Agents.Count(p => p.Team == player.Team))
			{
				CurrentTurn.Guesses.Add(new Guess(word, CurrentTurn.Guesses.Count, tile.Team));
				tile.Votes.Clear();
				tile.IsRevealed = true;

				if (tile.Team != player.Team || CurrentTurn.GuessesRemaining <= 0)
					EndCurrentTurn();
			}
		}

		public void VoteEndTurn(Player player)
		{
			RemovePlayerVote(player);

			CurrentTurn.EndTurnVotes.Add(new PlayerVote(player.Team, player.Number, player.Name));

			if (CurrentTurn.EndTurnVotes.Count == Agents.Count(p => p.Team == player.Team))
			{
				EndCurrentTurn();
			}
		}

		public void RemovePlayerVote(Player player)
		{
			if (Status != GameStatus.InProgress)
				throw new InvalidOperationException();

			if (CurrentTurn?.Status != TurnStatus.Guessing)
				throw new InvalidOperationException();

			if (CurrentTurn?.Team != player.Team)
				throw new InvalidOperationException();

			foreach (var tile in WordTiles)
			{
				if (tile.Votes.SingleOrDefault(v => v.Number == player.Number) is PlayerVote wordVote)
					tile.Votes.Remove(wordVote);
			}

			if (CurrentTurn.EndTurnVotes.SingleOrDefault(v => v.Number == player.Number) is PlayerVote endVote)
				CurrentTurn.EndTurnVotes.Remove(endVote);
		}

		[JsonIgnore]
		public IEnumerable<Player> BluePlayers => Players.Where(x => x.Team == Team.Blue);

		[JsonIgnore]
		public IEnumerable<Player> RedPlayers => Players.Where(x => x.Team == Team.Red);

		[JsonIgnore]
		public IEnumerable<Player> SpyMasters => Players.Where(x => x.IsSpyMaster);

		[JsonIgnore]
		public IEnumerable<Player> Agents => Players.Where(x => !x.IsSpyMaster);

		//This is sort of inefficient but whatever
		public bool GameCanStart()
		{
			if (Status != GameStatus.Lobby)
				return false;

			if (RedPlayers.Count(x => x.IsSpyMaster) != 1)
				return false;

			if (BluePlayers.Count(x => x.IsSpyMaster) != 1)
				return false;

			if (RedPlayers.Count() < 2)
				return false;

			if (BluePlayers.Count() < 2)
				return false;

			if (Math.Abs(RedPlayers.Count() - BluePlayers.Count()) > 1)
				return false;

			if (Players.Count != (RedPlayers.Count() + BluePlayers.Count()))
				return false;

			return true;
		}
	}
}
