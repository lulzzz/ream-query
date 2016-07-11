namespace ReamQuery.Handlers
{
    using System.Threading.Tasks;
    using ReamQuery.Services;
    using Microsoft.AspNetCore.Http;
    using System;

    public class WebSocketHandler
    {
        RequestDelegate _next;
        ClientService _clients;

        public WebSocketHandler(RequestDelegate next, ClientService clients)
        {
            _next = next;
            _clients = clients;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                await _clients.HandleClient(webSocket);
                return;
            }
            await _next(context);
        }
    }
}