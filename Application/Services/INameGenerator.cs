using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordGame.API.Application.Resources;

namespace WordGame.API.Application.Services
{
	public interface INameGenerator
	{
		string GetRandomName();
	}

	public class NameGenerator : INameGenerator
	{
		public string GetRandomName()
		{
			Random rand = new Random();

			var adjective = AdjectiveList.Adjectives[rand.Next(0, AdjectiveList.Adjectives.Length)];
			var animal = AnimalList.Animals[rand.Next(0, AnimalList.Animals.Length)];

			return $"{adjective.First().ToString().ToUpper()}{adjective.Substring(1)} {animal}";
		}
	}
}
