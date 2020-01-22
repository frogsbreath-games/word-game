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
			GameEventType.GameStarted => " Started The Game",
			GameEventType.HintApproved => $" Approved Hint Word \"{Data["word"]}\" ({Data["count"]})",
			GameEventType.HintGiven => $" Gave Hint Word \"{Data["word"]}\" ({Data["count"]})",
			GameEventType.Guessed => $" Guessed \"{Data["word"]}\"",
			GameEventType.VotedEndTurn => $" Voted To Stop Guessing",
			_ => null
		};

		public static GameEvent GameStarted(Player player, DateTime timestamp)
			=> new GameEvent(player, timestamp, GameEventType.GameStarted);

		public static GameEvent HintApproved(Player player, DateTime timestamp, string word, int count)
			=> new GameEvent(player, timestamp, GameEventType.HintApproved,
				new Dictionary<string, object>
				{
					["word"] = word,
					["count"] = count
				});

		public static GameEvent HintGiven(Player player, DateTime timestamp, string word, int count)
			=> new GameEvent(player, timestamp, GameEventType.HintGiven,
				new Dictionary<string, object>
				{
					["word"] = word,
					["count"] = count
				});

		public static GameEvent Guessed(Player player, DateTime timestamp, string word)
			=> new GameEvent(player, timestamp, GameEventType.Guessed,
				new Dictionary<string, object>
				{
					["word"] = word
				});

		public static GameEvent VotedEndTurn(Player player, DateTime timestamp)
			=> new GameEvent(player, timestamp, GameEventType.VotedEndTurn);
	}

	public enum GameEventType
	{
		GameStarted,
		HintApproved,
		HintGiven,
		Guessed,
		VotedEndTurn
	}
}
