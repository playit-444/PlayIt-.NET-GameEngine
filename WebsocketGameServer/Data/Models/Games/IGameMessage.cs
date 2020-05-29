namespace WebsocketGameServer.Data.Models.Games
{
    /// <summary>
    /// base interface for defining room related messages
    /// </summary>
    public interface IGameMessage
    {
        public string RoomId { get; set; }
        public string Action { get; set; }
        public object[] Args { get; set; }
    }
}
