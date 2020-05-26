using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebsocketGameServer.Data.Models.Rooms
{
    /// <summary>
    /// Game room transfer data for sending a simple game room object
    /// </summary>
    public interface IGameRoomData : IRoomData
    {
        /// <summary>
        /// The game type ID of the game being played within the room
        /// </summary>
        public int GameType { get; }
    }
}
