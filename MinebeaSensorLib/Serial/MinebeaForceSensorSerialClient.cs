using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Buffers.Binary;
using System.IO;
using System.IO.Ports;

namespace MinebeaSensorLib.Serial
{
    public class MinebeaForceSensorSerialClient: IDisposable
    {
        const Int32 bit24Harf = 0x800000;                                
        const Int32 minus     = 0x01000000;

        private List<byte> buf = new List<byte>();

        private const int TIMEOUT_MSEC = 5000;
        public MinebeaForceSensorSerialClient() { }

        private SerialPort serialPort = null;
        private Queue<byte> receiveQueue = new Queue<byte>();
        protected bool disposedValue = false;

        private Task dataLoop = null;
        protected object _locker = new object();

        public void Connect(string comName)
        {
            if (this.serialPort != null)
                this.DisConnect();

            this.serialPort = new SerialPort()
            {
                PortName    = comName,
                BaudRate    = CommandBuilder.BAUDRATE,
                Parity      = Parity.None,
                DataBits    = CommandBuilder.DATABITS,
                StopBits    = StopBits.One,
                NewLine     = "\r",
            };

            this.serialPort.DataReceived += SerialPort_DataReceived;

            this.serialPort.Open();
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var serial = (SerialPort)sender;
            if (!serial.IsOpen)
                return;
        

            var readble = serial.BytesToRead;
            var bytes = new byte[readble];

            serial.Read(bytes, 0, readble);

            lock (_locker)
            {
                for (int i = 0; i < readble; i++)
                {
                    this.receiveQueue.Enqueue(bytes[i]);
                }
            }          
        }

        public void DisConnect()
        {
            if (this.serialPort != null)
            {
                try
                {
                    if (this.serialPort.IsOpen)
                    {
                        this.serialPort.Close();
                    }
                }
                finally
                {
                    this.serialPort.Dispose();
                    this.serialPort = null;
                }
            }
        }

        private bool isContinue = false;

        private byte[] sendAndResponse(byte[] cmd, int readByte)
        {
            serialPort.Write(cmd, 0, cmd.Length);
            return GetSerialBytes(readByte);
        }

        /// <summary>
        /// コントローラを選択するコマンド。
        /// コントローラは、マルチスレーブ接続を考慮した設計となっており、Board ID が割り当てられています。
        /// BoardID を選択することで、コントローラ との接続を 確立します。
        /// ※ コントローラは、仮想 COM ポートでマルチスレーブ接続が可能ですので、 Board ID は 0x00 固定となります。
        /// </summary>
        /// <returns></returns>
        public ResponseStatus BootSelect()
        {
            var ret = sendAndResponse(CommandBuilder.BootSelect_Command, 2);
            return (ResponseStatus)ret[0];
        }

        /// <summary>
        /// コントローラのファームウェアバージョンを読み出すコマンド。
        /// </summary>
        /// <returns></returns>
        public string FirmwareVersion()
        {
            var ret = sendAndResponse(CommandBuilder.FirmwareVersion_Command, 6);
            ResponseStatus status = (ResponseStatus)ret[0];
            if (status != ResponseStatus.OK)
                throw new Exception(status.ToString());

            if (ret[1] != 4)
                throw new Exception($"data length error: {ret[1]}");

            return $"{ret[2]}.{ret[3]}.{ret[4]}.{ret[5]}";
        }

        /// <summary>
        /// コントローラ上の電源（LDO ）の ON/OFF を切り替えるコマンド。
        /// VDD45は力覚センサのアナログ電源、 VDD12 は力覚センサのデジタル電源です。
        /// </summary>
        /// <param name="vDD"></param>
        /// <param name="isPowerOn"></param>
        /// <returns></returns>
        public ResponseStatus PowerSwitch(VDD vDD, bool isPowerOn)
        {
            var ret = sendAndResponse(CommandBuilder.PowerSwitch_Command(vDD, isPowerOn), 2);
            return (ResponseStatus)ret[0];
        }

        /// <summary>
        /// 力覚センサの各軸に該当する AFE に個別にアクセスするために、アクセスする軸を選択するコマンド。
        /// 力覚センサをIdle 状態にするためには、 各軸に該当する AFE を個別に Idle 状態にしなければなりません。
        /// そのため、Idle コマンドを発行する前に、アクセスする軸を選択します。
        /// 本コマンドでアクセスする軸を選択し、その後、Idle コマンドで選択した軸の AFE を Idle 状態にします。
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ResponseStatus AxisSelect(AxisID id)
        {
            var ret = sendAndResponse(CommandBuilder.AxisSelect_Command(id), 2);
            return (ResponseStatus)ret[0];
        }

