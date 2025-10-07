using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace AsterixParser
{
    public class AsterixMessage
    {
        public byte SIC { get; set; } 
        public byte SAC { get; set; }
        public double? TimeOfDay { get; set; }
        //TODO data item 3
        //TODO data item 4
        //TODO data item 5
        public float? FlightLevel { get; set; }
    }
}
