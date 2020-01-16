using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordGame.API.Domain.Enums;

namespace WordGame.API.Domain.Models
{
    public class LobbyMessage
    {
        public LobbyMessage(string name, string message)
        {
            Name = name;
            Message = message;
            //Time stamp?
        }

        public string Name { get; protected set; }
        public string Message { get; protected set; }
    }
}
