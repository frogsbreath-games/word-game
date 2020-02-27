using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordGame.API.Domain.Enums;

namespace WordGame.API.Domain.Models
{
	public class Player
	{
		public Player(
			string name,
			Character character,
			UserRole role,
			int number,
			Guid? id = null,
			DateTime? createdDate = null)
		{
			Id = id ?? Guid.NewGuid();
			CreatedDate = createdDate ?? DateTime.Now;
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Character = character;
			Role = role;
			Team = character.Team;
			Number = number;
		}

		public Player(
			string name,
			Team team,
			UserRole role,
			int number,
			Guid? id = null,
			DateTime? createdDate = null)
		{
			Id = id ?? Guid.NewGuid();
			CreatedDate = createdDate ?? DateTime.Now;
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Character = null;
			Role = role;
			Team = team;
			Number = number;
		}

		[JsonIgnore]
		public Guid Id { get; protected set; }

		[JsonIgnore]
		public DateTime CreatedDate { get; protected set; }

		public int Number { get; protected set; }

		public string Name { get; protected set; }

		public Character? Character { get; protected set; }

		public UserRole Role { get; protected set; }

		public Team Team { get; protected set; }

		public void UpdateCharacter(Character character)
		{
			Character = character;
			Team = character.Team;
		}

		public void ClearCharacter()
		{
			Character = null;
		}

		public void UpdateTeam(Team team)
		{
			Team = team;
			Character = null;
		}
	}
}
