using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WordGame.API.Domain.Enums
{
	public enum TurnStatus
	{
		//Cultist is picking a word
		Planning,

		//Opposing Cultist is approving the word
		PendingApproval,

		//Researchers are guessing words
		Guessing,

		//Votes are in for a given guess
		Tallying,

		//Turn is over
		Over
	}
}
