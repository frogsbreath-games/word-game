using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordGame.API.Domain.Enums;

namespace WordGame.API.Domain.Models
{
	public class GameEvent
	{
		protected GameEvent(Player player, DateTime timestamp, GameEventType type, IDictionary<string, object> data = null)
		{
			Player = player?.Name ?? throw new ArgumentNullException(nameof(player));
			Team = player.Team;
			Timestamp = timestamp.ToLongTimeString();
			Type = type;
			Data = data;
		}

		protected GameEvent(string player, Team team, DateTime timestamp, GameEventType type, IDictionary<string, object> data = null)
		{
			Player = player;
			Team = team;
			Timestamp = timestamp.ToLongTimeString();
			Type = type;
			Data = data;
		}

		protected GameEvent(Team team, DateTime timestamp, GameEventType type, IDictionary<string, object> data = null)
		{
			Team = team;
			Timestamp = timestamp.ToLongTimeString();
			Type = type;
			Data = data;
		}

		public Team Team { get; protected set; }
		public string Player { get; protected set; }
		public string Timestamp { get; protected set; }
		public GameEventType Type { get; protected set; }
		public IDictionary<string, object> Data { get; protected set; }
		public string Message => Type switch
		{
			GameEventType.PlayerStartedGame => " Started The Game",
			GameEventType.PlayerRestartedGameInLobby => " Returned Game To Lobby",
			GameEventType.PlayerApprovedHint => $" Approved Hint Word \"{Data["word"]}\" ({Data["count"]})",
			GameEventType.PlayerRefusedHint => $" Refused Hint Word \"{Data["word"]}\" ({Data["count"]})",
			GameEventType.PlayerGaveHint => $" Gave Hint Word \"{Data["word"]}\" ({Data["count"]})",
			GameEventType.PlayerVotedWord => $" Voted For \"{Data["word"]}\"",
			GameEventType.PlayerVotedEndTurn => $" Voted To Stop Guessing",
			GameEventType.PlayerMessage => $" {Data["message"]}",

			GameEventType.TeamGuessedCorrectly => $" Guessed Correctly!",
			GameEventType.TeamGuessedIncorrectly => $" Guessed Incorrectly 😥",
			GameEventType.TeamWon => " WON!",
			_ => null
		};

		public static GameEvent PlayerStartedGame(Player player, DateTime timestamp)
			=> new GameEvent(player, timestamp, GameEventType.PlayerStartedGame);

		public static GameEvent PlayerRestartedGameInLobby(Player player, DateTime timestamp)
			=> new GameEvent(player, timestamp, GameEventType.PlayerRestartedGameInLobby);

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

		public static GameEvent PlayerMessage(string player, Team team, DateTime timestamp, string message)
			=> new GameEvent(player, team, timestamp, GameEventType.PlayerMessage,
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
		//Player Action Events
		PlayerStartedGame,
		PlayerRestartedGameInLobby,
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
