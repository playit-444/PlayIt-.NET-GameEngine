using System.Collections.Generic;
using WebsocketGameServer.Data.Game.Players;
using WebsocketGameServer.Data.Messages;

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

        /// <summary>
        /// Determines whether a certain player can join the room
        /// </summary>
        /// <param name="player">The player that is attempting to join the room</param>
        /// <returns>Whether the player is capable of joining the room</returns>
        public bool PlayerCanJoinRoom(IPlayer player);

        /// <summary>
        /// Handle message from socket
        /// </summary>
        /// <param name="message"></param>
        public void ReceiveMessage(IRoomMessage message);
    }
}
