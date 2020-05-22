using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsocketGameServer.Data.Game.Player;

namespace WebsocketGameServer.Data.Game.Room
{
    public interface IRoom
    {
        public string RoomID { get; }
        public HashSet<IPlayer> Players { get; }
    }
}
