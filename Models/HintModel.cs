using Newtonsoft.Json;

namespace WordGame.API.Models
{
	public class HintModel
	{
		[JsonConstructor]
		public HintModel(string hintWord, int wordCount)
		{
			HintWord = hintWord;
			WordCount = wordCount;
		}

		public string HintWord { get; }

		public int WordCount { get; }
	}
}
