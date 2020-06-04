namespace WebsocketGameServer.Data.Models.Games
{
    /// <summary>
    /// base interface for defining room related messages
    /// </summary>
    public interface IGameMessageUnity
    {
        public string RoomId { get; set; }
        public string Action { get; set; }
        public string Args { get; set; }
    }
}
