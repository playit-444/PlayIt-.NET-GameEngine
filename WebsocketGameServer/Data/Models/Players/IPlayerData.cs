namespace WebsocketGameServer.Data.Models.Players
{
    /// <summary>
    /// base interface for defining if a player have pressed ready
    /// </summary>
    public interface IPlayerData
    {
        public long PlayerId { get; }
        public string Name { get; }
        public bool Ready { get; }
    }
}
