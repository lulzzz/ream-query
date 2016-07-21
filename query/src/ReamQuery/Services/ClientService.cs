
namespace ReamQuery.Services
{
    using System;
    using System.Net.WebSockets;
    using ReamQuery.Core;
    using ReamQuery.Helpers;
    using Newtonsoft.Json;
    using System.Threading.Tasks;
    using NLog;

    public class ClientService
    {
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
            var msgNr = 0;
            emitter.Messages.Subscribe(async (msg) =>
            {
                // System.Threading.Thread.Sleep(1000);
                var json = JsonConvert.SerializeObject(msg);
                Logger.Debug("{2}: JSON emitted msg nr {1}, {0}", json, msgNr++, emitter.Session);
                await _client.SendString(json);
            });
        }
    }
}
