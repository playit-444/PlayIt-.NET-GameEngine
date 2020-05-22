using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;

namespace WebsocketGameServer.Services.Security
{
    public class VerificationService : IVerificationService
    {
        private readonly Uri ValidationRequestURL = new Uri("https://api.444.dk/api/Account/verify/");

        /// <summary>
        /// Calls The API to verify the Player
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<bool> VerifyToken(string token)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage message = client.GetAsync(new Uri(ValidationRequestURL, token)).Result;

                return Task.FromResult(message.IsSuccessStatusCode);
            }
        }
    }
}
