using System;

namespace MinebeaSensorLib.Ethernet
{
    public class MeasureStatus
    {
        private MeasureStatusFlgs flgs;

        public MeasureStatus(UInt16 flgs)
        {
            this.flgs = (MeasureStatusFlgs)Enum.ToObject(typeof(MeasureStatusFlgs),flgs);
        }

        public bool IsSensor1_Enabled => (MeasureStatusFlgs.Sensor1Enabled & flgs) > 0;
        public bool IsSensor2_Enabled => (MeasureStatusFlgs.Sensor2Enabled & flgs) > 0;
        public bool IsSensor3_Enabled => (MeasureStatusFlgs.Sensor3Enabled & flgs) > 0;
        public bool IsSensor4_Enabled => (MeasureStatusFlgs.Sensor4Enabled & flgs) > 0;
        public bool IsSPI => (MeasureStatusFlgs.IsSPI & flgs) > 0;
    }
}
