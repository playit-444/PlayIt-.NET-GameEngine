using System;
using System.Net;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebsocketGameServer.Managers.Room;
using WebsocketGameServer.Services.Generators;
using WebsocketGameServer.Services.Room;
using WebsocketGameServer.Services.Security;

namespace WebsocketGameServer
{
    public class Startup
    {
        Server.GameServer server =
            new Server.GameServer(
                new Controllers.GameController(
                    new RoomManager(),
                    new PlayerVerificationService(),
                    new HexIdGenerator(),
                    new LobbyService(),
                    new ChatRoomService()));

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
        }

        public delegate void SocketHandler(HttpContext context, WebSocket socket);

        public event SocketHandler SocketJoin;

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime applicationLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            applicationLifetime.ApplicationStopping.Register(OnShutdown);

            WebSocketOptions opts = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
                ReceiveBufferSize = 4096
            };

            server.Initialize();

            app.UseWebSockets(opts);

            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/ws")
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);
                        //SocketJoin?.Invoke(context, webSocket);
                        await server.HandleNewSocketAsync(context, webSocket).ConfigureAwait(false);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                    }
                }
                else
                {
                    await next().ConfigureAwait(true);
                }
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors(opts => opts
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
            );

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
        
        private void OnShutdown()
        {
            string baseUrl = "https://api.444.dk/api/";
        
            WebRequest request = WebRequest.Create(new Uri(baseUrl + "Game/ServerClose"));
            request.Method = "DELETE";
            request.Headers.Add("Authorization", "Bearer " + server.playerJwtTokenModel.JwtToken);
            request.GetResponse();
        }
    }
}
