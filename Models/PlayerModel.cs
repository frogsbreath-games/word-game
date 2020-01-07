using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordGame.API.Domain.Enums;

namespace WordGame.API.Models
{
	public class PlayerModel
	{
		public Team? Team { get; set; } = null;

		public string Name { get; set; } = null;

		public bool? IsSpyMaster { get; set; } = null;
	}
}
