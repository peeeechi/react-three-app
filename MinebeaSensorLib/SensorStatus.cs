using System;
using System.Buffers.Binary;

namespace MinebeaSensorLib
{
   public class SensorStatus
    {
        public SensorStatus(byte[] bytes, int offset)
        {
            if (bytes.Length - offset < 6) throw new Exception("SensorStatus: バイト数が足りません");

            // var statusCode = BitConverter.ToUInt16(bytes, offset);
            var statusCode = BinaryPrimitives.ReadUInt16BigEndian(new ReadOnlySpan<byte>(bytes, offset, 2));
            // var measureCode = BitConverter.ToUInt16(bytes, offset + 2);
            var measureCode = BinaryPrimitives.ReadUInt16BigEndian(new ReadOnlySpan<byte>(bytes, offset + 2, 2));
            this.State = (StateID)Enum.ToObject(typeof(StateID), bytes[offset + 4]);
        }

        public StatusCode StatusCode { get; private set; }
        public MeasureStatus MeasureStatus { get; private set; }
        public StateID State { get; private set; }
    }
}
