using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using WebsocketGameServer.Data.Game.Players;
using WebsocketGameServer.Data.Game.Room.Lobbies;
using WebsocketGameServer.Data.Models.Players;

namespace WebsocketGameServer.Managers.Timers
{
    public class ReadyTimer : ITimer
    {
        public event TimerEndHandler OnTimerEnd;

        public delegate void TimerEndHandler(string id);

        public IDictionary<string, int> Timers { get; set; }
        public IDictionary<string, IPlayer[]> TimersData { get; set; }

        public ReadyTimer()
        {
            Timers = new Dictionary<string, int>();
            TimersData = new Dictionary<string, IPlayer[]>();
        }

        public void StartTimer()
        {
            for (;;)
            {
                foreach (var room in Timers.ToList())
                {
                    Timers[room.Key]--;
                    SendMessage(TimersData[room.Key], Timers[room.Key]);
                    if (Timers[room.Key] <= 0)
                    {
                        RemoveTimer(room.Key);
                        OnTimerEnd?.Invoke(room.Key);
                    }
                }

                Thread.Sleep(1000);
            }

            // ReSharper disable once FunctionNeverReturns
        }

        public void AddTimer(GameStartEventArgs args, int timer)
        {
            lock (Timers)
            {
                if (args.MinimumPlayers <= args.ReadyPlayers)
                {
                    if (Timers.ContainsKey(args.RoomId))
                    {
                        if (args.MinimumPlayers >= args.ReadyPlayers)
                        {
                            Timers[args.RoomId] = 5;
                        }
                        else
                        {
                            Timers[args.RoomId] = 60;
                        }
                    }
                    else
                    {
                        Timers.Add(args.RoomId, timer);
                    }

                    TimersData[args.RoomId] = args.Players;
                }
                else
                {
                    if (Timers.ContainsKey(args.RoomId))
                    {
                        RemoveTimer(args.RoomId);
                        SendMessage(args.Players, -1);
                    }
                }
            }
        }

        public void RemoveTimer(string id)
        {
            lock (Timers)
            lock (TimersData)
            {
                Timers.Remove(id);
                TimersData.Remove(id);
            }
        }

        public async void SendMessage(IPlayer[] players, int timer)
        {
            foreach (var player in players.ToList())
            {
                var encoded =
                    Encoding.UTF8.GetBytes(
                        JsonConvert.SerializeObject(new PlayerCountdown(timer)));
                var buffers = new ArraySegment<Byte>(encoded, 0, encoded.Length);
                await player.Socket.SendAsync(buffers, WebSocketMessageType.Text, true,
                        CancellationToken.None)
                    .ConfigureAwait(false);
            }
        }
    }
}
