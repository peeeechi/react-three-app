using System;
using System.Buffers.Binary;

namespace MinebeaSensorLib
{
    public class SensorData
    {
        public const int SENSOR_DATA_LENGTH = 18;
        public SensorData(float fx, float fy, float fz, float mx, float my, float mz)
        {
            Fx = fx;
            Fy = fy;
            Fz = fz;
            Mx = mx;
            My = my;
            Mz = mz;
        }
        public float Fx { get; private set; }
        public float Fy { get; private set; }
        public float Fz { get; private set; }
        public float Mx { get; private set; }
        public float My { get; private set; }
        public float Mz { get; private set; }

        public static SensorData Bytes2Data(byte[] bytes, int offset)
        {
            if(bytes.Length - offset < SENSOR_DATA_LENGTH) throw new Exception("Create SensorData: バイトデータが足りません");

            float fx = Convert.ToSingle(SensorData.Int24BitesToInt(bytes, offset)) / 1000;
            float fy = Convert.ToSingle(SensorData.Int24BitesToInt(bytes, offset + 3)) / 1000;
            float fz = Convert.ToSingle(SensorData.Int24BitesToInt(bytes, offset + 6)) / 1000;
            float mx = Convert.ToSingle(SensorData.Int24BitesToInt(bytes, offset + 9)) / 100000;
            float my = Convert.ToSingle(SensorData.Int24BitesToInt(bytes, offset + 12)) / 100000;
            float mz = Convert.ToSingle(SensorData.Int24BitesToInt(bytes, offset + 15)) / 100000;

            return new SensorData(fx, fy, fz, mx, my, mz);

        }

        public static Int32 Int24BitesToInt(byte[] bytes, int offset)
        {
            if(bytes.Length - offset < 3) throw new Exception("Int24BitesToInt: バイトデータが足りません");

            var byte32 = new byte[]{0x00, bytes[offset],  bytes[offset + 1],  bytes[offset + 2]};
            int intVal = BinaryPrimitives.ReadInt32BigEndian(new ReadOnlySpan<byte>(byte32, 0, 4));
            if(intVal >= 0x800000) intVal -= 0x01000000;

            return intVal;
        }
    }
}
