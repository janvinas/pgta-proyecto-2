using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace AsterixParser
{
    public enum CAT
    {
        CAT028,
        CAT048,
    }

    public class RadarPlotCharacteristics
    {
        public double? SRL { get; set; } // SRL in degrees
        public byte? SSR { get; set; }   // Number of Received Replies for (M)SSR
        public sbyte? SAM { get; set; }  // Amplitude of M(SSR) Reply in dBm
        public double? PRL { get; set; } // PRL in degrees
        public sbyte? PAM { get; set; }  // Amplitude of PSR Reply in dBm
        public double? RPD { get; set; } // Difference in Range Betweeen PSR and SSR (PSR-SSR) in NM
        public double? APD { get; set; } // Difference in azimuth between PSR and SSR plot in degrees
        public byte? SCO { get; set; }   // Score
        public double? SCR { get; set; } // Signal / Clutter Radio
        public double? RW { get; set; }  // Range Width in NM
        public double? AR { get; set; }  // Ambiguous Range in NM
    }

    public class FlightLevel(float? flightLevel, bool garbledCode, bool codeNotValidated)
    {
        public float? flightLevel = flightLevel;
        public Boolean garbledCode = garbledCode;
        public Boolean codeNotValidated = codeNotValidated;
    }

    public class AsterixMessage
    {
        public CAT Cat { get; set; }
        public byte SIC { get; set; } // DI1
        public byte SAC { get; set; } // DI1
        public double? TimeOfDay { get; set; } // DI2
        //TODO data item 3
        //TODO data item 4
        //TODO data item 5
        public FlightLevel? FlightLevel { get; set; } // DI6
        public RadarPlotCharacteristics? RadarPlotCharacteristics { get; set; } // DI7
        public uint? Address { get; set; } // DI8
        public string? Identification {  get; set; } // DI9
        public ushort? TrackNum { get; set; } // DI11
        public double? GS { get; set; } // DI13
        public double? Heading { get; set; } // DI13
    }
}
