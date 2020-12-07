using System;
using System.Numerics;
using System.Threading;
using System.Text;
using System.IO;
using System.Linq;
using System.Timers;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Net.WebSockets;
using Newtonsoft.Json;

namespace api_server
{
    public class SensorDataInfo
    {
        [JsonProperty("sensor-1")]
        public ForceSensorData Sensor1 { get; set; } = null;

        [JsonProperty("time")]
        public Int64 Time { get; set; }
    }
    public class SensorInfoResponse
    {
        [JsonProperty("force")]
        public SensorDataInfo Force { get; set; } = null;

        [JsonProperty("robot")]
        public RobotInfo RobotInfo { get; set; } = null;

        [JsonProperty("time")]
        public Int64 TimeStamp { get; set; }
        //public DateTime TimeStamp { get; set; }
    }

    public static class WebSocketCommands
    {
        public const string SENSOR_RESET    = "sensor-reset";
        public const string LOG_START       = "log-start";
        public const string LOG_STOP        = "log-stop";
    }

    public class WebSocketRequest
    {
        [JsonProperty("cmd")]
        public string Cmd { get; set; }

        [JsonProperty("args")]
        public object Args { get; set; }
    }

    public class SensorInfoHandler : WebSocketHandler
    {
        private const double SOCKET_SEND_SPAN       = 20; // mSec
        private const double LOG_DATA_PICK_SPAN     = 4; // mSec

        private DateTime sendBefore = DateTime.Now;

        private object _sensorValueLocker       = new object();
        private object _robotValueLocker        = new object();
        private bool isEthernet                 = false;
        private string comPort1                 = "COM5";
        private System.Timers.Timer messageSendTimer;
        private System.Timers.Timer logValuePickTimer;

        private uint sensorCount                    = 0;
        private ForceSensorData sensor1Offset       = new ForceSensorData();
        private static uint meanTimes               = 1000;
        private List<SensorInfoResponse> logBuffer  = new List<SensorInfoResponse>();

        float[] meanbuf1fx = new float[meanTimes];
        float[] meanbuf1fy = new float[meanTimes];
        float[] meanbuf1fz = new float[meanTimes];
        float[] meanbuf1mx = new float[meanTimes];
        float[] meanbuf1my = new float[meanTimes];
        float[] meanbuf1mz = new float[meanTimes];

        private readonly SemaphoreSlim robotSemaphore   = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim sensorSemaphore  = new SemaphoreSlim(1, 1);

        private SensorDataInfo currentSensorValue = new SensorDataInfo
        {
            Sensor1 = new ForceSensorData
            {
                Fx = 0,
                Fy = 0,
                Fz = 0,
                Mx = 0,
                My = 0,
                Mz = 0,
            },
            Time = 0,
        };
        public SensorDataInfo CurrentSensorValue
        {
            get
            {
                lock (_sensorValueLocker)
                {
                    return currentSensorValue;
                }
            }
            set
            {
                lock (_sensorValueLocker)
                {
                    currentSensorValue = value;
                }
            }
        }

        private RobotInfo currentRobotInfo = new RobotInfo { Tcp = new Position { X = 0, Y = 0, Z = 0, Role = 0, Pitch = 0, Yaw = 0 } };
        public RobotInfo CurrentRobotInfo 
        {
            get
            {
                lock (_robotValueLocker)
                {
                    return currentRobotInfo;
                }
            }
            set
            {
                lock (_robotValueLocker)
                {
                    currentRobotInfo = value;
                }
            }
        }

        private IForceSensorManager forceSensor;
        private CFD_Logger cfdLogger;
        private Settings settings = null;


        public SensorInfoHandler(WebSocketObjectHolder webSocketObjectHolder, Settings settings): base(webSocketObjectHolder)
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
            cfdLogger = new CFD_Logger();
            cfdLogger.RobotInfoUpdated += CfdLogger_RobotInfoUpdated;

        }

