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
		private List<GameEvent> _publicEvents = new List<GameEvent>();
		private List<GameEvent> _cultistEvents = new List<GameEvent>();

		[JsonIgnore]
		public ObjectId Id { get; protected set; }

		[JsonIgnore]
		public DateTime CreatedDate { get; protected set; }

		public string Code { get; protected set; }

		public GameStatus Status { get; protected set; }

		public List<Player> Players { get; protected set; } = new List<Player>();

		public List<WordTile> WordTiles { get; protected set; } = new List<WordTile>();

		public int BlueTilesRemaining => GetTilesRemaining(Team.Blue);

		public int RedTilesRemaining => GetTilesRemaining(Team.Red);

		public int GetTilesRemaining(Team team)
			=> WordTiles.Count(x => !x.IsRevealed && x.Team == team);

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
			DateTime? createdDate = null,
			ObjectId? id = null)
		{
			Id = id ?? ObjectId.GenerateNewId();
			CreatedDate = createdDate ?? DateTime.Now;
			Code = code?.ToUpper() ?? throw new ArgumentNullException(nameof(code));
			Status = GameStatus.Lobby;
			AddPlayer(new Player(
				adminName,
				adminTeam,
				UserRole.Organizer,
				0,
				createdDate: CreatedDate));
		}

		protected void AddPlayer(Player player)
		{
			Players.Add(player);
		}

		public Player AddNewPlayer(
			string name,
			UserRole role = UserRole.Player,
			Team? team = null,
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

			var player = new Player(
				name,
				team.Value,
				role,
				number.Value);

			AddPlayer(player);

			AddPublicEvent(GameEvent.PlayerJoinedGame(player, DateTime.Now));

			return player;
		}

		public void UpdatePlayerCharacter(Player player, Character character)
		{
			if (Players.Any(p => p.Character?.Number == character.Number))
				throw new InvalidOperationException();

			player.UpdateCharacter(character);

			AddPublicEvent(GameEvent.PlayerChangedCharacter(player, character.Name, DateTime.Now));
		}

		public void ClearPlayerCharacter(Player player)
		{
			player.ClearCharacter();

			AddPublicEvent(GameEvent.PlayerClearedCharacter(player, DateTime.Now));
		}

		public void UpdatePlayerTeam(Player player, Team team)
		{
			if (player.Team != team)
			{
				player.UpdateTeam(team);

				AddPublicEvent(GameEvent.PlayerChangedTeam(player, team, DateTime.Now));
			}
		}

		public void GenerateBoard(Player player, List<WordTile> tiles)
		{
			if (this.CanGenerateBoard(player).IsFailure(out var message))
				throw new InvalidOperationException(message);

			if (tiles.Count != 24)
				throw new InvalidOperationException("Must generate board with 25 words!");

			Status = GameStatus.BoardReview;
			WordTiles = tiles;

			AddPublicEvent(GameEvent.OrganizerGeneratedBoard(DateTime.Now));
		}

		public void ReplaceWord(Player player, WordTile tile, string newWord)
		{
			if (this.CanReplaceWord(player).IsFailure(out var message))
				throw new InvalidOperationException(message);

			AddPublicEvent(GameEvent.OrganizerReplacedWord(DateTime.Now, tile.Word, newWord));

			tile.ReplaceWord(newWord);
		}

		public void StartGame(Player player)
		{
			if (this.CanStart(player).IsFailure(out var message))
				throw new InvalidOperationException(message);

			var startingTeam = BlueTilesRemaining > RedTilesRemaining
				? Team.Blue
				: Team.Red;

			Status = GameStatus.InProgress;
			Turns = new List<Turn>
			{
				new Turn(startingTeam, 1)
			};

			AddPublicEvent(GameEvent.OrganizerStartedGame(DateTime.Now));
		}

		public void BackToLobby(Player player)
		{
			if (this.CanRestart(player).IsFailure(out var message))
				throw new InvalidOperationException(message);

			Turns.Clear();
			WordTiles.Clear();
			Status = GameStatus.Lobby;

			AddPublicEvent(GameEvent.OrganizerRestartedGameInLobby(DateTime.Now));
		}

		private void AddPublicEvent(GameEvent @event)
		{
			_publicEvents ??= new List<GameEvent>();
			_publicEvents.Add(@event);
		}

		private void AddCultistEvent(GameEvent @event)
		{
			_cultistEvents ??= new List<GameEvent>();
			_cultistEvents.Add(@event);
		}

		private void EndCurrentTurn()
		{
			if (Status != GameStatus.InProgress)
				throw new InvalidOperationException();

			CurrentTurn.End();

			if (GetWinningTeam() is Team winningTeam)
				AddPublicEvent(GameEvent.TeamWon(winningTeam, DateTime.Now));
			else
				Turns.Add(new Turn(CurrentTurn.Team.GetOpposingTeam(), CurrentTurn.TurnNumber + 1));
		}

		public void SetPlayerVote(Player player, WordTile tile)
		{
			RemovePlayerVote(player);

			if (tile.IsRevealed)
				throw new InvalidOperationException();

			tile.Votes.Add(new PlayerVote(player.Team, player.Number, player.Name));

			AddPublicEvent(GameEvent.PlayerVotedWord(player, DateTime.Now, tile.Word));

			if (tile.Votes.Count == Researchers.Count(p => p.Team == player.Team))
			{
				CurrentTurn.SetToTallying();
			}
		}

		public void VoteEndTurn(Player player)
		{
			RemovePlayerVote(player);

			CurrentTurn.EndTurnVotes.Add(new PlayerVote(player.Team, player.Number, player.Name));

			AddPublicEvent(GameEvent.PlayerVotedEndTurn(player, DateTime.Now));

			if (CurrentTurn.EndTurnVotes.Count == Researchers.Count(p => p.Team == player.Team))
			{
				CurrentTurn.SetToTallying();
			}
		}

		public void TallyVotes()
		{
			if (Status != GameStatus.InProgress)
				throw new InvalidOperationException();

			if (CurrentTurn?.Status != TurnStatus.Tallying)
				throw new InvalidOperationException();

			if (CurrentTurn.EndTurnVotes.Count == Researchers.Count(p => p.Team == CurrentTurn.Team))
			{
				EndCurrentTurn();
			}
			else if (WordTiles.SingleOrDefault(t => t.Votes.Count == Researchers.Count(p => p.Team == CurrentTurn.Team)) is WordTile tile)
			{
				CurrentTurn.Guesses.Add(new Guess(tile.Word, CurrentTurn.Guesses.Count, tile.Team));
				tile.Votes.Clear();
				tile.IsRevealed = true;

				if (tile.Team == CurrentTurn.Team)
				{
					AddPublicEvent(GameEvent.TeamGuessedCorrectly(CurrentTurn.Team, DateTime.Now));

					if (CurrentTurn.GuessesRemaining <= 0
						|| GetTilesRemaining(CurrentTurn.Team) <= 0)
						EndCurrentTurn();
					else
						CurrentTurn.GuessAgain();
				}
				else
				{
					AddPublicEvent(GameEvent.TeamGuessedIncorrectly(CurrentTurn.Team, DateTime.Now));
					EndCurrentTurn();
				}
			}
			else
				throw new InvalidOperationException();
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

		public void GiveHint(Player player, string hintWord, int wordCount)
		{
			CurrentTurn.GiveHint(hintWord, wordCount);

			AddCultistEvent(GameEvent.PlayerGaveHint(player, DateTime.Now, hintWord, wordCount));
		}

		public void ApproveHint(Player player)
		{
			CurrentTurn.ApproveHint();

			AddPublicEvent(GameEvent.PlayerApprovedHint(player, DateTime.Now, CurrentTurn.HintWord!, CurrentTurn.WordCount!.Value));
		}

		public void RefuseHint(Player player)
		{
			string word = CurrentTurn.HintWord!;
			int count = CurrentTurn.WordCount!.Value;
			CurrentTurn.RefuseHint();

			AddCultistEvent(GameEvent.PlayerRefusedHint(player, DateTime.Now, word, count));
		}

		[JsonIgnore]
		public IEnumerable<Player> BluePlayers => Players.FilterByTeam(Team.Blue);

		[JsonIgnore]
		public IEnumerable<Player> RedPlayers => Players.FilterByTeam(Team.Red);

		[JsonIgnore]
		public IEnumerable<Player> Cultists => Players.FilterByType(CharacterType.Cultist);

		[JsonIgnore]
		public IEnumerable<Player> Researchers => Players.FilterByType(CharacterType.Researcher);

		//This is sort of inefficient but whatever
		public bool BoardCanBeGenerated()
		{
			if (Status != GameStatus.Lobby)
				return false;

			if (RedPlayers.Any(p => p.Character == null))
				return false;

			if (BluePlayers.Any(p => p.Character == null))
				return false;

			if (RedPlayers.FilterByType(CharacterType.Cultist).Count() != 1)
				return false;

			if (BluePlayers.FilterByType(CharacterType.Cultist).Count() != 1)
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

		public List<GameEvent> DispatchPublicEvents()
		{
			_publicEvents ??= new List<GameEvent>();
			var events = new List<GameEvent>(_publicEvents);
			_publicEvents.Clear();
			return events;
		}

		public List<GameEvent> DispatchCultistEvents()
		{
			_cultistEvents ??= new List<GameEvent>();
			var events = new List<GameEvent>(_cultistEvents);
			_cultistEvents.Clear();
			return events;
		}
	}
}
