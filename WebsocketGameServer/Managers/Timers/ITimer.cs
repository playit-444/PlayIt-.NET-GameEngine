using System.Collections.Generic;
using WebsocketGameServer.Data.Game.Room.Lobbies;

namespace WebsocketGameServer.Managers.Timers
{
    public interface ITimer
    {
        public event ReadyTimer.TimerEndHandler OnTimerEnd;

        public delegate void TimerEndHandler(string id);

        public IDictionary<string, int> Timers { get; set; }

        public void AddTimer(GameStartEventArgs args, int timer);
        public void RemoveTimer(string id);

    }
}
