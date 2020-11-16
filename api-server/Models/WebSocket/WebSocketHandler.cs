using System;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Net.WebSockets;

namespace api_server
{
    public abstract class WebSocketHandler
    {
        protected WebSocketObjectHolder WebSocketObjectHolder { get; set; }

        public WebSocketHandler(WebSocketObjectHolder webSocketObjectHolder)
        {
            WebSocketObjectHolder = webSocketObjectHolder;
        }

        /// <summary>
        /// 接続時処理
        /// socketをストアに追加する
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        public virtual Task OnConnected(WebSocket socket)
        {
            WebSocketObjectHolder.AddSocket(socket);
            return Task.CompletedTask; // 完了済みのTaskとして返す

            /*
             * Task.CompletedTask は以下と同じ意味
             *  1. 
             *      Task.Run(() => { ... do something...});
             *  2.
             *      Task.FromResult(0);             
             */
        }

        /// <summary>
        /// 切断時の処理
        /// socketをストアから削除する
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        public virtual async Task OnDisconnected(WebSocket socket)
        {
            await WebSocketObjectHolder.RemoveSocket(WebSocketObjectHolder.GetId(socket));
        }

        /// <summary>
        /// Clientへmessageを送信します
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendMessageAsync(WebSocket socket, byte[] message, WebSocketMessageType type=WebSocketMessageType.Binary, bool isEndOfMessage=true)
        {
            if (socket.State != WebSocketState.Open) return;

            await socket.SendAsync(new ArraySegment<byte>(message, 0, message.Length), type, isEndOfMessage, CancellationToken.None);


        }

        /// <summary>
        /// Clientへmessageを送信します(文字列として送信します)
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendMessageAsync(WebSocket socket, string message, System.Text.Encoding encoding, bool isEndOfMessage = true)
        {
            if (message == null || message == string.Empty) return;

            var bytes = encoding.GetBytes(message);
            await this.SendMessageAsync(socket, bytes, WebSocketMessageType.Text, isEndOfMessage);
        }

        /// <summary>
        /// Clientへmessageを送信します
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendMessageAsync(string socketId, byte[] message, WebSocketMessageType type = WebSocketMessageType.Binary, bool isEndOfMessage = true)
        {
            var socket = WebSocketObjectHolder.GetSocketById(socketId);
            if (socket == null) return;

            await SendMessageAsync(socket, message, type, isEndOfMessage);
        }

        /// <summary>
        /// Clientへmessageを送信します(文字列として送信します)
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendMessageAsync(string socketId, string message, System.Text.Encoding encoding, bool isEndOfMessage = true)
        {
            var socket = WebSocketObjectHolder.GetSocketById(socketId);
            if (socket == null) return;

            await SendMessageAsync(socket, message, encoding, isEndOfMessage);
        }

        /// <summary>
        /// 登録されているsocket 全てへメッセージを送信します(バイナりとして送信します)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="isEndOfMessage"></param>
        /// <returns></returns>
        public async Task SendMessageToAllAsync(byte[] message, bool isEndOfMessage = true)
        {
            foreach (var pair in WebSocketObjectHolder.GetAll())
            {
                if (pair.Value.State == WebSocketState.Open)
                {
                    await SendMessageAsync(pair.Value, message, WebSocketMessageType.Binary, isEndOfMessage);
                }
            }
        }

        /// <summary>
        /// 登録されているsocket 全てへメッセージを送信します(文字列として送信します)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="isEndOfMessage"></param>
        /// <returns></returns>
        public async Task SendMessageToAllAsync(string message, System.Text.Encoding encoding, bool isEndOfMessage=true)
        {
            foreach (var pair in WebSocketObjectHolder.GetAll())
            {
                if (pair.Value.State == WebSocketState.Open)
                {
                    await SendMessageAsync(pair.Value, message, encoding, isEndOfMessage);
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="result"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public abstract Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer);
    }
}
