using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebsocketGameServer.Data.Players
{
    public interface IPlayerData
    {
        public long PlayerId { get; }
        public string Name { get; }
        public bool Ready { get; }
    }
}
