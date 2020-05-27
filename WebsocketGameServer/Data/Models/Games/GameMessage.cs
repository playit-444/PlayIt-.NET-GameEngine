namespace WebsocketGameServer.Data.Models.Games
{
    public class GameMessage : IGameMessage
    {
        public string RoomId { get; set; }
        public string Action { get; set; }
        public object[] Args { get; set; }

        public GameMessage(string roomId, string action, object[] args)
        {
            RoomId = roomId;
            Action = action;
            Args = args;
        }
    }
}
