namespace WebsocketGameServer.Models.Player
{
    public class PlayerJwtTokenModel
    {
        public PlayerJwtTokenModel(string jwtToken)
        {
            JwtToken = jwtToken;
        }


        public string JwtToken { get; set; }
    }
}
