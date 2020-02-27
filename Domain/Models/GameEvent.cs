using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordGame.API.Domain.Enums;

namespace WordGame.API.Domain.Models
{
	public class GameEvent
	{
		protected GameEvent(Player player, DateTime timestamp, GameEventType type, IDictionary<string, object>? data = null)
		{
			Player = player?.Name ?? throw new ArgumentNullException(nameof(player));
			Team = player.Team;
			Timestamp = timestamp.ToLongTimeString();
			Type = type;
			Data = data ?? new Dictionary<string, object>();
		}

		protected GameEvent(string player, Team team, DateTime timestamp, GameEventType type, IDictionary<string, object>? data = null)
		{
			Player = player;
			Team = team;
			Timestamp = timestamp.ToLongTimeString();
			Type = type;
			Data = data ?? new Dictionary<string, object>();
		}

		protected GameEvent(Team team, DateTime timestamp, GameEventType type, IDictionary<string, object>? data = null)
		{
			Team = team;
			Timestamp = timestamp.ToLongTimeString();
			Type = type;
			Data = data ?? new Dictionary<string, object>();
		}

		public Team Team { get; protected set; }
		public string? Player { get; protected set; }
		public string Timestamp { get; protected set; }
		public GameEventType Type { get; protected set; }
		public IDictionary<string, object> Data { get; protected set; }
		public string Message => Type switch
		{
			GameEventType.OrganizerStartedGame => " Started The Game",
			GameEventType.OrganizerGeneratedBoard => " Generated The Game Board",
			GameEventType.OrganizerRestartedGameInLobby => " Returned Game To Lobby",
			GameEventType.OrganizerReplacedWord => $" Replaced \"{Data["oldWord"]}\" with \"{Data["newWord"]}\"",

			GameEventType.PlayerJoinedGame => " Joined The Game",
			GameEventType.PlayerChangedTeam => $" Switched to Team {Data["team"]}",
			GameEventType.PlayerChangedCharacter => $" Locked in '{Data["character"]}'",
			GameEventType.PlayerClearedCharacter => $" Went Back to Picking a Character",
			GameEventType.PlayerApprovedHint => $" Approved Hint Word \"{Data["word"]}\" ({Data["count"]})",
			GameEventType.PlayerRefusedHint => $" Refused Hint Word \"{Data["word"]}\" ({Data["count"]})",
			GameEventType.PlayerGaveHint => $" Gave Hint Word \"{Data["word"]}\" ({Data["count"]})",
			GameEventType.PlayerVotedWord => $" Voted For \"{Data["word"]}\"",
			GameEventType.PlayerVotedEndTurn => $" Voted To Stop Guessing",
			GameEventType.PlayerMessage => $" {Data["message"]}",

			GameEventType.TeamGuessedCorrectly => $" Guessed Correctly!",
			GameEventType.TeamGuessedIncorrectly => $" Guessed Incorrectly 😥",
			GameEventType.TeamWon => " WON!",
			_ => string.Empty
		};

		public static GameEvent OrganizerGeneratedBoard(DateTime timestamp)
			=> new GameEvent("The Organizer", Team.Neutral, timestamp, GameEventType.OrganizerGeneratedBoard);

		public static GameEvent OrganizerStartedGame(DateTime timestamp)
			=> new GameEvent("The Organizer", Team.Neutral, timestamp, GameEventType.OrganizerStartedGame);

		public static GameEvent OrganizerRestartedGameInLobby(DateTime timestamp)
			=> new GameEvent("The Organizer", Team.Neutral, timestamp, GameEventType.OrganizerRestartedGameInLobby);

		public static GameEvent OrganizerReplacedWord(DateTime timestamp, string oldWord, string newWord)
			=> new GameEvent("The Organizer", Team.Neutral, timestamp, GameEventType.OrganizerReplacedWord,
				new Dictionary<string, object>
				{
					["oldWord"] = oldWord,
					["newWord"] = newWord
				});

		public static GameEvent PlayerJoinedGame(Player player, DateTime timestamp)
			=> new GameEvent(player, timestamp, GameEventType.PlayerJoinedGame);

		public static GameEvent PlayerChangedTeam(Player player, Team team, DateTime timestamp)
			=> new GameEvent(player, timestamp, GameEventType.PlayerChangedTeam,
				new Dictionary<string, object>
				{
					["team"] = team
				});

		public static GameEvent PlayerChangedCharacter(Player player, string character, DateTime timestamp)
			=> new GameEvent(player, timestamp, GameEventType.PlayerChangedCharacter,
				new Dictionary<string, object>
				{
					["character"] = character
				});

		public static GameEvent PlayerClearedCharacter(Player player, DateTime timestamp)
			=> new GameEvent(player, timestamp, GameEventType.PlayerClearedCharacter);

		public static GameEvent PlayerApprovedHint(Player player, DateTime timestamp, string word, int count)
			=> new GameEvent(player, timestamp, GameEventType.PlayerApprovedHint,
				new Dictionary<string, object>
				{
					["word"] = word,
					["count"] = count
				});

		public static GameEvent PlayerRefusedHint(Player player, DateTime timestamp, string word, int count)
			=> new GameEvent(player, timestamp, GameEventType.PlayerRefusedHint,
				new Dictionary<string, object>
				{
					["word"] = word,
					["count"] = count
				});

		public static GameEvent PlayerGaveHint(Player player, DateTime timestamp, string word, int count)
			=> new GameEvent(player, timestamp, GameEventType.PlayerGaveHint,
				new Dictionary<string, object>
				{
					["word"] = word,
					["count"] = count
				});

		public static GameEvent PlayerVotedWord(Player player, DateTime timestamp, string word)
			=> new GameEvent(player, timestamp, GameEventType.PlayerVotedWord,
				new Dictionary<string, object>
				{
					["word"] = word
				});

		public static GameEvent PlayerVotedEndTurn(Player player, DateTime timestamp)
			=> new GameEvent(player, timestamp, GameEventType.PlayerVotedEndTurn);

		public static GameEvent PlayerMessage(Player player, DateTime timestamp, string message)
			=> new GameEvent(player, timestamp, GameEventType.PlayerMessage,
			new Dictionary<string, object>
			{
				["message"] = message
			});

		public static GameEvent TeamGuessedCorrectly(Team team, DateTime timestamp)
			=> new GameEvent(team, timestamp, GameEventType.TeamGuessedCorrectly);

		public static GameEvent TeamGuessedIncorrectly(Team team, DateTime timestamp)
			=> new GameEvent(team, timestamp, GameEventType.TeamGuessedIncorrectly);

		public static GameEvent TeamWon(Team team, DateTime timestamp)
			=> new GameEvent(team, timestamp, GameEventType.TeamWon);
	}

	public enum GameEventType
	{
		//Organizer Events
		OrganizerGeneratedBoard,
		OrganizerStartedGame,
		OrganizerRestartedGameInLobby,
		OrganizerReplacedWord,

		//Player Action Events
		PlayerJoinedGame,
		PlayerChangedTeam,
		PlayerChangedCharacter,
		PlayerClearedCharacter,
		PlayerApprovedHint,
		PlayerRefusedHint,
		PlayerGaveHint,
		PlayerVotedWord,
		PlayerVotedEndTurn,
		PlayerMessage,

		//Team Events
		TeamGuessedCorrectly,
		TeamGuessedIncorrectly,
		TeamWon
	}
}