        /// <summary>
        /// 力覚センサ（各軸に該当する AFE ）を Idle 状態にするコマンド。
        /// コントローラがマトリクス係数を読み出すには、力覚センサをIdle 状態にする必要があります。
        /// 力覚センサをIdle 状態にするには、各軸に該当する AFE を個別に Idle 状態にします。
        /// 本コマンドを実行する前に、必ずAxis Select コマンドでアクセスする軸を選択してください。
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ResponseStatus Idle()
        {
            var ret = sendAndResponse(CommandBuilder.Idle_Command, 2);
            return (ResponseStatus)ret[0];
        }

        /// <summary>
        /// コントローラが力覚センサからマトリクス係数を読み出すコマンド。
        /// ※ 測定を開始する前に、マトリクス係数を読み出しておいてください。
        /// </summary>
        /// <returns></returns>
        public ResponseStatus Bootload()
        {
            var ret = sendAndResponse(CommandBuilder.Bootload_Command, 2);
            return (ResponseStatus)ret[0];
        }
        /// <summary>
        /// コントローラが保持しているマトリクス 係数 をホストに転送するコマンド。
        /// ※ 予め Bootload コマンドを実行しておいてください。
        /// Axis IDと Coefficient ID を組み合わせて、読み出してください。
        /// </summary>
        /// <param name="axisID"></param>
        /// <param name="coefficientID"></param>
        /// <returns></returns>
        public int Coefficient(AxisID axisID, CoefficientID coefficientID)
        {
            var ret = sendAndResponse(CommandBuilder.Coefficient_Command(axisID, coefficientID), 6);
            ResponseStatus status = (ResponseStatus)ret[0];
            if (status != ResponseStatus.OK)
                throw new Exception(status.ToString());

            if (ret[1] != 4)
                throw new Exception($"data length error: {ret[1]}");

            var coeBytes = new byte[4];
            Array.Copy(ret, 2, coeBytes, 0, 4);

            return BitConverter.ToInt32(coeBytes);
        }

        /// <summary>
        /// コントローラが力覚センサからデータを取得する間隔を設定するコマンド。
        /// 設定有効範囲は、0usec 10sec(10,000,000usec) で、 usec 単位で設定してください。
        /// 本コマンドが未実行の場合、0usec 設定となります。
        /// 非常に短い間隔に設定した場合は、実動作として、データ取得時間、マトリクス演算時間、データの出力時間が
        /// 発生しますので、設定どおりの間隔になるとは限りません。
        /// 力覚センサのデータ更新間隔は780usec(typ.) ですので、 1000usec 以上の設定を推奨します。
        /// 本コマンドは、Start コマンド実行前のみ有効です 。
        /// </summary>
        /// <returns></returns>
        public ResponseStatus IntervalMeasure(UInt32 intervalMicroSec = 1000)
        {
            var ret = sendAndResponse(CommandBuilder.IntervalMeasure_Command(intervalMicroSec), 2);
            return (ResponseStatus)ret[0];
        }

        /// <summary>
        /// コントローラが力覚センサの温度補正を更新する間隔を設定するコマンド。
        /// 設定有効範囲は、0usec 10sec(10,000,000usec) で、 usec 単位で設定してください。
        /// 本コマンドが未実行の場合、0usec 設定となります。
        /// 設定値が0usec の場合 、初回のみの温度更新となり、以降、温度更新は 行われません 。
        /// 非常に短い間隔に設定 した場合は、実動作として、データ取得 時間、マトリクス演算時間、データの出力時間が発生しますので、設定どおりの間隔になるとは限りません。
        /// 力覚センサが温度更新を実施するとデータ更新にかかる時間が、5000usec(type.) となりますので、5000usec以上の設定を推奨します。
        /// 本設定を5000usec とした場合、データの更新間隔は 5000usec となります。
        /// 本コマンドは、Start コマンド実行前のみ有効です。
        /// </summary>
        /// <param name="intervalMicroSec"></param>
        /// <returns></returns>
        public ResponseStatus IntervalRestart(UInt32 intervalMicroSec = 1000)
        {
            var ret = sendAndResponse(CommandBuilder.IntervalRestart_Command(intervalMicroSec), 2);
            return (ResponseStatus)ret[0];
        }

        private static Int32 Byte24CnvInt32(byte[] src, int offset)
        {
            byte[] buf = new byte[4] { src[offset + 2], src[offset + 1], src[offset], 0x00 };
            Int32 intval = BitConverter.ToInt32(buf);

            if (intval >= MinebeaForceSensorSerialClient.bit24Harf)
                intval -= MinebeaForceSensorSerialClient.minus;

            return intval;

            //if (src.Length - offset < 3) throw new Exception("Int24BitesToInt: バイトデータが足りません");

            //var byte32 = new byte[] { 0x00, src[offset], src[offset + 1], src[offset + 2] };
            //int intVal = BinaryPrimitives.ReadInt32BigEndian(new ReadOnlySpan<byte>(byte32, 0, 4));
            //if (intVal >= 0x800000) intVal -= 0x01000000;

            //return intVal;
        }