        private void CfdLogger_RobotInfoUpdated(RobotInfo info)
        {
            this.CurrentRobotInfo = info;
        }

        private void LogStart()
        {
            this.logBuffer.Clear();
            StartLoggingTimer();
        }

        private void LogStopAndSave(string fileName)
        {
            StopLoggingTimer();

            var keys = new string[]
            {
                "fx",
                "fy",
                "fz",
                "mx",
                "my",
                "mz",
                "sensor-time-stamp",
                "tcp-x",
                "tcp-y",
                "tcp-z",
                "tcp-role",
                "tcp-pitch",
                "tcp-yaw",
                "cfd-time-stamp",
                "log-time-stamp"
            };

            var sb = new StringBuilder();
            sb.Append(string.Join(',', keys));

            foreach (var log in this.logBuffer)
            {
                var rowItem = new object[]
                {
                    log.Force.Sensor1.Fx,
                    log.Force.Sensor1.Fy,
                    log.Force.Sensor1.Fz,
                    log.Force.Sensor1.Mx,
                    log.Force.Sensor1.My,
                    log.Force.Sensor1.Mz,
                    log.Force.Time,

                    log.RobotInfo.Tcp.X,
                    log.RobotInfo.Tcp.Y,
                    log.RobotInfo.Tcp.Z,
                    log.RobotInfo.Tcp.Role,
                    log.RobotInfo.Tcp.Pitch,
                    log.RobotInfo.Tcp.Yaw,
                    log.RobotInfo.Time,

                    log.TimeStamp
                };

                sb.Append(Environment.NewLine);
                sb.Append(string.Join(',', rowItem));
            }

            var saveFileName = (fileName != null || fileName != String.Empty) ? fileName : "testlog.csv";
            var dir = Path.GetDirectoryName(saveFileName);

            if (dir != null && dir != string.Empty && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            using (StreamWriter sw = new StreamWriter(saveFileName,  false, AppConst.NON_BOM_UTF8_ENCORDING))
            {
                sw.Write(sb.ToString());
            }
        }

        /// <summary>
        /// コントローラーのステータスを全体へ通知
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private void ForceSensor_SensorDataUpdated(ForceSensorUpdateArgs data)
        {
            if (data.Sensor1Data != null)
            {
                if (sensorCount < meanTimes)
                {
                    meanbuf1fx[sensorCount] = data.Sensor1Data.Fx;
                    meanbuf1fy[sensorCount] = data.Sensor1Data.Fy;
                    meanbuf1fz[sensorCount] = data.Sensor1Data.Fz;
                    meanbuf1mx[sensorCount] = data.Sensor1Data.Mx;
                    meanbuf1my[sensorCount] = data.Sensor1Data.My;
                    meanbuf1mz[sensorCount] = data.Sensor1Data.Mz;

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

                    var sensor1ForceVector = new Vector3(data.Sensor1Data.Fx - sensor1Offset.Fx, data.Sensor1Data.Fy - sensor1Offset.Fy, data.Sensor1Data.Fz - sensor1Offset.Fz);
                    var sensor1MomentVector = new Vector3(data.Sensor1Data.Mx - sensor1Offset.Mx, data.Sensor1Data.My - sensor1Offset.My, data.Sensor1Data.Mz - sensor1Offset.Mz);

                    this.CurrentSensorValue = new SensorDataInfo
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
                        Time = data.Time
                    };


                }


          
            }            
        }

        private void StartSendTimer()
        {
            if (messageSendTimer == null)
            {
                messageSendTimer            = new System.Timers.Timer(SOCKET_SEND_SPAN);
                messageSendTimer.Elapsed    += MessageSendTimer_Elapsed;
                messageSendTimer.Start();
            }
        }

        private void StopSendTimer()
        {
            if (messageSendTimer != null)
            {
                messageSendTimer.Stop();
                messageSendTimer = null;
            }
        }

