using System;
using System.Threading;
using System.Text;
using System.Numerics;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Net.WebSockets;
using Newtonsoft.Json;

namespace api_server
{
    public class RobotInfoTestHandler : WebSocketHandler, IDisposable
    {
        private Task testLoop;
        private bool isStop = false;
        public RobotInfoTestHandler(WebSocketObjectHolder webSocketObjectHolder): base(webSocketObjectHolder)
        {
            DateTime s = DateTime.Now;

            this.testLoop = Task.Run(async () => {
                this.isStop = false;

                var random = new Random();
                var seed = 2.0;
                while(!this.isStop)
                {
                    var randomFArray = Enumerable.Range(0, 6).Select(i => Convert.ToSingle(seed * random.NextDouble() - seed/2.0)).ToArray();
                    var data = new Position
                    {
                        X=randomFArray[0],
                        Y=randomFArray[1],
                        Z=randomFArray[2],

                        Role=randomFArray[3],
                        Pitch=randomFArray[4],
                        Yaw=randomFArray[5],
                    };

                    var res = new RobotInfo
                    {
                        Tcp = data,
                    };

                    var resStr = JsonConvert.SerializeObject(res);

                    await SendMessageToAllAsync(resStr, AppConst.NON_BOM_UTF8_ENCORDING);

                    System.Threading.Thread.Sleep(10);
                }

            });
        }

        public override async Task OnConnected(WebSocket socket)
        {
            await base.OnConnected(socket);

            var socketid = WebSocketObjectHolder.GetId(socket);
            Console.WriteLine($"socket created: {socketid}");

            await Task.CompletedTask;
            //await SendMessageToAllAsync($"{socketid} is now connected", AppConst.NON_BOM_UTF8_ENCORDING);
        }

        public override async Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
        {
            var socketid = WebSocketObjectHolder.GetId(socket);

            var message = AppConst.NON_BOM_UTF8_ENCORDING.GetString(buffer, 0, result.Count);

            await Task.CompletedTask;   // Todo: �R�}���h��M����

        }

        public override async Task OnDisconnected(WebSocket socket)
        {
            var socketId = WebSocketObjectHolder.GetId(socket);

            await base.OnDisconnected(socket);
            Console.WriteLine($"socket  closed: {socketId}");

            await Task.CompletedTask;

            //await SendMessageToAllAsync($"{socketId} disconnected", AppConst.NON_BOM_UTF8_ENCORDING);
        }

        #region Dispose Pattern

        private bool disposedValue;

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: �}�l�[�W�h��Ԃ�j�����܂� (�}�l�[�W�h �I�u�W�F�N�g)
                }

                // TODO: �A���}�l�[�W�h ���\�[�X (�A���}�l�[�W�h �I�u�W�F�N�g) ��������A�t�@�C�i���C�U�[���I�[�o�[���C�h���܂�
                // TODO: �傫�ȃt�B�[���h�� null �ɐݒ肵�܂�


                this.isStop = true;
                this.testLoop.Wait(2000);

                base.Dispose(disposing);
                disposedValue = true;
                Console.WriteLine("ForceSensorTestHandler Disposed.");
            }
        }

        // TODO: 'Dispose(bool disposing)' �ɃA���}�l�[�W�h ���\�[�X���������R�[�h���܂܂��ꍇ�ɂ̂݁A�t�@�C�i���C�U�[���I�[�o�[���C�h���܂�
        ~RobotInfoTestHandler()
        {
            // ���̃R�[�h��ύX���Ȃ��ł��������B�N���[���A�b�v �R�[�h�� 'Dispose(bool disposing)' ���\�b�h�ɋL�q���܂�
            Dispose(disposing: false);
        }

        // public void Dispose()
        // {
        //     // ���̃R�[�h��ύX���Ȃ��ł��������B�N���[���A�b�v �R�[�h�� 'Dispose(bool disposing)' ���\�b�h�ɋL�q���܂�
        //     Dispose(disposing: true);
        //     GC.SuppressFinalize(this);
        // }

        #endregion
    }
}
