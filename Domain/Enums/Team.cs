using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WordGame.API.Domain.Enums
{
	public enum Team
	{
		Red,
		Blue,
		Black,
		Neutral,
		Unknown
	}

	public static class TeamEx
	{
		public static Team GetOpposingTeam(this Team team)
		{
			return team switch
			{
				Team.Red => Team.Blue,
				Team.Blue => Team.Red,
				_ => team
			};
		}
	}
}
