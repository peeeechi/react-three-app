using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using MinebeaSensorLib;

namespace api_server
{
    public class ForceSensorManager: IDisposable
    {
        public delegate Task SensorDataUpdatedHandler(SensorDataResponse data);

        const string SENSOR_IP = "192.168.0.200";
        const int SENSOR_PORT = 1366;
        const int LOCAL_PORT = 3500;

        const int SLEEP_TIME = 100;


        #region Public Methods        

        public void Connect()
        {
            try
            {
                this.Client = new MinebeaForceSeosorClient(SENSOR_IP, SENSOR_PORT, LOCAL_PORT);

                this.Client.Open();

                this.IsConnecting = true;


                StatusCode ret = this.Client.M_Reset();
                Console.WriteLine($"Reset: {ret}");
                Thread.Sleep(SLEEP_TIME);

                // ret = this.Client.M_SelectI2C();
                ret = this.Client.M_SelectSPI(SensorUseFlg.Sensor1IsEnabled | SensorUseFlg.Sensor2IsEnabled);
                Console.WriteLine($"Select: {ret}");
                Thread.Sleep(SLEEP_TIME);

                ret = this.Client.M_Boot();
                Console.WriteLine($"Boot: {ret}");
                // Thread.Sleep(SLEEP_TIME);
                Thread.Sleep(500);

                ret = this.Client.M_Start();
                Console.WriteLine($"Start: {ret}");
                Thread.Sleep(SLEEP_TIME);

                Console.WriteLine($"Data ���W��...");

                this.FetchSensorDataLoop = Task.Run(async () =>
                {
                    while (this.IsConnecting)
                    {
                        var data = this.Client.M_Data();

                        await this.SensorDataUpdated?.Invoke(data);
                    }

                    Console.WriteLine("");

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

        public MinebeaForceSeosorClient Client { get; set; } = null;

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
                    // TODO: �}�l�[�W�h��Ԃ�j�����܂� (�}�l�[�W�h �I�u�W�F�N�g)
                }

                // TODO: �A���}�l�[�W�h ���\�[�X (�A���}�l�[�W�h �I�u�W�F�N�g) ��������A�t�@�C�i���C�U�[���I�[�o�[���C�h���܂�
                // TODO: �傫�ȃt�B�[���h�� null �ɐݒ肵�܂�
                disposedValue = true;
                this.DisConnect();
            }
        }

        // // TODO: 'Dispose(bool disposing)' �ɃA���}�l�[�W�h ���\�[�X���������R�[�h���܂܂��ꍇ�ɂ̂݁A�t�@�C�i���C�U�[���I�[�o�[���C�h���܂�
        ~ForceSensorManager()
        {
            // ���̃R�[�h��ύX���Ȃ��ł��������B�N���[���A�b�v �R�[�h�� 'Dispose(bool disposing)' ���\�b�h�ɋL�q���܂�
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // ���̃R�[�h��ύX���Ȃ��ł��������B�N���[���A�b�v �R�[�h�� 'Dispose(bool disposing)' ���\�b�h�ɋL�q���܂�
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
