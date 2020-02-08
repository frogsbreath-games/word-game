using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WordGame.API.Models
{
	public class VoteModel
	{
		[JsonConstructor]
		public VoteModel(string word)
			=> Word = word;

		public string Word { get; }
	}
}
