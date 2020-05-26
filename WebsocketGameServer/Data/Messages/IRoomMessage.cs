using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebsocketGameServer.Data.Messages
{
    /// <summary>
    /// base interface for defining room related messages
    /// </summary>
    public interface IRoomMessage
    {
        public long PlayerId { get; set; }
        public string Action { get; set; }
        public object[] Args { get; set; }
    }
}
