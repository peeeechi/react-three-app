using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Linq;
using System.Collections.Generic;
using MinebeaSensorLib.Ethernet;

namespace api_server
{
    public class ForceSensorUpdateArgs
    {
        public ForceSensorData Sensor1Data { get; set; } = null;
        public ForceSensorData Sensor2Data { get; set; } = null;
        public Int64 Time { get; set; } = 0;
    }

    public delegate void SensorDataUpdatedHandler(ForceSensorUpdateArgs data);

    public class ForceSensorEthernetManager: IDisposable, IForceSensorManager
    {

        const string SENSOR_IP = "192.168.0.200";
        const int SENSOR_PORT = 1366;
        const int LOCAL_PORT = 8500;
        const int SLEEP_TIME = 1000;


        #region Public Methods        

        public void Connect()
        {
            try
            {
                this.Client = new MinebeaForceSensorEthernetClient(SENSOR_IP, SENSOR_PORT, LOCAL_PORT);

                this.Client.Open();

                this.IsConnecting = true;
                StatusCode ret = StatusCode.NotSupportCommand;

                int tryCount = 0;

                do
                {
                    if (tryCount > 5)
                    {
                        throw new Exception(ret.ToString());
                    }

                    ret = this.Client.M_Reset();
                    Console.WriteLine($"Reset: {ret}");
                    Thread.Sleep(SLEEP_TIME);

                    // ret = this.Client.M_SelectI2C();
                    ret = this.Client.M_SelectSPI(SensorUseFlg.Sensor1IsEnabled | SensorUseFlg.Sensor2IsEnabled);
                    Console.WriteLine($"Select: {ret}");
                    Thread.Sleep(SLEEP_TIME);

                    ret = this.Client.M_Boot();
                    Console.WriteLine($"Boot: {ret}");
                    // Thread.Sleep(SLEEP_TIME);
                    Thread.Sleep(SLEEP_TIME);

                    ret = this.Client.M_Start();
                    Console.WriteLine($"Start: {ret}");
                    Thread.Sleep(SLEEP_TIME);

                    tryCount ++;

                } while (ret != StatusCode.OK);


                Console.WriteLine($"Data 収集中...");

                this.FetchSensorDataLoop = Task.Run(() =>
                {
                    while (this.IsConnecting)
                    {
                        var data = this.Client.M_Data();

                        var forceDataList = new List<SensorData> { data.Sensor1Data, data.Sensor2Data }.Select(d => new ForceSensorData()
                        {
                            Fx = d.Fx,
                            Fy = d.Fy,
                            Fz = d.Fz,
                            Mx = d.Mx,
                            My = d.My,
                            Mz = d.Mz,
                        }).ToList();

                        this.SensorDataUpdated?.Invoke(new ForceSensorUpdateArgs() 
                        {
                            Sensor1Data = new ForceSensorData
                            {
                                Fx = data.Sensor1Data.Fx,
                                Fy = data.Sensor1Data.Fy,
                                Fz = data.Sensor1Data.Fz,
                                Mx = data.Sensor1Data.Mx,
                                My = data.Sensor1Data.My,
                                Mz = data.Sensor1Data.Mz
                            },
                            Sensor2Data = new ForceSensorData
                            {
                                Fx = data.Sensor2Data.Fx,
                                Fy = data.Sensor2Data.Fy,
                                Fz = data.Sensor2Data.Fz,
                                Mx = data.Sensor2Data.Mx,
                                My = data.Sensor2Data.My,
                                Mz = data.Sensor2Data.Mz
                            },
                            //Time = data.MeasureTime,
                            Time = DateTime.Now.ToFileTime(),
                        });
                        Thread.Sleep(1);
                    }

                    Thread.Sleep(SLEEP_TIME);
                    this.Client.M_Stop();
                    Console.WriteLine($"Stop: {ret}");
                });
            }
            catch (Exception)
            {
                this.DisConnect();
                throw;
            }
          

        }

        public void DisConnect()
        {
            if (this.Client != null)
            {
                if (this.IsConnecting)
                {
                    try
                    {
                        this.Client.Close();
                    }
                    catch(Exception err)
                    {
                        Console.Error.WriteLine(err.ToString());
                    }
                    finally
                    {
                        this.IsConnecting = false;
                    }        
                }

                this.Client.Dispose();
                this.Client = null;
            }
        }

        #endregion

        #region Private Methods

        #endregion

        #region Props

        public MinebeaForceSensorEthernetClient Client { get; set; } = null;

        public bool IsConnecting { get; private set; } = false;

        public event SensorDataUpdatedHandler SensorDataUpdated = null;

        #endregion



        #region Private Fields

        private Task FetchSensorDataLoop = null;


        #endregion


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
                disposedValue = true;
                this.DisConnect();
            }
        }

        // // TODO: 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
        ~ForceSensorEthernetManager()
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
