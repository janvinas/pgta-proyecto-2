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
        CAT021,
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
        public List<string>? TargetReportDescriptor { get; set; } // DI3
        public float? SlantAngle { get; set; } // DI4 theta
        public float? Azimuth { get; set; } // DI4 rho
        public ushort? Mode3A { get; set; } // DI5
        public FlightLevel? FlightLevel { get; set; } // DI6
        public RadarPlotCharacteristics? RadarPlotCharacteristics { get; set; } // DI7
        public uint? Address { get; set; } // DI8
        public string? Identification {  get; set; } // DI9
        public ushort? TrackNum { get; set; } // DI11
        public double? GS { get; set; } // DI13
        public double? Heading { get; set; } // DI13
        public float? Latitude { get; set; } //DI
        public float? Longitude { get; set; }
    }
}
