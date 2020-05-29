using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebsocketGameServer.Data.Models.Types
{
    public class GameTypeData
    {
        public int gameTypeId { get; set; }
        public string name { get; set; }
        public int minimumPlayers { get; set; }
        public int maxPlayers { get; set; }
    }
}
