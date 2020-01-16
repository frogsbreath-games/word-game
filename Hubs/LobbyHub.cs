using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordGame.API.Domain.Models;
using WordGame.API.Extensions;

namespace WordGame.API.Hubs
{
    public class LobbyHub : AuthorizedGroupHub<ILobbyClient>
    {
        protected override string GroupName => $"{Context.User.GetGameCode()}-lobby";

        public Task SendMessage(string message)
        {
            return Clients.Group(GroupName).MessageSent(
                new LobbyMessage(Context.User.Identity.Name, message));
        }
    }

    public interface ILobbyClient
    {
        Task MessageSent(LobbyMessage message);

        Task GameDeleted();

        Task GameUpdated(Game game);
    }
}
