using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebsocketGameServer.Data.Messages
{
    public class RoomMessage : IRoomMessage
    {
        public RoomMessage(long playerId, string action, object[] args)
        {
            PlayerId = playerId;
            Action = action;
            Args = args;
        }

        public long PlayerId { get; set; }
        public string Action { get; set; }
        public object[] Args { get; set; }
    }
}
