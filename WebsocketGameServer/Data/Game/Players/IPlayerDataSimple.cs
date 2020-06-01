namespace WebsocketGameServer.Data.Game.Players
{
    public interface IPlayerDataSimple
    {
        /// <summary>
        /// PlayerId for player
        /// </summary>
        long PlayerId { get; set; }

        /// <summary>
        /// Name for player
        /// </summary>
        string Name { get; set; }
    }
}
