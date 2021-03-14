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
    public class ForceSensorTestHandlerDouble : WebSocketHandler, IDisposable
    {
        private Task testLoop;
        private bool isStop = false;
        public ForceSensorTestHandlerDouble(WebSocketObjectHolder webSocketObjectHolder): base(webSocketObjectHolder)
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

                    var res = new ForceSensorResponceDouble
                    {
                        Sensor1 = data,
                        Sensor2 = data,
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
            ForceSensorData sensor2 = (data.Sensor2Data == null)? new ForceSensorData() : new ForceSensorData
            {
                Fx = data.Sensor2Data.Fx,
                Fy = data.Sensor2Data.Fy,
                Fz = data.Sensor2Data.Fz,
                Mx = data.Sensor2Data.Mx,
                My = data.Sensor2Data.My,
                Mz = data.Sensor2Data.Mz,
            };

            var sensor1ForceVector  = new Vector3(sensor1.Fx, sensor1.Fy, sensor1.Fz);
                var sensor1MomentVector = new Vector3(sensor1.Mx, sensor1.My, sensor1.Mz);

                var sensor2ForceVector  = new Vector3(sensor2.Fx, sensor2.Fy, sensor2.Fz);
                var sensor2MomentVector = new Vector3(sensor2.Mx, sensor2.My, sensor2.Mz);

                Quaternion q = new Quaternion(new Vector3(0, 1, 0), (float)Math.PI);

                // Y軸周りに180°回転
                sensor2ForceVector  = Vector3.Transform(sensor2ForceVector, q);
                sensor2MomentVector = Vector3.Transform(sensor2MomentVector, q);

                var combindForce    = sensor1ForceVector + sensor2ForceVector;
                var combindMoment   = sensor1MomentVector + sensor2MomentVector;


                var responce = new ForceSensorResponceDouble()
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
                    Sensor2 = new ForceSensorData()
                    {
                        Fx = sensor2ForceVector.X,
                        Fy = sensor2ForceVector.Y,
                        Fz = sensor2ForceVector.Z,
                        Mx = sensor2MomentVector.X,
                        My = sensor2MomentVector.Y,
                        Mz = sensor2MomentVector.Z,
                    },
                    Combined = new ForceSensorData()
                    {
                        Fx = combindForce.X,
                        Fy = combindForce.Y,
                        Fz = combindForce.Z,
                        Mx = combindMoment.X,
                        My = combindMoment.Y,
                        Mz = combindMoment.Z,
                    },
                    //TimeStamp = data.Time,
                    //TimeStamp = DateTime.Now.ToFileTime(),
                    TimeStamp = DateTime.Now.ToFileTime(),
                };

                var resStr = JsonConvert.SerializeObject(responce);
                // Console.WriteLine($"updated: {ii}\n{data.StatusCode}");

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
        ~ForceSensorTestHandlerDouble()
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
