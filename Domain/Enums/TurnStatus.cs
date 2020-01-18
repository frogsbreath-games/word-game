﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WordGame.API.Domain.Enums
{
	public enum TurnStatus
	{
		//Spy master is picking a word
		Planning,

		//Opposing Spy master is approving the word
		PendingApproval,

		//Agents are guessing words
		Guessing,

		//Turn is over
		Over
	}
}
