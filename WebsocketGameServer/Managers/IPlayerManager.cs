using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsocketGameServer.Data.Game.Player;
using WebsocketGameServer.Data.Game.Room;

namespace WebsocketGameServer.Managers
{
    public interface IPlayerManager
    {
        IDictionary<IPlayer, ICollection<IRoom>> PlayerRooms { get; }
        Task<bool> AddPlayer(IPlayer player, IRoom[] rooms = null);
        Task RemovePlayer(IPlayer player);
    }
}