        private void MessageSendTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var responce = new SensorInfoResponse();
            responce.Force = CurrentSensorValue;
            responce.RobotInfo = CurrentRobotInfo;
            responce.TimeStamp = e.SignalTime.ToFileTime();
            //sendBefore = e.SignalTime;

            var resStr = JsonConvert.SerializeObject(responce);
            // Console.WriteLine($"updated: {ii}\n{data.StatusCode}");

            SendMessageToAllAsync(resStr, AppConst.NON_BOM_UTF8_ENCORDING);
        }

        private void StartLoggingTimer()
        {
            if (logValuePickTimer == null)
            {
                logValuePickTimer = new System.Timers.Timer(LOG_DATA_PICK_SPAN);
                logValuePickTimer.Elapsed += LogValuePickTimer_Elapsed;
                logValuePickTimer.Start();
            }
        }

        private void LogValuePickTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var log = new SensorInfoResponse
            {
                Force = CurrentSensorValue,
                RobotInfo = CurrentRobotInfo,
                TimeStamp = e.SignalTime.ToFileTime()
            };
            logBuffer.Add(log);
        }

        private void StopLoggingTimer()
        {
            if (logValuePickTimer != null)
            {
                logValuePickTimer.Stop();
                logValuePickTimer = null;
            }
        }

        // WebSocket 関連イベント

        public override async Task OnConnected(WebSocket socket)
        {            
            await base.OnConnected(socket);

            if (!cfdLogger.IsLogging)
            {
                cfdLogger.Connect(settings.CfdIP, settings.CfdPort);
            }

            if (!forceSensor.IsConnecting)
            {
                forceSensor.Connect();
            }

            StartSendTimer();

            var socketid = WebSocketObjectHolder.GetId(socket);
            Console.WriteLine($"socket created: {socketid}");

            await Task.CompletedTask;
        }

        public override async Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
        {
            var socketid = WebSocketObjectHolder.GetId(socket);

            var message = AppConst.NON_BOM_UTF8_ENCORDING.GetString(buffer, 0, result.Count);

            if (message.Length > 0)
            {
                try
                {
                    var request = JsonConvert.DeserializeObject<WebSocketRequest>(message);
                    Console.WriteLine(request.Cmd);
                    Console.WriteLine(request.Args);

                    switch (request.Cmd)
                    {
                        case (WebSocketCommands.SENSOR_RESET):
                            sensorCount = 0;
                            break;

                        case (WebSocketCommands.LOG_START):
                            LogStart();
                            break;

                        case (WebSocketCommands.LOG_STOP):
                            LogStopAndSave(request.Args.ToString());
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    //await socket.SendAsync(AppConst.NON_BOM_UTF8_ENCORDING.GetBytes(e.ToString()), WebSocketMessageType.Text, true, CancellationToken.None);
                }

            }




            await Task.CompletedTask;   // Todo: コマンド受信処理

        }

        public override async Task OnDisconnected(WebSocket socket)
        {
            var socketId = WebSocketObjectHolder.GetId(socket);
            await base.OnDisconnected(socket);
            Console.WriteLine($"socket  closed: {socketId}");

            if (WebSocketObjectHolder.Count <= 0)
            {
                StopLoggingTimer();
                StopSendTimer();
              
                Thread.Sleep(100);

                this.forceSensor.DisConnect();
                this.cfdLogger.DisConnect();
            }

            await Task.CompletedTask;

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

                StopLoggingTimer();
                StopSendTimer();

                this.forceSensor.Dispose();
                this.cfdLogger.Dispose();
                disposedValue = true;
            }
        }

        // TODO: 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
        ~SensorInfoHandler()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: false);
        }

        //public void Dispose()
        //{
        //    // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
        //    Dispose(disposing: true);
        //    GC.SuppressFinalize(this);
        //}

        #endregion
    }
}
