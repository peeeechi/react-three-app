using System;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Net.WebSockets;

namespace api_server
{
    public class WebSocketObjectHolder
    {
        private ConcurrentDictionary<string, WebSocket> _sockets = new ConcurrentDictionary<string, WebSocket>();

        /// <summary>
        /// id に一致するwebsocket を取得します
        /// </summary>
        /// <param name="id">取得するwebsocket の id</param>
        /// <returns></returns>
        public WebSocket GetSocketById(string id)
        {
            return _sockets.FirstOrDefault(s => s.Key == id).Value;
        }

        /// <summary>
        /// 全てのwebsocket を取得します
        /// </summary>
        /// <returns></returns>
        public ConcurrentDictionary<string, WebSocket> GetAll()
        {
            return _sockets;
        }

        /// <summary>
        /// socketが登録されていた場合、その識別IDを取得します
        /// 無い場合はnullを返します
        /// </summary>
        /// <param name="socket"></param>
        /// <returns>id or null</returns>
        public string GetId(WebSocket socket)
        {
            var soc = _sockets.FirstOrDefault(v => v.Value == socket);

            return soc.Equals(default(KeyValuePair<string, WebSocket>)) ? null : soc.Key;
        }

        public int Count { get => _sockets.Count; }


        public void AddSocket(WebSocket socket)
        {
            _sockets.TryAdd(CreateConnectionId(), socket);
        }

        public async Task RemoveSocket(string id)
        {
            WebSocket socket;
            bool isRemoved = _sockets.TryRemove(id, out socket);

            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by the WebSocketManager", CancellationToken.None);
        }

        private string CreateConnectionId()
        {
            return Guid.NewGuid().ToString();
        }

    }
}
