using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsocketGameServer.Data.Game.Room;

namespace WebsocketGameServer.Models.Args
{
    public class RoomArgs
    {
        public RoomArgs(IRoom room)
        {
            Room = room;
        }

        public IRoom Room { get; private set; }
    }
}
