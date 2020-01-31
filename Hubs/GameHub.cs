using System.Threading.Tasks;
using WordGame.API.Domain.Models;
using WordGame.API.Extensions;
using WordGame.API.Models;
using WordGame.API.Domain.Enums;

namespace WordGame.API.Hubs
{
	public class GameHub : AuthorizedGroupHub<IGameClient>
	{
		protected override string GroupName => Context.User.GetGameCode();

		public Task SendMessage(string message, Team team)
		{
			return Clients.Group(GroupName).GameEvent(
				GameEvent.PlayerMessage(Context.User.Identity.Name, team, System.DateTime.Now, message));
		}
	}

	public interface IGameClient
	{
		Task GameDeleted();

		Task GameUpdated(GameModel gameModel);

		Task GameEvent(GameEvent gameEvent);
	}
}
