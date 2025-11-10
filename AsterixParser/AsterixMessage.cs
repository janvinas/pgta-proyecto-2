using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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

    public class TrackStatus
    {
        public string? CNF { get; set; }
        public string? RAD { get; set; }
        public string? DOU { get; set; }
        public string? MAH { get; set; }
        public string? CDM { get; set; }
        public string? TRE { get; set; }
        public string? GHO { get; set; }
        public string? SUP { get; set; }
        public string? TCC { get; set; }

    }

    public class  I048230
    {
        public string? COM { get; set; }
        public string? STAT { get; set; }
        public string? SI { get; set; }
        public string? MSSC { get; set; }
        public string? ARC { get; set; }
        public string? AIC { get; set; }
        public int? B1A { get; set; }
        public int? B1B { get; set; }
    }

    public class BDS 
    {
        public string? BDSsTabla { get; set; }
        public string? BDSsCSV { get; set; }
        public int? statusMCP { get; set; }
        public int? MCP { get; set; }
        public int? statusFMS { get; set; }
        public int? FMS { get; set; }
        public int? statusBARO { get; set; }
        public float? BARO { get; set; }
        public int? infoMCP { get; set; }
        public int? VNAV { get; set; }
        public int? ALTflag { get; set; }
        public int? APPR { get; set; }
        public int? statusTarget { get; set; }
        public string? TargetALT { get; set; }

        public float? statusROLL { get; set; }
        public float? ROLL { get; set; }
        public float? statusTTA { get; set; }
        public float? TTA { get; set; }
        public float? statusGS { get; set; }
        public float? GS { get; set; }
        public float? statusTAR { get; set; }
        public float? TAR { get; set; }
        public float? statusTAS { get; set; }
        public float? TAS { get; set; }

        public float? statusMH { get; set; }
        public float? MH { get; set; }
        public float? statusIAS { get; set; }
        public float? IAS { get; set; }
        public float? statusMACH { get; set; }
        public float? MACH { get; set; }
        public float? statusBAROV { get; set; }
        public float? BAROV { get; set; }
        public float? statusIVV { get; set; }
        public float? IVV { get; set; }

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
        public double? TimeOfDay { get; set; } // DI2 CAT48 y DI12 CAT21
        public List<string>? TargetReportDescriptor048 { get; set; } // DI3 I048/20
        public List<string>? TargetReportDescriptor021 { get; set; } // DI2 I021/40
        public double? Distance { get; set; } // DI4 rho
        public double? Azimuth { get; set; } // DI4 theta
        public ushort? Mode3A { get; set; } // DI5
        public string? configMode3A { get; set; }
        public FlightLevel? FlightLevel { get; set; } // DI6
        public RadarPlotCharacteristics? RadarPlotCharacteristics { get; set; } // DI7
        public uint? Address { get; set; } // DI8 CAT48 y DI11 CAT21
        public string? Identification {  get; set; } // DI9
        public ushort? TrackNum { get; set; } // DI11 CAT48
        public double? GS { get; set; } // DI13
        public double? Heading { get; set; } // DI13
        public TrackStatus? TrackStatus { get; set; } // DI14 CAT48
        public I048230? I048230 { get; set; } // DI21 CAT48
        public double? Latitude { get; set; } //DI7 CAT21
        public double? Longitude { get; set; } //DI7 CAT21
        public float? BPS { get; set; } // DI48 CAT21
        public BDS? BDS { get; set; } // DI7
        public double? QNHcorrection { get; set; }

    }
}
