using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsocketGameServer.Data.Game.Player;
using WebsocketGameServer.Data.Game.Room;

namespace WebsocketGameServer.Managers
{
    public class PlayerManager : IPlayerManager
    {
        public IDictionary<IPlayer, ICollection<IRoom>> PlayerRooms { get; set; }

        public Task<bool> AddPlayer(IPlayer player, IRoom[] rooms = null)
        {
            throw new NotImplementedException();
        }

        public Task RemovePlayer(IPlayer player)
        {
            throw new NotImplementedException();
        }
    }
}
