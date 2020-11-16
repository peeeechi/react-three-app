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
        /// id �Ɉ�v����websocket ���擾���܂�
        /// </summary>
        /// <param name="id">�擾����websocket �� id</param>
        /// <returns></returns>
        public WebSocket GetSocketById(string id)
        {
            return _sockets.FirstOrDefault(s => s.Key == id).Value;
        }

        /// <summary>
        /// �S�Ă�websocket ���擾���܂�
        /// </summary>
        /// <returns></returns>
        public ConcurrentDictionary<string, WebSocket> GetAll()
        {
            return _sockets;
        }

        /// <summary>
        /// socket���o�^����Ă����ꍇ�A���̎���ID���擾���܂�
        /// �����ꍇ��null��Ԃ��܂�
        /// </summary>
        /// <param name="socket"></param>
        /// <returns>id or null</returns>
        public string GetId(WebSocket socket)
        {
            var soc = _sockets.FirstOrDefault(v => v.Value == socket);

            return soc.Equals(default(KeyValuePair<string, WebSocket>)) ? null : soc.Key;
        }


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
