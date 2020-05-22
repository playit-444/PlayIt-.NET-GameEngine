using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebsocketGameServer.Managers;
using WebsocketGameServer.Server;
using WebsocketGameServer.Services.Security;

namespace WebsocketGameServer
{
    public class Startup
    {
        WebsocketGameServer.Server.GameServer server =
            new WebsocketGameServer.Server.GameServer(
                new Controllers.PlayerController(
                    new PlayerManager(),
                    new PlayerVerificationService()));

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
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            WebSocketOptions opts = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
                ReceiveBufferSize = 8192
            };

            //subscribe to socket event
            SocketJoin += server.HandleNewSocket;

            app.UseWebSockets(opts);

            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/ws")
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);
                        SocketJoin?.Invoke(context, webSocket);
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

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
