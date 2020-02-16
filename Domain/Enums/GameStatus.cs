using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WordGame.API.Domain.Enums
{
	public enum GameStatus
	{
		Lobby,
		BoardReview,
		InProgress,
		PostGame,
		Archived
	}
}
