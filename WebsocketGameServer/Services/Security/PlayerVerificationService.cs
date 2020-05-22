﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using WebsocketGameServer.Models.Player;

namespace WebsocketGameServer.Services.Security
{
    public class PlayerVerificationService : IVerificationService<PlayerVerificationResponseModel>
    {
        private readonly Uri ValidationRequestURL = new Uri("https://api.444.dk/api/Account/verify/");

        /// <summary>
        /// Calls The API to verify the Player
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<PlayerVerificationResponseModel> VerifyToken(string token)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage message = client.GetAsync(new Uri(ValidationRequestURL, token)).Result;

                //TODO: make response
            }
        }
    }
}