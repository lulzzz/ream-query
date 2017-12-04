namespace ReamQuery.Services
{
    using System;
    using System.Net.WebSockets;
    using ReamQuery.Core;
    using ReamQuery.Helpers;
    using Newtonsoft.Json;
    using System.Threading.Tasks;
    using NLog;
    using Microsoft.AspNetCore.Hosting;
    using System.Threading;
    using ReamQuery.Core.Api;

    public class ClientService
    {
        IApplicationLifetime _appLifeTime;

        public ClientService(IApplicationLifetime appLifeTime)
        {
            _appLifeTime = appLifeTime;
            _appLifeTime.ApplicationStopping.Register(() => {
                if (_client != null) {
                    Logger.Info("Closing client websocket");
                    try {
                        _client.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, "ApplicationStopping", CancellationToken.None);
                    } catch { }
                } else {
                    Logger.Info("No client websocket");
                }
            });
        }

        private static Logger Logger = LogManager.GetCurrentClassLogger();

        WebSocket _client;

        public async Task HandleClient(WebSocket client)
        {
            if (_client != null)
            {
                throw new NotImplementedException("close old or something");
            }
            
            _client = client;
            
            while (_client.State == WebSocketState.Open)
            {
                await _client.ReadString();
            }
        }

        public void AddEmitter(Emitter emitter)
        {
            // emitter.Messages.Subscribe(this.HandleMessage);
            emitter.Messages.Subscribe(async (msg) =>
            {
                var json = JsonConvert.SerializeObject(msg);
                await _client.SendString(json);
            });
        }

        void HandleMessage(Message msg)
        {
            var serializer = new JsonSerializer();
            using (var writer = new WebSocketWriter(_client))
            {
                serializer.Serialize(writer, msg);
            }
        }
    }
}
