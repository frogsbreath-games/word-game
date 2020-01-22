using System.Threading.Tasks;
using WordGame.API.Domain.Models;
using WordGame.API.Extensions;
using WordGame.API.Models;

namespace WordGame.API.Hubs
{
	public class GameHub : AuthorizedGroupHub<IGameClient>
	{
		protected override string GroupName => Context.User.GetGameCode();

		public Task SendMessage(string message)
		{
			return Clients.Group(GroupName).MessageSent(
				new LobbyMessage(Context.User.Identity.Name, message));
		}
	}

	public interface IGameClient
	{
		Task MessageSent(LobbyMessage message);

		Task GameDeleted();

		Task GameUpdated(GameModel gameModel);

		Task GameEvent(GameEvent gameEvent);
	}
}
