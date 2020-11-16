using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;

namespace api_server
{
    public class WebSocketReceiver
    {
        public async Task Receive(Microsoft.AspNetCore.Http.HttpContext context, Func<Task> next)
        {
            if (context.Request.Path == "/controller")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    using(WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync())
                    {
                        await this.Echo(context, webSocket);
                    }
                }
                else
                {
                    context.Response.StatusCode = 400;
                }
            }
            else
            {
                await next();
            }
        }


        private async Task Echo(Microsoft.AspNetCore.Http.HttpContext context, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }
}
