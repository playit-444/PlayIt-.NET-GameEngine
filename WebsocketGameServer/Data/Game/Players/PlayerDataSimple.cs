namespace WebsocketGameServer.Data.Game.Players
{
    public class PlayerDataSimple : IPlayerDataSimple
    {
        public long PlayerId { get; set; }
        public string Name { get; set; }

        public PlayerDataSimple(long playerId, string name)
        {
            PlayerId = playerId;
            Name = name;
        }
    }
}
