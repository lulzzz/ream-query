
namespace ReamQuery.Services
{
    using System;
    using System.Net.WebSockets;

    public class ClientService
    {
        WebSocket _client;

        public void AddClient(WebSocket client)
        {
            if (_client != null)
            {
                throw new NotImplementedException("close old or something");
            }
            _client = client;
        }

        
    }
}
