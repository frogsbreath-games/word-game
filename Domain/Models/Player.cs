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
			string nickName,
			bool isAdminFlag,
			bool isSpyMasterFlag,
			Team team,
			Guid? id = null,
			DateTime? createdDate = null)
		{
			Id = id ?? Guid.NewGuid();
			CreatedDate = createdDate ?? DateTime.Now;
			NickName = nickName ?? throw new ArgumentNullException(nameof(nickName));
			IsAdminFlag = isAdminFlag;
			IsSpyMasterFlag = isSpyMasterFlag;
			Team = team;
		}

		[JsonIgnore]
		public Guid Id { get; protected set; }

		[JsonIgnore]
		public DateTime CreatedDate { get; protected set; }

		public string NickName { get; protected set; }

		public bool IsAdminFlag { get; protected set; }

		public bool IsSpyMasterFlag { get; protected set; }

		public Team Team { get; protected set; }
	}
}
