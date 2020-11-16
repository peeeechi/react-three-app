using System;
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;

namespace MinebeaSensorLib
{
    public class MinebeaForceSeosorClient: IDisposable
    {
        private UdpClient client;
        private bool disposedValue;

        private string sensorIP = "192.168.0.200";
        private int sensorPort = 1366;
        private int localPort = 3500;

        public MinebeaForceSeosorClient()
        {
            client = new UdpClient(localPort);
        }

        private byte[] GetBytes(int length)
        {
            int getLength = 0;
            byte[] retBytes = new byte[length];
            // IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
            IPEndPoint remote = new IPEndPoint(IPAddress.Parse(sensorIP), 0);
            while (true)
            {
                byte[] ret = client.Receive(ref remote);
                for (int i = 0; i < ret.Length; i++)
                {
                    retBytes[getLength] = ret[i];
                    getLength ++;

                    if (getLength >= length) return retBytes;
                }
            }
        }
        private StatusCode GetStatusCode(byte[] bytes)
        {
            if(bytes.Length != 2) throw new Exception("GetStatusCode: バイト数が一致しません");

            // UInt16 val = BitConverter.ToUInt16(bytes, 0);
            UInt16 val = BinaryPrimitives.ReadUInt16BigEndian(new ReadOnlySpan<byte>(bytes, 0, 2));

            return (StatusCode)Enum.ToObject(typeof(StatusCode), val);
        }

        public MinebeaForceSeosorClient(string sensorIP, int sensorPort, int localPort)
        {
            this.sensorPort = sensorPort;
            this.sensorIP = sensorIP;
            this.localPort = localPort;

            client = new UdpClient(localPort);
        }

        public void Open()
        {
            client.Connect(sensorIP, sensorPort);
        }

        public void Close()
        {
            client.Close();
        }

        public StatusCode M_Start()
        {
            byte[] sendCommands = new byte[]{(byte)CommandID.START};
            client.Send(sendCommands,sendCommands.Length);

            byte[] retBytes = GetBytes(2);

            return retBytes.GetStatusCode(0);
        }

        public SensorDataResponse M_Data()
        {
            byte[] sendCommands = new byte[]{(byte)CommandID.DATA};
            client.Send(sendCommands,sendCommands.Length);

            byte[] retBytes = GetBytes(100);

            return retBytes.GetSensorDataResponse(0);
        }

        public StatusCode M_Boot()
        {
            byte[] sendCommands = new byte[]{(byte)CommandID.BOOT};
            client.Send(sendCommands,sendCommands.Length);

            byte[] retBytes = GetBytes(2);

            return retBytes.GetStatusCode(0);
        }

        public StatusCode M_Stop()
        {
            byte[] sendCommands = new byte[]{(byte)CommandID.STOP};
            client.Send(sendCommands,sendCommands.Length);

            byte[] retBytes = GetBytes(2);

            return retBytes.GetStatusCode(0);
        }

        public StatusCode M_Reset()
        {
            byte[] sendCommands = new byte[]{(byte)CommandID.RESET};
            client.Send(sendCommands,sendCommands.Length);

            byte[] retBytes = GetBytes(2);

            return retBytes.GetStatusCode(0);
        }

        public SensorStatus M_Status()
        {
            byte[] sendCommands = new byte[]{(byte)CommandID.STATUS};
            client.Send(sendCommands,sendCommands.Length);

            byte[] retBytes = GetBytes(6);

            return retBytes.GetSensorStatus(0);
        }

        public StatusCode M_SelectI2C()
        {
            byte[] sendCommands = new byte[]{(byte)CommandID.SELECT, 0x00, 0x01};
            client.Send(sendCommands,sendCommands.Length);

            byte[] retBytes = GetBytes(2);

            return retBytes.GetStatusCode(0);
        }

        public StatusCode M_SelectSPI(SensorUseFlg flg)
        {
            byte[] sendCommands = new byte[]{(byte)CommandID.SELECT, 0x01, (byte)flg};
            client.Send(sendCommands,sendCommands.Length);

            byte[] retBytes = GetBytes(2);

            return retBytes.GetStatusCode(0);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージド状態を破棄します (マネージド オブジェクト)
                }

                // this.client.Send(new byte[]{(byte)CommandID.STOP}, 1, this.sensorIP, this.sensorPort);
                this.client.Close();
                this.client.Dispose();
                disposedValue = true;
            }
        }

        ~MinebeaForceSeosorClient()
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
    }
}