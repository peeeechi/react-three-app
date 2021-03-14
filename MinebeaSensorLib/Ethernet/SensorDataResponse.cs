using System;
using System.Buffers.Binary;

namespace MinebeaSensorLib.Ethernet
{
     public class SensorDataResponse
    {
        public StatusCode StatusCode { get; private set; }
        public MeasureStatus MeasureStatus { get; private set; }
        public UInt16 MeasureCount { get; private set; }
        public UInt32 MeasureTime { get; private set; }

        public SensorData Sensor1Data {get; private set;}
        public SensorData Sensor2Data {get; private set;}
        public SensorData Sensor3Data {get; private set;}
        public SensorData Sensor4Data {get; private set;}
        public SensorData Sensor5Data {get; private set;}

        public SensorDataResponse(byte[] bytes, int offset)
        {
            if(bytes.Length - offset < 100) throw new Exception("Create SensorDataResponse: バイトデータが足りません");

            this.StatusCode = bytes.GetStatusCode(0);
            this.MeasureStatus = bytes.GetMeasureStatus(2);
            // this.MeasureCount = BitConverter.ToUInt16(bytes, 4);
            this.MeasureCount = BinaryPrimitives.ReadUInt16BigEndian(new ReadOnlySpan<byte>(bytes, 4, 2));
            // this.MeasureTime  = BitConverter.ToUInt32(bytes, 6);
            this.MeasureTime  = BinaryPrimitives.ReadUInt32BigEndian(new ReadOnlySpan<byte>(bytes, 6, 4));

            this.Sensor1Data = SensorData.Bytes2Data(bytes, 10);
            this.Sensor2Data = SensorData.Bytes2Data(bytes, 28);
            this.Sensor3Data = SensorData.Bytes2Data(bytes, 46);
            this.Sensor4Data = SensorData.Bytes2Data(bytes, 64);
            this.Sensor5Data = SensorData.Bytes2Data(bytes, 82);
        }
    }
}
