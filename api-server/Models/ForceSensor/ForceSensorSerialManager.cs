using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Linq;
using System.Collections.Generic;
using MinebeaSensorLib.Serial;
using System.IO.Ports;

namespace api_server
{
    public class ForceSensorSerialManager : IForceSensorManager, IDisposable
    {
        public ForceSensorSerialManager(){}

        public string PortName { get; set; } = null;

        private bool isConnecting = false;
        public bool IsConnecting 
        { 
            get => isConnecting;
            private set 
            {
                isConnecting = value;
            }
        }

        private MinebeaForceSensorSerialClient client = null;

        public event SensorDataUpdatedHandler SensorDataUpdated = null;

        public void Connect()
        {         
            if (this.client != null)
            {
                this.client.Dispose();
            }
            client = new MinebeaForceSensorSerialClient();
            client.Connect(this.PortName);

            ResponseStatus status;

            var vddList = new List<VDD> { VDD.VDD12, VDD.VDD45 };
            try
            {
                status = client.BootSelect();
                Console.WriteLine($"BootSelect: {status}");

                vddList.ForEach(vdd =>
                {
                    status = client.PowerSwitch(vdd, true);
                    Console.WriteLine($"PowerSwitch - {vdd}: {status}");
                });

                var axisList = new List<AxisID>
                    {
                        AxisID.Fx,
                        AxisID.Fy,
                        AxisID.Fz,
                        AxisID.Mx,
                        AxisID.My,
                        AxisID.Mz
                    };

                axisList.ForEach(id =>
                {
                    status = client.AxisSelect(id);
                    Console.WriteLine($"AxisSelect - {id}: {status}");

                    status = client.Idle();
                    Console.WriteLine($"Idle: {status}");
                });

                status = client.Bootload();
                Console.WriteLine($"Bootload: {status}");

                uint interval = 1000;

                status = client.IntervalMeasure(interval);
                Console.WriteLine($"IntervalMeasure - {interval}: {status}");

                status = client.IntervalRestart(0);
                Console.WriteLine($"IntervalRestart - {0}: {status}");

                status = client.Start((data) =>
                {
                    this.SensorDataUpdated?.Invoke(new ForceSensorUpdateArgs
                    {
                        Sensor1Data = new ForceSensorData
                        {
                            Fx = data.Fx,
                            Fy = data.Fy,
                            Fz = data.Fz,
                            Mx = data.Mx,
                            My = data.My,
                            Mz = data.Mz,
                        },
                        Sensor2Data = null,
                        //Time = data.Time
                        Time = DateTime.Now.ToFileTime(),
                    });
                });

                Console.WriteLine($"Start: {status}");

                IsConnecting = true;


            }
            catch (Exception e)
            {

                Console.Error.WriteLine(e.ToString());
                //client.Stop();
                //this.DisConnect();

                throw;
            }
          

        }

        public void DisConnect()
        {
            if (client != null)
            {
                var status = client.Stop();
                Console.WriteLine($"Stop: {status}");

                new List<VDD> { VDD.VDD12, VDD.VDD45 }.ForEach(vdd =>
                   {
                       status = client.PowerSwitch(vdd, false);
                       Console.WriteLine($"PowerSwitch - {vdd}: {status}");
                   });
                client.DisConnect();
                client.Dispose();
                client = null;
            }
            this.IsConnecting = false;
        }

        #region Dispose

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

                if (this.client != null)
                {
                    if (IsConnecting)
                    {
                        this.DisConnect();
                    }
                    this.client.Dispose();
                    this.client = null;
                }
            }
        }

        ~ForceSensorSerialManager()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion

    }
}
