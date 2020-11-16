using System;
using System.Buffers.Binary;

namespace MinebeaSensorLib
{
    
    public static class MinebeaForceSensorUtil
    {
        public static StatusCode GetStatusCode(this byte[] bytes, int offset)
        {
            if(bytes.Length - offset < 2) throw new Exception("GetStatusCode: バイト数が不足しています");

            // UInt16 val = BitConverter.ToUInt16(bytes, offset);
            UInt16 val = BinaryPrimitives.ReadUInt16BigEndian(new ReadOnlySpan<byte>(bytes, offset, 2));

            return (StatusCode)Enum.ToObject(typeof(StatusCode), val);
        }

        public static MeasureStatus GetMeasureStatus(this byte[] bytes, int offset)
        {
            if(bytes.Length - offset < 2) throw new Exception("GetMeasureStatus: バイト数が不足しています");

            // UInt16 flgs = BitConverter.ToUInt16(bytes, offset);
            UInt16 flgs = BinaryPrimitives.ReadUInt16BigEndian(new ReadOnlySpan<byte>(bytes, offset, 2));

            return new MeasureStatus(flgs);
        }

        public static SensorDataResponse GetSensorDataResponse(this byte[] bytes, int offset)
        {
            if(bytes.Length - offset < 100) throw new Exception("GetMeasureStatus: バイト数が不足しています");

            return new SensorDataResponse(bytes, offset);
        }

        public static SensorStatus GetSensorStatus(this byte[] bytes, int offset)
        {
            if(bytes.Length - offset < 6) throw new Exception("GetSensorStatus: バイト数が不足しています");

            return new SensorStatus(bytes, offset);

        }

    }
}