using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using WordGame.API.Application.Services;
using WordGame.API.Domain.Enums;
using WordGame.API.Domain.Models;

namespace WordGame.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Produces("application/json"), Consumes("application/json")]
	public class TestController : ControllerBase
	{
		protected readonly IGameBoardGenerator _gameBoardGenerator;
		protected readonly INameGenerator _nameGenerator;

		public TestController(IGameBoardGenerator gameBoardGenerator, INameGenerator nameGenerator)
		{
			_gameBoardGenerator = gameBoardGenerator ?? throw new ArgumentNullException(nameof(gameBoardGenerator));
			_nameGenerator = nameGenerator ?? throw new ArgumentNullException(nameof(nameGenerator));
		}

		[HttpGet("randomName")]
		public string GetRandomName()
			=> _nameGenerator.GetRandomName();

		[HttpGet("randomGameBoard")]
		public List<WordTile> GetRandomGameBoard(Team team = Team.Red)
			=> _gameBoardGenerator.GenerateGameBoard(team);
	}
}