using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace api_server
{
    public class WebSocketChatMiddleware
    {
        private readonly RequestDelegate _next;
        private WebSocketHandler _webSocketHandler;

        public WebSocketChatMiddleware(RequestDelegate next, WebSocketHandler handler)
        {
            this._next = next;
            this._webSocketHandler = handler;
        }

        public async Task Invoke(HttpContext context)
        {
            //if (!context.WebSockets.IsWebSocketRequest) return;
            if (!context.WebSockets.IsWebSocketRequest) await _next(context); //WebSocket����Ȃ������牽�����Ȃ�

            // Websocket �����N�G�X�g����擾
            var socket = await context.WebSockets.AcceptWebSocketAsync();

            // Store�ɓo�^����
            await _webSocketHandler.OnConnected(socket);


            var buffer = new byte[AppConst.WebSocketReceiveBufferSize];

            // �ڑ����Ă���ԑ҂�������
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                switch (result.MessageType)
                {
                    case WebSocketMessageType.Text:
                        await _webSocketHandler.ReceiveAsync(socket, result, buffer);
                        break;

                    case WebSocketMessageType.Close:
                        await _webSocketHandler.OnDisconnected(socket);
                        break;

                    case WebSocketMessageType.Binary:
                        break;
                    default:
                        break;
                }
            }
        }

        //public async Task Invoke(HttpContext context)
        //{
        //    if (!context.WebSockets.IsWebSocketRequest) return;

        //    var socket = await context.WebSockets.AcceptWebSocketAsync();
        //    await _webSocketHandler.OnConnected(socket);

        //    await Receive(socket, async (result, buffer) =>
        //    {
        //        if (result.MessageType == WebSocketMessageType.Text)
        //        {
        //            await _webSocketHandler.ReceiveAsync(socket, result, buffer);
        //            return;
        //        }
        //        else if (result.MessageType == WebSocketMessageType.Close)
        //        {
        //            await _webSocketHandler.OnDisconnected(socket);
        //        }
        //    });
        //}

        //public async Task Receive(WebSocket webSocket, Action<WebSocketReceiveResult, byte[]> handleMessage)
        //{
        //    var buffer = new byte[1024 * 4];

        //    while (webSocket.State == WebSocketState.Open)
        //    {
        //        var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        //        handleMessage(result, buffer);
        //    }
        //}
    }
}
