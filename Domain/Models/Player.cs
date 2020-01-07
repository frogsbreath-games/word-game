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
			bool isAdmin,
			bool isSpyMaster,
			int number,
			Team team,
			Guid? id = null,
			DateTime? createdDate = null)
		{
			Id = id ?? Guid.NewGuid();
			CreatedDate = createdDate ?? DateTime.Now;
			Name = name ?? throw new ArgumentNullException(nameof(name));
			IsAdmin = isAdmin;
			IsSpyMaster = isSpyMaster;
			Team = team;
			Number = number;
		}

		[JsonIgnore]
		public Guid Id { get; protected set; }

		[JsonIgnore]
		public DateTime CreatedDate { get; protected set; }

		public int Number { get; protected set; }

		public string Name { get; protected set; }

		public bool IsAdmin { get; protected set; }

		public bool IsSpyMaster { get; protected set; }

		public Team Team { get; protected set; }

		public void UpdatePlayer(
			Team? team = null,
			string name = null,
			bool? isSpyMaster = null)
		{
			if (team.HasValue)
				Team = team.Value;

			if (name != null)
				Name = name;

			if (isSpyMaster.HasValue)
				IsSpyMaster = isSpyMaster.Value;
		}
	}
}
