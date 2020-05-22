using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;
using System.Web;
using WebsocketGameServer.Models.Player;

namespace WebsocketGameServer.Services.Security
{
    public class PlayerVerificationService : IVerificationService<PlayerVerificationResponseModel>
    {
        private readonly string ValidationRequestURL = "https://api.444.dk/api/Account/verifyToken";

        /// <summary>
        /// Calls The API to verify the Player
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<PlayerVerificationResponseModel> VerifyToken(string token)
        {
            PlayerJwtTokenModel jwtTokenModel = new PlayerJwtTokenModel(token);
            var stringContent = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(jwtTokenModel), Encoding.UTF8, "application/json");

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response =
                    client.PostAsync(ValidationRequestURL, stringContent).Result;
                using (HttpContent content = response.Content)
                {
                    string jsonString = await content.ReadAsStringAsync().ConfigureAwait(false);
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<PlayerVerificationResponseModel>(jsonString);
                }
            }
        }
    }
}
