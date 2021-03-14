using System;
using System.Threading;
using System.Text;
using System.Numerics;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Net.Sockets;
using System.Xml.Serialization;
using System.Net;
using userApp;
using Newtonsoft.Json;

namespace api_server
{
    public class RobotInfoHandler : WebSocketHandler, IDisposable
    {
        private Task watchLoop = null;
        private Task sendLoop = null;
        private bool isStop = false;
        private Settings settings = null;
        private Socket socket;
        private RobotInfo robotInfo;
        private readonly SemaphoreSlim infoSemaphore = new SemaphoreSlim(1, 1);
        private byte[] resBuffer = new byte[4096];
        private XmlSerializer serializer = new XmlSerializer(typeof(flexData));
        private byte[] sendMessage;

        const UInt16 MAGIC_NUMBER   = 0xBA5E;
        const UInt16 VERSION_NUMBER = 0x0001;

        public RobotInfoHandler(WebSocketObjectHolder webSocketObjectHolder, Settings settings): base(webSocketObjectHolder)
        {
            this.settings = settings;
            

            this.robotInfo = new RobotInfo
            {
                Tcp = new Position
                {
                    X = 0,
                    Y = 0,
                    Z = 0,
                    Role = 0,
                    Pitch = 0,
                    Yaw = 0
                }
            };

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
                                    priority = "5"
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
    

        private void Connect()
        {
            if (socket != null)
            {
                DisConnect();
            }

            this.socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            this.socket.Connect(new IPEndPoint(IPAddress.Parse(settings.CfdIP), settings.CfdPort));
        }

        private void DisConnect()
        {
            if (this.watchLoop != null)
            {
                isStop          = true;
                this.watchLoop.Wait(2000);
                this.watchLoop  = null;
            }

            if (this.sendLoop != null)
            {
                isStop = true;
                this.sendLoop.Wait(2000);
                this.sendLoop = null;
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

        }

        private int ParseHeader(byte[] bytes, int offset)
        {
            var magic       = BitConverter.ToUInt16(bytes, offset);
            var version     = BitConverter.ToUInt16(bytes, offset + 2);
            var length      = BitConverter.ToInt32(bytes, offset + 4);
            return length;
        }

        public override async Task OnConnected(WebSocket client)
        {
            await base.OnConnected(client);

            var socketid = WebSocketObjectHolder.GetId(client);
            Console.WriteLine($"socket created: {socketid}");

            if (this.watchLoop == null)
            {
                this.watchLoop = Task.Run(async () =>
                {
                    Connect();
                    isStop = false;

                    while (!isStop)
                    {
                        this.socket.Send(this.sendMessage);

                        int receiveLength = socket.Receive(this.resBuffer);

                        flexData resData = null;

                        using (var ms = new System.IO.MemoryStream())
                        {
                            try
                            {
                                var length = ParseHeader(this.resBuffer, 0);
                                System.Diagnostics.Debug.WriteLine(length);
                                // ms.Write(res, 8, receiveLength - 8);
                                ms.Write(this.resBuffer, 8, length);
                                ms.Position = 0;

                                //Console.WriteLine(Encoding.UTF8.GetString(ms.GetBuffer()));

                                resData = (flexData)serializer.Deserialize(ms);

                                if (resData.notifications != null)
                                {
                                    resData.notifications.ToList().ForEach(n =>
                                    {
                                        Console.Error.WriteLine($"{n.code}: {n.message}");
                                    });

                                    Thread.Sleep(1000);
                                    continue;
                                }


                                var updateData = resData.dataExchange.dataUpdate[0].data[0].r;

                                await infoSemaphore.WaitAsync();
                                try
                                {
                                    this.robotInfo.Tcp.X = (float)updateData[0];
                                    this.robotInfo.Tcp.Y = (float)updateData[1];
                                    this.robotInfo.Tcp.Z = (float)updateData[2];
                                    this.robotInfo.Tcp.Role = (float)updateData[3];
                                    this.robotInfo.Tcp.Pitch = (float)updateData[4];
                                    this.robotInfo.Tcp.Yaw = (float)updateData[5];
                                }
                                finally
                                {
                                    infoSemaphore.Release();
                                }

                                Thread.Sleep(5);
                            }
                            catch (System.Exception err)
                            {
                                Console.WriteLine(err.ToString());
                                Thread.Sleep(5000);
                                throw;
                            }
                            finally
                            {
                                ms.Dispose();
                            }


                        }
                    }

                });
            }

            Thread.Sleep(100);

            if (sendLoop == null)
            {
                this.sendLoop = Task.Run(async () =>
                {
                    isStop = false;
                    string str = null;
                    while (!isStop)
                    {
                        
                        await infoSemaphore.WaitAsync();
                        try
                        {
                            str = JsonConvert.SerializeObject(this.robotInfo);
                        }
                        finally
                        {
                            infoSemaphore.Release();
                        }

                        //Console.WriteLine(str);
                        await SendMessageToAllAsync(str, AppConst.NON_BOM_UTF8_ENCORDING);
                        Thread.Sleep(10);
                    }
                });
            }
            
            await Task.CompletedTask;
        }

        public override async Task ReceiveAsync(WebSocket client, WebSocketReceiveResult result, byte[] buffer)
        {
            var socketid = WebSocketObjectHolder.GetId(client);

            var message = AppConst.NON_BOM_UTF8_ENCORDING.GetString(buffer, 0, result.Count);

            await Task.CompletedTask;   // Todo: コマンド受信処理

        }

        public override async Task OnDisconnected(WebSocket client)
        {
            var socketId = WebSocketObjectHolder.GetId(client);

            await base.OnDisconnected(client);
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

                DisConnect();
                base.Dispose(disposing);
                disposedValue = true;
                Console.WriteLine($"{nameof(RobotInfoHandler)} Disposed.");
            }
        }

        // TODO: 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
        ~RobotInfoHandler()
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
