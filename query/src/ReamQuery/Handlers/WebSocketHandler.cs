namespace ReamQuery.Handlers
{
    using System.Threading.Tasks;
    using ReamQuery.Api;
    using ReamQuery.Services;
    using Microsoft.AspNetCore.Http;

    public class WebSocketHandler
    {
        protected RequestDelegate _next;

        public WebSocketHandler(RequestDelegate next, ClientService clients)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            }
            await _next(context);
        }
    }
}