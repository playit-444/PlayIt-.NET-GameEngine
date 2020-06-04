namespace WebsocketGameServer.Data.Models.Games
{
    public class GameMessageUnity : IGameMessageUnity
    {
        public string RoomId { get; set; }
        public string Action { get; set; }
        public string Args { get; set; }

        public GameMessageUnity(string roomId, string action, string args)
        {
            RoomId = roomId;
            Action = action;
            Args = args;
        }
    }
}
