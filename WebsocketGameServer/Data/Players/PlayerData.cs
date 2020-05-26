using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebsocketGameServer.Data.Players
{
    public class PlayerData : IPlayerData
    {
        public PlayerData(long playerId, string name, bool ready)
        {
            PlayerId = playerId;
            Name = name;
            Ready = ready;
        }

        public long PlayerId { get; private set; }
        public string Name { get; private set; }
        public bool Ready { get; private set; }
    }
}
