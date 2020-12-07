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
    public class ForceSensorTestHandlerSingle : WebSocketHandler, IDisposable
    {
        private Task testLoop;
        private bool isStop = false;
        public ForceSensorTestHandlerSingle(WebSocketObjectHolder webSocketObjectHolder): base(webSocketObjectHolder)
        {
            DateTime s = DateTime.Now;

            this.testLoop = Task.Run(async () => {
                this.isStop = false;

                var random = new Random();
                var seed = 2.0;
                while(!this.isStop)
                {
                    var randomFArray = Enumerable.Range(0, 6).Select(i => Convert.ToSingle(seed * random.NextDouble() - seed/2.0)).ToArray();
                    var data = new ForceSensorData
                    {
                        Fx=randomFArray[0],
                        Fy=randomFArray[1],
                        Fz=randomFArray[2],

                        Mx=randomFArray[3],
                        My=randomFArray[4],
                        Mz=randomFArray[5],
                    };

                    var res = new ForceSensorResponceSingle
                    {
                        Sensor1 = data,
                        Time = Convert.ToUInt32((DateTime.Now - s).TotalMilliseconds),
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
        private async Task ForceSensor_SensorDataUpdated(ForceSensorUpdateArgs data)
        {
            ForceSensorData sensor1 = (data.Sensor1Data == null)? new ForceSensorData() : new ForceSensorData
            {
                Fx = data.Sensor1Data.Fx,
                Fy = data.Sensor1Data.Fy,
                Fz = data.Sensor1Data.Fz,
                Mx = data.Sensor1Data.Mx,
                My = data.Sensor1Data.My,
                Mz = data.Sensor1Data.Mz,
            };
          
            var sensor1ForceVector  = new Vector3(sensor1.Fx, sensor1.Fy, sensor1.Fz);
            var sensor1MomentVector = new Vector3(sensor1.Mx, sensor1.My, sensor1.Mz);

          

                var responce = new ForceSensorResponceSingle()
                {
                    Sensor1 = new ForceSensorData()
                    {
                        Fx = sensor1ForceVector.X,
                        Fy = sensor1ForceVector.Y,
                        Fz = sensor1ForceVector.Z,
                        Mx = sensor1MomentVector.X,
                        My = sensor1MomentVector.Y,
                        Mz = sensor1MomentVector.Z,
                    },
                    Time = data.Time,
                };

                var resStr = JsonConvert.SerializeObject(responce);
                // Console.WriteLine($"updated: {ii}\n{data.StatusCode}");

                await SendMessageToAllAsync(resStr, AppConst.NON_BOM_UTF8_ENCORDING);

            Thread.Sleep(10);
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

        protected override void Dispose(bool disposing)
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

                base.Dispose(disposing);
                disposedValue = true;
                Console.WriteLine("ForceSensorTestHandler Disposed.");
            }
        }

        // TODO: 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
        ~ForceSensorTestHandlerSingle()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: false);
        }

        // public void Dispose()
        // {
        //     // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
        //     Dispose(disposing: true);
        //     GC.SuppressFinalize(this);
        // }

        #endregion
    }
}
