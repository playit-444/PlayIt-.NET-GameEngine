using System.Collections.Generic;
using System.Threading.Tasks;
using WebsocketGameServer.Data.Game.Players;
using WebsocketGameServer.Data.Game.Room;
using WebsocketGameServer.Models.Args;

namespace WebsocketGameServer.Managers.Room
{
    public interface IRoomManager
    {
        /// <summary>
        /// Delegate for defining methods capable of handling room state events
        /// </summary>
        /// <param name="args">The room arguments of the event</param>
        public delegate void RoomHandler(RoomArgs args);

        /// <summary>
        /// The event being invoked upon a room changing state
        /// </summary>
        public event RoomHandler RoomStateChanged;

        public IDictionary<string, IRoom> Rooms { get; }
        public IDictionary<IPlayer, ICollection<IRoom>> PlayerRooms { get; }
        public Task<bool> AddPlayer (IPlayer player, string roomId);
        public Task<bool> RemovePlayer(IPlayer player, string roomId);
        public void AddRoom(IRoom room);
        public void RemoveRoom(string roomId);
    }
}
