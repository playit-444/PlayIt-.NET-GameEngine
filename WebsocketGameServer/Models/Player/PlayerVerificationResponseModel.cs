using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebsocketGameServer.Models.Player
{
    public class PlayerVerificationResponseModel
    {
        public PlayerVerificationResponseModel(string key, long playerId, string name)
        {
            Key = key;
            PlayerId = playerId;
            Name = name;
        }

        public string Key { get; set; }
        public long PlayerId { get; set; }
        public string Name { get; set; }
    }
}
