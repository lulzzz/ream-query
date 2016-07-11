
namespace ReamQuery.Services
{
    using System;
    using System.Net.WebSockets;
    using ReamQuery.Shared;
    using ReamQuery.Helpers;
    using Newtonsoft.Json;
    using System.Threading.Tasks;

    public class ClientService
    {
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
            emitter.Messages.Subscribe(async (msg) =>
            {
                // System.Threading.Thread.Sleep(1000);
                var json = JsonConvert.SerializeObject(msg);
                await _client.SendString(json);
            });
        }
    }
}
