using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Xml.Serialization;
using System.Linq;
using System.Collections.Generic;
using userApp;
using System.Text;

namespace api_server
{
    public delegate void RobotInfoUpdatedHandler(RobotInfo info);

    public class CFD_Logger: IDisposable
    {
        private Socket socket               = null;
        private Task watchLoop              = null;
        private bool isStop                 = false;
        private byte[] resBuffer            = new byte[4096];
        private XmlSerializer serializer    = new XmlSerializer(typeof(flexData));
        private byte[] sendMessage;
        private bool disposedValue;
        const UInt16 MAGIC_NUMBER       = 0xBA5E;
        const UInt16 VERSION_NUMBER     = 0x0001;

        public bool IsLogging { get; set; } = false;


        public event RobotInfoUpdatedHandler RobotInfoUpdated = null;

        public CFD_Logger()
        {
            var flex = new flexData
            {
                version = "1",
                dataExchange = new type_dataExchange
                {
                    dataRequest = new type_dataExchange_dataRequest[]
                  {
                        new type_dataExchange_dataRequest
                        {
                            data = new type_dataExchange_dataRequest_data[]
                            {
                                new type_dataExchange_dataRequest_data
                                {
                                    unit = "1",
                                    group = type_DataGroup.Generic,
                                    id = "SYSTEM!",
                                    subid = "810",
                                    count = "6",
                                    push ="-1",
                                    threshold= 0.01,
                                    priority = "-1"
                                }
                            }
                        }
                  }
                }
            };
            this.sendMessage = createMessage(flex);
        }

        
        private byte[] createMessage(flexData data)
        {
            //XmlSerializerオブジェクトを作成
            //オブジェクトの型を指定する

            byte[] buf = null;

            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                var writer = System.Xml.XmlWriter.Create(ms, new System.Xml.XmlWriterSettings()
                {
                    Encoding = Encoding.ASCII,
                    Indent = false,
                    OmitXmlDeclaration = false,
                });

                serializer.Serialize(writer, data);
                ms.Position = 0;
                buf = ms.GetBuffer();
            }

            byte[] messageBuf = new byte[buf.Length + 8];
            Array.Copy(BitConverter.GetBytes(MAGIC_NUMBER), 0, messageBuf, 0, 2);
            Array.Copy(BitConverter.GetBytes(VERSION_NUMBER), 0, messageBuf, 2, 2);
            Array.Copy(BitConverter.GetBytes(buf.Length), 0, messageBuf, 4, 4);
            Array.Copy(buf, 0, messageBuf, 8, buf.Length);

            return messageBuf;
        }

        private int ParseHeader(byte[] bytes, int offset)
        {
            var magic   = BitConverter.ToUInt16(bytes, offset);
            var version = BitConverter.ToUInt16(bytes, offset + 2);
            var length  = BitConverter.ToInt32(bytes, offset + 4);
            return length;
        }

        public void Connect(string ip, int port)
        {
            if (socket != null)
            {
                DisConnect();
            }

            this.socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            this.socket.Connect(new IPEndPoint(IPAddress.Parse(ip), port));

            if (this.watchLoop == null)
            {
                this.watchLoop = Task.Run(() =>
                {
                    isStop = false;
                    while (!isStop)
                    {
                        this.socket.Send(this.sendMessage);

                        int receiveLength   = socket.Receive(this.resBuffer);
                        flexData resData    = null;

                        using (var ms = new System.IO.MemoryStream())
                        {
                            try
                            {
                                var length = ParseHeader(this.resBuffer, 0);
                                //System.Diagnostics.Debug.WriteLine(length);
                                ms.Write(this.resBuffer, 8, length);
                                ms.Position = 0;

                                if (length == 0) continue;

                                resData = (flexData)serializer.Deserialize(ms);

                                if (resData.notifications != null)
                                {
                                    //resData.notifications.ToList().ForEach(n =>
                                    //{
                                    //    Console.Error.WriteLine($"{n.code}: {n.message}");
                                    //});

                                    //Thread.Sleep(1000);
                                    continue;
                                }


                                var updateData = resData.dataExchange.dataUpdate[0].data[0].r;

                                var info = new RobotInfo
                                {
                                    Tcp = new Position
                                    {
                                        X = (float)updateData[0],
                                        Y = (float)updateData[1],
                                        Z = (float)updateData[2],
                                        Role = (float)updateData[3],
                                        Pitch = (float)updateData[4],
                                        Yaw = (float)updateData[5],
                                    },
                                    Time = DateTime.Now.ToFileTime(),
                                };
                                this.RobotInfoUpdated?.Invoke(info);

                                Thread.Sleep(1);
                            }
                            catch (System.Exception err)
                            {
                                Console.WriteLine(err.ToString());
                                Thread.Sleep(5000);
                                return;
                            }
                            finally
                            {
                                ms.Dispose();
                            }
                        }
                    }

                });

                IsLogging = true;
            }

        }

        public void DisConnect()
        {
            if (this.watchLoop != null)
            {
                isStop = true;
                this.watchLoop.Wait(2000);
                this.watchLoop = null;
            }

            if (this.socket != null)
            {
                if (this.socket.Connected)
                {
                    this.socket.Close();
                }

                this.socket.Dispose();
                this.socket = null;
            }

            IsLogging = false;

        }

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
                DisConnect();
                disposedValue = true;
            }
        }

        // // TODO: 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
        ~CFD_Logger()
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
