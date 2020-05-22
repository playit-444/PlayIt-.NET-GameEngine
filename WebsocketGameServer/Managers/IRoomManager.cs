using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsocketGameServer.Data.Game.Player;
using WebsocketGameServer.Data.Game.Room;

namespace WebsocketGameServer.Managers
{
    public interface IRoomManager
    {
        public IDictionary<string, IRoom> Rooms { get; }
        public Task<bool> AddPlayer(IPlayer player, string roomId);
        public Task RemovePlayer(IPlayer player, string roomId);
    }
}