        public ResponseStatus Start(Action<SensorData> callback)
        {
            var ret = sendAndResponse(CommandBuilder.Start_Command, 2);
            ResponseStatus status = (ResponseStatus)ret[0];

            if (status != ResponseStatus.OK)
                throw new Exception(status.ToString());

            if (status != ResponseStatus.OK || ret[1] != 0)
                throw new Exception($"data length error: {ret[1]}");

            this.dataLoop = Task.Run(() =>
            {
                isContinue = true;
                while (isContinue)
                {
                    try
                    {
                        var dataByte = sendAndResponse(CommandBuilder.Start_Command, 25);

                        this.buf.AddRange(dataByte);

                        var status2 = (ResponseStatus)dataByte[0];

                        if (status2 != ResponseStatus.OK)
                            continue;
                            //Console.WriteLine(status2);
                        //throw new Exception(status2.ToString());

                        //if (dataByte[1] != 0x15)
                        //if (dataByte[1] != 0x17)
                        //    throw new Exception($"data length error: {dataByte[1]}");



                        var floatArrBuf = new float[6] { 0, 0, 0, 0, 0, 0 };
                        for (int i = 0; i < 6; i++)
                        {
                            var startbyte = (i * 3) + 4;
                            var intVal = MinebeaForceSensorSerialClient.Byte24CnvInt32(dataByte, startbyte);

                            var div = (i > 2) ? 100000 : 1000;

                            floatArrBuf[i] = Convert.ToSingle(intVal) / div;
                            //floatArrBuf[i] = ((float)BitConverter.ToInt32(readByteBuf));
                            //floatArrBuf[i] = BitConverter.ToSingle(readByteBuf);
                        }

                        //var readByteBuf = new byte[4];

                        //readByteBuf[0] = dataByte[24];
                        //readByteBuf[1] = dataByte[23];
                        //readByteBuf[2] = dataByte[22];
                        //readByteBuf[3] = 0;

                        //var time = MinebeaForceSensorSerialClient.Byte24CnvInt32(dataByte, 22);

                        var bytes = new byte[] { dataByte[24], dataByte[23], dataByte[22], 0 };
                        var time = BitConverter.ToUInt32(bytes);


                        var data = new SensorData()
                        {
                            Fx = floatArrBuf[0],
                            Fy = floatArrBuf[1],
                            Fz = floatArrBuf[2],
                            Mx = floatArrBuf[3],
                            My = floatArrBuf[4],
                            Mz = floatArrBuf[5],
                            //Time = Convert.ToSingle(time) / 65536.0f,
                            Time = Convert.ToUInt32(time),

                        };
                        callback(data);
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine(e.ToString());
                        throw;
                    }
                   
                }
            });

            return status;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ResponseStatus Stop()
        {
            isContinue = false;

            if (this.dataLoop != null)
            {
                if(!this.dataLoop.IsCompleted)
                {
                    try
                    {
                        this.dataLoop.Wait();
                    }
                    catch (Exception err)
                    {
                        Console.Error.WriteLine(err.ToString());
                    }
                }                

                this.dataLoop = null;              
            }


            do
            {
                receiveQueue.Clear();

                serialPort.Write(CommandBuilder.Stop_Command, 0, CommandBuilder.Stop_Command.Length);
                Thread.Sleep(100);
            } while (receiveQueue.Count != 2);

            var res = this.GetSerialBytes(2);

            return (ResponseStatus)res[0];
        }

        private byte[] GetSerialBytes(int readNum)
        {
            int timeout = TIMEOUT_MSEC;
            int count   = 0;
            byte[] ret  = new byte[readNum];
            bool flg    = false;
            do
            {
                lock (_locker)
                {
                    if (this.receiveQueue.Count > 0)
                    {
                        ret[count] = receiveQueue.Dequeue();
                        count++;
                        flg = true;
                    }
                }
                if (flg)
                {

                }
                else
                {
                    Thread.Sleep(1);
                    timeout--;
                }
                
            } while (count < readNum && timeout > 0);

            return (timeout <= 0) ? new byte[] { 0xff, 0 } : ret;
        }



        #region Dispose       

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

                this.DisConnect();

                disposedValue = true;
            }
        }

        ~MinebeaForceSensorSerialClient()
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