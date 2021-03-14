using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;

namespace MinebeaSensorLib.Serial
{
    public class SensorData
    {
        public float Fx { get; set; }
        public float Fy { get; set; }
        public float Fz { get; set; }
        public float Mx { get; set; }
        public float My { get; set; }
        public float Mz { get; set; }
        public UInt32 Time { get; set; }
    }
}

