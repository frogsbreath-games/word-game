using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
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
		}

		[JsonIgnore]
		public Guid Id { get; protected set; }

		[JsonIgnore]
		public DateTime CreatedDate { get; protected set; }

		public string Name { get; protected set; }

		public bool IsAdmin { get; protected set; }

		public bool IsSpyMaster { get; protected set; }

		public Team Team { get; protected set; }
	}
}
