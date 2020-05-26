using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsocketGameServer.Data.Constants;
using WebsocketGameServer.Data.Game.Room;

namespace WebsocketGameServer.Models.Args
{
    public class RoomArgs
    {
        public RoomArgs(IRoom room, RoomActionType roomActionType)
        {
            Room = room;
            ActionType = roomActionType;
        }

        public IRoom Room { get; private set; }

        public RoomActionType ActionType { get; private set; }
    }
}
