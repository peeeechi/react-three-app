using System;
using System.Threading;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Net.WebSockets;
using Newtonsoft.Json;

namespace api_server
{
    public class ForceSensorTestHandler : WebSocketHandler, IDisposable
    {
        private Task testLoop;
        private bool isStop = false;
        public ForceSensorTestHandler(WebSocketObjectHolder webSocketObjectHolder): base(webSocketObjectHolder)
        {
            DateTime s = DateTime.Now;

            this.testLoop = Task.Run(async () => {
                this.isStop = false;

                var random = new Random();

                while(!this.isStop)
                {
                    var randomFArray = Enumerable.Range(0, 6).Select(i => 2.0f * Convert.ToSingle(random.NextDouble())).ToArray();
                    var data = new ForceSensorData
                    {
                        Fx=randomFArray[0],
                        Fy=randomFArray[1],
                        Fz=randomFArray[2],

                        Mx=randomFArray[3],
                        My=randomFArray[4],
                        Mz=randomFArray[5],
                    };

                    var res = new ForceSensorResponce
                    {
                        Sensor1 = data,
                        Sensor2 = null,
                        TimeStamp = Convert.ToUInt32((DateTime.Now - s).TotalMilliseconds),
                    };

                    var resStr = JsonConvert.SerializeObject(res);

                    await SendMessageToAllAsync(resStr, AppConst.NON_BOM_UTF8_ENCORDING);

                    System.Threading.Thread.Sleep(10);
                }

            });
        }

        /// <summary>
        /// コントローラーのステータスを全体へ通知
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private async Task ForceSensor_SensorDataUpdated(MinebeaSensorLib.SensorDataResponse data)
        {
            var responce = new ForceSensorResponce()
            {
                Sensor1 = new ForceSensorData()
                {
                    Fx = data.Sensor1Data.Fx,
                    Fy = data.Sensor1Data.Fy,
                    Fz = data.Sensor1Data.Fz,
                    Mx = data.Sensor1Data.Mx,
                    My = data.Sensor1Data.My,
                    Mz = data.Sensor1Data.Mz,
                },
                Sensor2 = null,
                TimeStamp = data.MeasureTime,
            };

            var resStr = JsonConvert.SerializeObject(responce);

            await SendMessageToAllAsync(resStr, AppConst.NON_BOM_UTF8_ENCORDING);
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

            await Task.CompletedTask;   // Todo: コマンド受信処理

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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージド状態を破棄します (マネージド オブジェクト)
                }

                // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
                // TODO: 大きなフィールドを null に設定します
                this.isStop = true;

                this.testLoop.Wait(2000);
                disposedValue = true;
            }
        }

        // TODO: 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
        ~ForceSensorTestHandler()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
