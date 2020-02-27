using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordGame.API.Domain.Enums;
using WordGame.API.Domain.Models;

namespace WordGame.API.Models
{
	public class DescriptionModel
	{
		public DescriptionModel(Game game, Player localPlayer)
		{
			if (game.Status == GameStatus.Lobby)
			{
				Status = "Organizing Lobby";
				StatusDescription = "The game will start when there are enough players on both teams and the organizer starts the game.";

				if (localPlayer.Role == UserRole.Organizer)
				{
					LocalPlayerInstruction = "The game cannot be started until both teams are even and there is a Cultist for each. When you have all your players please ensure the teams are set up correctly and start the game!";
				}
				else
				{
					LocalPlayerInstruction = $"You have joined the Lobby: {game.Code}. Wait for the game organizer to start the game.";
				}
			}

			if (game.Status == GameStatus.BoardReview)
			{
				Status = "Game Board Review 🕵️‍♂️";
				StatusDescription = "The game board is being reviewed.";
			}

			if (game.Status == GameStatus.InProgress)
			{
				if (game.CurrentTurn.Status == TurnStatus.Planning)
				{
					Status = "Hint Planning Phase 🤔";
					StatusDescription = $"{game.CurrentTurn.Team} Team Cultist is formulating a hint.";
					if (game.CurrentTurn.Team == localPlayer.Team)
					{
						if (localPlayer.Character?.Type == CharacterType.Cultist)
						{
							LocalPlayerInstruction = "Create a hint and clue number for your team!";
						}
						else
						{
							LocalPlayerInstruction = "Review the board and wait for the Cultist to give you a hint.";
						}
					}
				}

				if (game.CurrentTurn.Status == TurnStatus.PendingApproval)
				{
					Status = "Hint Pending Approval 📋";
					StatusDescription = $"{TeamEx.GetOpposingTeam(game.CurrentTurn.Team)} Team Cultist is approving a hint.";
					if (game.CurrentTurn.Team == localPlayer.Team)
					{
						if (localPlayer.Character?.Type == CharacterType.Cultist)
						{
							LocalPlayerInstruction = "Wait for the opposing Cultist to approve your hint! ⏳";
						}
						else
						{
							LocalPlayerInstruction = "Review the board and wait for the Cultist to give you a hint.";
						}
					}
					else
					{
						if (localPlayer.Character?.Type == CharacterType.Cultist)
						{
							LocalPlayerInstruction = "Review the hint and approve or deny it!";
						}
						else
						{
							LocalPlayerInstruction = "Review the board and be polite as you wait for your team's turn 🙄⏲";
						}
					}
				}

				if (game.CurrentTurn.Status == TurnStatus.Guessing)
				{
					Status = "Guessing Phase 😕";
					StatusDescription = $"{game.CurrentTurn.Team} Team Researchers are submitting guesses.";
					if (game.CurrentTurn.Team == localPlayer.Team)
					{
						if (localPlayer.Character?.Type == CharacterType.Cultist)
						{
							LocalPlayerInstruction = "Keep a straight face and wait for your researchers to submit guesses! 😎";
						}
						else
						{
							LocalPlayerInstruction = "Review the hint and submit your guess! If you aren't feeling confident you can always vote to end turn.";
						}
					}
					else
					{
						LocalPlayerInstruction = "Review the board and be polite as you wait for your team's turn 😇";
					}
				}

				if (game.CurrentTurn.Status == TurnStatus.Tallying)
				{
					Status = "Robots Calculating Results 🤖";
					StatusDescription = $"{game.CurrentTurn.Team} Team Researchers have submitted guesses. Hold for result! 🥁";
					if (game.CurrentTurn.Team == localPlayer.Team)
					{
						if (localPlayer.Character?.Type == CharacterType.Cultist)
						{
							LocalPlayerInstruction = "Nothing you can do now your team has spoken!";
						}
						else
						{
							LocalPlayerInstruction = "Hold on tight the result of your vote will be finalized soon!";
						}
					}
					else
					{
						LocalPlayerInstruction = "Hold on tight the opposing teams vote will be finalized soon!";
					}
				}

				if (game.CurrentTurn.Status == TurnStatus.Over)
				{
					Team? winningTeam = game.GetWinningTeam();
					if (winningTeam != null)
					{
						Status = $"{winningTeam} Wins! 🧨";
						if (localPlayer.Team == winningTeam)
						{
							StatusDescription = "Congratulations! You won! 🎉🎉🎉🎉";
							LocalPlayerInstruction = "Give yourself a pat on the back. 😎";
						}
						else
						{
							StatusDescription = "Unfortunately... you lost. 🎻";
							LocalPlayerInstruction = "Do better next time. 😟";
						}
					}
				}
			}
		}

		public string LocalPlayerInstruction { get; } = string.Empty;
		public string StatusDescription { get; } = string.Empty;
		public string Status { get; } = string.Empty;

	}
}
