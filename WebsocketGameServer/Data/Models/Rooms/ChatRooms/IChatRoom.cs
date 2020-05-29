using System;
using System.Collections.Generic;
using WebsocketGameServer.Data.Game.Players;
using WebsocketGameServer.Data.Game.Room;

namespace WebsocketGameServer.Data.Models.Rooms.ChatRooms
{
    public interface IChatRoom : IRoom
    {
        public static int MaxRoomSize = 3000;
        public string Name { get; }
        public IDictionary<IPlayer, DateTime> LastMessages { get; }

        public void Message(IPlayer player, string message);
        public void Whisper(IPlayer fromPlayer, IPlayer toPlayer, string message);
    }
}
