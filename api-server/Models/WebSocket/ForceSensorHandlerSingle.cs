using System;
using System.Numerics;
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
    public class ForceSensorResponceSingle
    {
        [JsonProperty("sensor-1")]
        public ForceSensorData Sensor1 { get; set; } = null;

        [JsonProperty("time")]
        public Int64 Time { get; set; }
    }

    public class ForceSensorHandlerSingle : WebSocketHandler
    {
        private bool isEthernet = false;
        private string comPort1 = "COM5";

        private uint sensorCount =  0;
        private ForceSensorData sensor1Offset = new ForceSensorData();
        private static uint meanTimes = 1000;

        float[] meanbuf1fx = new float[meanTimes];
        float[] meanbuf1fy = new float[meanTimes];
        float[] meanbuf1fz = new float[meanTimes];
        float[] meanbuf1mx = new float[meanTimes];
        float[] meanbuf1my = new float[meanTimes];
        float[] meanbuf1mz = new float[meanTimes];


        //private ForceSensorEthernetManager forceSensor;

        private IForceSensorManager forceSensor;
        private Settings settings = null;
        public ForceSensorHandlerSingle(WebSocketObjectHolder webSocketObjectHolder, Settings settings): base(webSocketObjectHolder)
        {
            this.settings = settings;

            if (isEthernet)
            {
                forceSensor = new ForceSensorEthernetManager();
            }
            else
            {
                forceSensor = new ForceSensorSerialManager
                {
                    PortName = settings.SensorCom
                };
            }

            forceSensor.SensorDataUpdated += ForceSensor_SensorDataUpdated;

            try
            {
                forceSensor.Connect();
            }
            catch (System.Exception e)
            {
                Console.Error.WriteLine(e.ToString());
            }
            
        }

        /// <summary>
        /// コントローラーのステータスを全体へ通知
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private void ForceSensor_SensorDataUpdated(ForceSensorUpdateArgs data)
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
       
            if (sensorCount < meanTimes)
            {
                meanbuf1fx[sensorCount] = sensor1.Fx;
                meanbuf1fy[sensorCount] = sensor1.Fy;
                meanbuf1fz[sensorCount] = sensor1.Fz;
                meanbuf1mx[sensorCount] = sensor1.Mx;
                meanbuf1my[sensorCount] = sensor1.My;
                meanbuf1mz[sensorCount] = sensor1.Mz;

                sensorCount++;

                if (sensorCount >= meanTimes)
                {
                    sensor1Offset.Fx = meanbuf1fx.Average();
                    sensor1Offset.Fy = meanbuf1fy.Average();
                    sensor1Offset.Fz = meanbuf1fz.Average();
                    sensor1Offset.Mx = meanbuf1mx.Average();
                    sensor1Offset.My = meanbuf1my.Average();
                    sensor1Offset.Mz = meanbuf1mz.Average();
                
                }
            }
            else
            {

                var sensor1ForceVector  = new Vector3(sensor1.Fx - sensor1Offset.Fx, sensor1.Fy - sensor1Offset.Fy, sensor1.Fz - sensor1Offset.Fz);
                var sensor1MomentVector = new Vector3(sensor1.Mx - sensor1Offset.Mx, sensor1.My - sensor1Offset.My, sensor1.Mz - sensor1Offset.Mz);
            
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

                SendMessageToAllAsync(resStr, AppConst.NON_BOM_UTF8_ENCORDING);
            }
            
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

                base.Dispose(disposing);
                this.forceSensor.Dispose();
                disposedValue = true;
            }
        }

        // TODO: 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
        ~ForceSensorHandlerSingle()
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
