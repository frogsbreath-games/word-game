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
			GameEventType.PlayerApprovedHint => $" Approved Hint Word \"{Data["word"]}\" ({Data["count"]})",
			GameEventType.PlayerGaveHint => $" Gave Hint Word \"{Data["word"]}\" ({Data["count"]})",
			GameEventType.PlayerVotedWord => $" Voted For \"{Data["word"]}\"",
			GameEventType.PlayerVotedEndTurn => $" Voted To Stop Guessing",

			GameEventType.TeamGuessedCorrectly => $" Guessed Correctly!",
			GameEventType.TeamGuessedIncorrectly => $" Guessed Incorrectly :(",
			GameEventType.TeamWon => " WON!",
			_ => null
		};

		public static GameEvent PlayerStartedGame(Player player, DateTime timestamp)
			=> new GameEvent(player, timestamp, GameEventType.PlayerStartedGame);

		public static GameEvent PlayerApprovedHint(Player player, DateTime timestamp, string word, int count)
			=> new GameEvent(player, timestamp, GameEventType.PlayerApprovedHint,
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
		PlayerApprovedHint,
		PlayerGaveHint,
		PlayerVotedWord,
		PlayerVotedEndTurn,

		//Team Events
		TeamGuessedCorrectly,
		TeamGuessedIncorrectly,
		TeamWon
	}
}
