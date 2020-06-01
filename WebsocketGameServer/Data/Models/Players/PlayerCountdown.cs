namespace WebsocketGameServer.Data.Models.Players
{
    public class PlayerCountdown
    {
        public int Timer { get; private set; }

        public PlayerCountdown(int timer)
        {
            Timer = timer;
        }
    }
}
