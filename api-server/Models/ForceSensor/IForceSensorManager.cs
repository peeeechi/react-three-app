using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Linq;
using System.Collections.Generic;
using MinebeaSensorLib.Ethernet;

namespace api_server
{
    public interface IForceSensorManager: IDisposable
    {
        public event SensorDataUpdatedHandler SensorDataUpdated;
        public void Connect();
        public void DisConnect();
        public bool IsConnecting { get; }
    }
}
