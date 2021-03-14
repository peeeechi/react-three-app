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

    public class ForceSensorResponceDouble
    {
        [JsonProperty("sensor-1")]
        public ForceSensorData Sensor1 { get; set; } = null;

        [JsonProperty("sensor-2")]
        public ForceSensorData Sensor2 { get; set; } = null;

        [JsonProperty("combined")]
        public ForceSensorData Combined {get; set;} = null;

        [JsonProperty("time-stamp")]
        public Int64 TimeStamp { get; set; }
    }

    public class ForceSensorHandlerDouble : WebSocketHandler
    {
        private uint sensorCount =  0;
        private ForceSensorData sensor1Offset = new ForceSensorData();
        private ForceSensorData sensor2Offset = new ForceSensorData();
        private static uint meanTimes = 1000;

        float[] meanbuf1fx = new float[meanTimes];
        float[] meanbuf1fy = new float[meanTimes];
        float[] meanbuf1fz = new float[meanTimes];
        float[] meanbuf1mx = new float[meanTimes];
        float[] meanbuf1my = new float[meanTimes];
        float[] meanbuf1mz = new float[meanTimes];

        float[] meanbuf2fx = new float[meanTimes];
        float[] meanbuf2fy = new float[meanTimes];
        float[] meanbuf2fz = new float[meanTimes];
        float[] meanbuf2mx = new float[meanTimes];
        float[] meanbuf2my = new float[meanTimes];
        float[] meanbuf2mz = new float[meanTimes];
     

        private ForceSensorEthernetManager forceSensor;
        private Quaternion q = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), (float)Math.PI);
   

        public ForceSensorHandlerDouble(WebSocketObjectHolder webSocketObjectHolder): base(webSocketObjectHolder)
        {
            forceSensor = new ForceSensorEthernetManager();
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
            ForceSensorData sensor1 = (data.Sensor1Data == null) ? new ForceSensorData() : data.Sensor1Data;
            ForceSensorData sensor2 = (data.Sensor2Data == null) ? new ForceSensorData() : data.Sensor2Data;
           

            if (sensorCount < meanTimes)
            {
                meanbuf1fx[sensorCount] = sensor1.Fx;
                meanbuf1fy[sensorCount] = sensor1.Fy;
                meanbuf1fz[sensorCount] = sensor1.Fz;
                meanbuf1mx[sensorCount] = sensor1.Mx;
                meanbuf1my[sensorCount] = sensor1.My;
                meanbuf1mz[sensorCount] = sensor1.Mz;

                meanbuf2fx[sensorCount] = sensor2.Fx;
                meanbuf2fy[sensorCount] = sensor2.Fy;
                meanbuf2fz[sensorCount] = sensor2.Fz;
                meanbuf2mx[sensorCount] = sensor2.Mx;
                meanbuf2my[sensorCount] = sensor2.My;
                meanbuf2mz[sensorCount] = sensor2.Mz;

                sensorCount++;

                if (sensorCount >= meanTimes)
                {
                    sensor1Offset.Fx = meanbuf1fx.Average();
                    sensor1Offset.Fy = meanbuf1fy.Average();
                    sensor1Offset.Fz = meanbuf1fz.Average();
                    sensor1Offset.Mx = meanbuf1mx.Average();
                    sensor1Offset.My = meanbuf1my.Average();
                    sensor1Offset.Mz = meanbuf1mz.Average();

                    sensor2Offset.Fx = meanbuf2fx.Average();
                    sensor2Offset.Fy = meanbuf2fy.Average();
                    sensor2Offset.Fz = meanbuf2fz.Average();
                    sensor2Offset.Mx = meanbuf2mx.Average();
                    sensor2Offset.My = meanbuf2my.Average();
                    sensor2Offset.Mz = meanbuf2mz.Average();
                }
            }
            else
            {

                var sensor1ForceVector  = new Vector3(sensor1.Fx - sensor1Offset.Fx, sensor1.Fy - sensor1Offset.Fy, sensor1.Fz - sensor1Offset.Fz);
                var sensor1MomentVector = new Vector3(sensor1.Mx - sensor1Offset.Mx, sensor1.My - sensor1Offset.My, sensor1.Mz - sensor1Offset.Mz);

                var sensor2ForceVector  = new Vector3(sensor2.Fx - sensor2Offset.Fx, sensor2.Fy - sensor2Offset.Fy, sensor2.Fz - sensor2Offset.Fz);
                var sensor2MomentVector = new Vector3(sensor2.Mx - sensor2Offset.Mx, sensor2.My - sensor2Offset.My, sensor2.Mz - sensor2Offset.Mz);

                q = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), (float)Math.PI);
                // Quaternion q = new Quaternion(new Vector3(0, 1, 0), 180.0f);

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
                    //TimeStamp = data.MeasureTime,
                    TimeStamp = 0,
                };

                var resStr = JsonConvert.SerializeObject(responce);
                // Console.WriteLine($"updated: {ii}\n{data.StatusCode}");

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
        ~ForceSensorHandlerDouble()
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
