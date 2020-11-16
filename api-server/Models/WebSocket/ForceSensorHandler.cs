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
    public class ForceSensorData
    {
        [JsonProperty("fx")]
        public float Fx { get; set; }

        [JsonProperty("fy")]
        public float Fy { get; set; }

        [JsonProperty("fz")]
        public float Fz { get; set; }

        [JsonProperty("mx")]
        public float Mx { get; set; }

        [JsonProperty("my")]
        public float My { get; set; }

        [JsonProperty("mz")]
        public float Mz { get; set; }
    }

    public class ForceSensorResponce
    {
        [JsonProperty("sensor-1")]
        public ForceSensorData Sensor1 { get; set; } = null;

        [JsonProperty("sensor-2")]
        public ForceSensorData Sensor2 { get; set; } = null;

        [JsonProperty("time-stamp")]
        public uint TimeStamp { get; set; }
    }

    public class ForceSensorHandler : WebSocketHandler, IDisposable
    {

        private ForceSensorManager forceSensor;
   

        public ForceSensorHandler(WebSocketObjectHolder webSocketObjectHolder): base(webSocketObjectHolder)
        {
            forceSensor = new ForceSensorManager();
            forceSensor.SensorDataUpdated += ForceSensor_SensorDataUpdated;
            forceSensor.Connect();
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
                this.forceSensor.Dispose();
                disposedValue = true;
            }
        }

        // TODO: 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
        ~ForceSensorHandler()
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
