using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsocketGameServer.Data.Game.Player;

namespace WebsocketGameServer.Data.Game.Room
{
    public interface IRoom
    {
        /// <summary>
        /// The randomly generated ID of the room
        /// </summary>
        string RoomID { get; }

        /// <summary>
        /// The players contained within the room
        /// </summary>
        HashSet<IPlayer> Players { get; }
    }
}
