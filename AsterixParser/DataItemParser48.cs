using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AsterixParser
{
    internal class DataItemParser48
    {

        private readonly AsterixMessage message;

        /*Jan decia de hacerlo asi:
        public Action[] functions = {}
        */
        //Para poner el valor de "k" y poderlo modificar se necesita poner ref y por lo cual crear el delegate 
        public delegate void DelegateFunctions(ref int k, byte[] body);
        public DelegateFunctions[] functions;
        // las funciones deben definirse en el constructor porque son propiedades de la instancia (no estáticas)

        public DataItemParser48(AsterixMessage message) {
            this.message = message;

            functions =
            [
                DataItem1,
                DataItem2,
                DataItem3,
                DataItem4,
                DataItem5,
                DataItem6,
                DataItem7,
                DataItem8,
                DataItem9,
                DataItem10,
                DataItem11,
                DataItem12,
                DataItem13,
                DataItem14,
                DataItem15,
                DataItem16,
                DataItem17,
                DataItem18,
                DataItem19,
                DataItem20,
                DataItem21
            ];
        }

        public void DataItem1(ref int k, byte[] body) // I048/010
        {
            byte SAC = body[k];
            byte SIC = body[k + 1];
            message.SIC = SIC;
            message.SAC = SAC;
            k += 2;
        }

        public void DataItem2(ref int k, byte[] body) //Comprobar si esta bien, ya que el primero que sale realmente en el de prueba no existe
        {
            int date = (body[k + 2] | body[k + 1] << 8 | (body[k] << 16));
            float seconds = date/128f;
            message.TimeOfDay = seconds;
            k += 3;
        }

        public void DataItem3(ref int k, byte[] body) //No esta hecho, solo lo salto, no lo entiendo jeje (Pau)
        {
            Console.WriteLine("(DF-3)");
            Queue<string> TargetReport = new Queue<string>();

            string TYP = null;
            string SIM = null;
            string RDP = null;
            string SPI = null;
            string RAB = null;
            string TST = null;
            string ERR = null;
            string XPP = null;
            string ME = null;
            string MI = null;
            string FOE = null;
            string ADSB_EP = null;
            string ADSB_VAL = null;
            string SCN_EP = null;
            string SCN_VAL = null;
            string PAI_EP = null;
            string PAI_VAL = null;

            int bits;

            bits = (body[k] >> 5) & 0b111;
            switch (bits)
            {
                case 0b000:
                    TYP = "No detection";
                    break;
                case 0b001:
                    TYP = "PSR";
                    break;
                case 0b010:
                    TYP = "SSR";
                    break;
                case 0b011:
                    TYP = "SSR + PSR";
                    break;
                case 0b100:
                    TYP = "ModeS All-Call";
                    break;
                case 0b101:
                    TYP = "ModeS Roll-Call";
                    break;
                case 0b110:
                    TYP = "ModeS All-Call + PSR";
                    break;
                case 0b111:
                    TYP = "ModeS Roll-Call + PSR";
                    break;
            }
            TargetReport.Enqueue(TYP);

            if (body[k] >> 4 == 1) SIM = "Simulated";
            else SIM = "Actual";
            TargetReport.Enqueue(SIM);

            if (body[k] >> 3 == 1) RDP = "Chain 2";
            else RDP = "Chain 1";
            TargetReport.Enqueue(RDP);

            if (body[k] >> 2 == 1) SPI = "SPI";
            else SPI = "No SPI";
            TargetReport.Enqueue(SPI);

            if (body[k] >> 1 == 1) RAB = "Field Monitor";
            else RAB = "Aircraft";
            TargetReport.Enqueue(RAB);

            bits = body[k] >> 0 & 0b1;
            if ( bits == 1)
            {
                k += 1;
                if (body[k] >> 7 == 1) TST = "Test";
                else TST = "Real";
                TargetReport.Enqueue(TST);

                if (body[k] >> 6 == 1) ERR = "Extended";
                else ERR = "No Extended";
                TargetReport.Enqueue(ERR);

                if (body[k] >> 5 == 1) XPP = "No X-Pulse";
                else XPP = "X-Pulse";
                TargetReport.Enqueue(XPP);

                if (body[k] >> 4 == 1) ME = "Military Emergency";
                else ME = "No Military Emergency";
                TargetReport.Enqueue(ME);

                if (body[k] >> 3 == 1) MI = "Military ID";
                else MI = "No Military ID";
                TargetReport.Enqueue(MI);

                bits = (body[k] >> 1) & 0b11;
                switch (bits)
                {
                    case 0b00:
                        FOE = "No Mode 4";
                        break;
                    case 0b01:
                        FOE = "Friendly target";
                        break;
                    case 0b10:
                        FOE = "Unknown target";
                        break;
                    case 0b11:
                        FOE = "No reply";
                        break;
                }
                TargetReport.Enqueue(FOE);

                bits = body[k] >> 0 & 0b1;
                if (bits == 1)
                {
                    k += 1;

                    if (body[k] >> 7 == 1) ADSB_EP = "ADSB populated";
                    else ADSB_EP = "ADSB not populated";
                    TargetReport.Enqueue(ADSB_EP);

                    if (body[k] >> 6 == 1) ADSB_VAL = "Available";
                    else ADSB_VAL = "Not Available";
                    TargetReport.Enqueue(ADSB_VAL);

                    if (body[k] >> 5 == 1) SCN_EP = "SCN populated";
                    else SCN_EP = "SCN not populated";
                    TargetReport.Enqueue(SCN_EP);

                    if (body[k] >> 4 == 1) SCN_VAL = "Available";
                    else SCN_VAL = "Not Available";
                    TargetReport.Enqueue(SCN_VAL);

                    if (body[k] >> 3 == 1) PAI_EP = "PAI populated";
                    else PAI_EP = "PAI not populated";
                    TargetReport.Enqueue(PAI_EP);

                    if (body[k] >> 2 == 1) PAI_VAL = "Available";
                    else PAI_VAL = "Not Available";
                    TargetReport.Enqueue(PAI_VAL);

                }
            }

            k += 1;
            Console.WriteLine("Target Report data: ");
            foreach (string data in TargetReport) Console.WriteLine("· " + data);

        }

        public void DataItem4(ref int k, byte[] body) //Creo que no esta acabado
        {
            Console.WriteLine("(DF-4)");
            ushort rho_raw = (ushort)(body[k + 1] | (body[k] << 8));
            ushort theta_raw = (ushort)(body[k + 2] << 8 | body[k + 3]);
            float rho = rho_raw / 256f;
            float theta = theta_raw * 360f / (float)Math.Pow(2, 16);

            Console.WriteLine("Rho: " + rho);
            Console.WriteLine("Theta: " + theta);

            k += 4;
        }

        public void DataItem5(ref int k, byte[] body) //No esta hecho, solo lo salto
        {
            Console.WriteLine("(DF-5)");

            string[] Config = new string[3];
            string V = null;
            string G = null;
            string L = null;

            ushort raw = (ushort)(body[k + 1] | (body[k] << 8));
            int bits;

            bits = (raw >> 15) & 0b1;
            if (bits == 1) V = "Not Validated";
            else V = "Validated";

            bits = (raw >> 14) & 0b1;
            if (bits == 1) G = "Garbled code";
            else G = "Default";

            bits = (raw >> 13) & 0b1;
            if (bits == 1) L = "Not Last Scan";
            else L = "Replay";

            int mode3A = raw & 0x0FFF;

            Config = [V,G,L];

            Console.WriteLine("Config 3/A: ");
            foreach (string config in Config) Console.WriteLine("· " + config);
            string octalCode = Convert.ToString(mode3A, 8);
            Console.WriteLine($"Mode-3/A en octal: {octalCode}");

            k += 2;
        }

        public void DataItem6(ref int k, byte[] body)
        {
            bool notValidated = ((body[k] >> 7) & 1) == 1;
            bool garbledCode = ((body[k] >> 6) & 1) == 1;

            ushort FL_raw = (ushort)(body[k + 1] | (body[k] << 8));
            FL_raw &= 0x3FFF;

            message.FlightLevel = new FlightLevel(FL_raw / 4f, garbledCode, notValidated);
            k += 2;
        }

        public static Queue<int> PrimarySubfieldDecoder(ref int k, byte[] body) // Para leer subfield primario del I048/130 (por ahora)
        {
            bool fx = true;
            Queue<int> nSubfield = new Queue<int>();
            int subfieldindex = 1;
            while (fx)
            {
                byte b = body[k];
                for (int i = 7; i > 0; i--)
                {
                    int v = (b >> i) & 1;
                    if (v == 1) nSubfield.Enqueue(subfieldindex);
                    subfieldindex++;
                }

                int bit = b & 1;
                if (bit == 0) fx = false;
                k++;
            }
            return nSubfield;
        }

        public void DataItem7(ref int k, byte[] body) // I048/130 Radar Plot Characteristics
        {
            RadarPlotCharacteristics ch = new();
            int n;
            Queue<int> nSubfield = PrimarySubfieldDecoder(ref k, body);
            while (nSubfield.Count > 0)
            {
                n = nSubfield.Dequeue();
                switch (n)
                {
                    case 1: // Subfield 1 SRL
                        double srl = body[k];
                        srl = srl * 360 / Math.Pow(2,13); // SRL in degrees
                        ch.SRL = srl;
                        k++;
                        break;
                    case 2: // Subfield 2 SRR
                        ch.SSR = body[k]; // Number of Received Replies for (M)SSR
                        k++;
                        break;
                    case 3: // Subfield 3 SAM
                        sbyte sam = (sbyte)body[k]; // Amplitude of M(SSR) Reply in dBm
                        ch.SAM = sam;
                        k++;
                        break;
                    case 4: // Subfield 4 PRL
                        double prl = body[k] * 360 / Math.Pow(2,13); // PRL in degrees
                        ch.PRL = prl;
                        k++;
                        break;
                    case 5: // Subfield 5 PAM
                        sbyte pam = (sbyte)body[k]; // Amplitude of PSR Reply in dBm
                        ch.PAM = pam;
                        k++;
                        break;
                    case 6: // Subfield 6 RPD
                        sbyte rpd_byte = (sbyte)body[k];
                        ch.RPD = rpd_byte / 256d; // Difference in Range Betweeen PSR and SSR (PSR-SSR) in NM
                        k++;
                        break;
                    case 7: // Subfield 7 APD
                        sbyte apd_byte = (sbyte) body[k];
                        ch.APD = apd_byte * 360 / Math.Pow(2, 14); // Difference in azimuth between PSR and SSR plot in degrees
                        k++;
                        break;
                    case 8: // Subfield 8 SCO
                        ch.SCO = body[k];
                        k++;
                        break;
                    case 9: // Subfield 9 SCR
                        ushort scr_combined = (ushort)((body[k] << 8) | body[k+1]);
                        ch.SCR = scr_combined * 0.1; // Signal / Clutter Radio
                        k += 2;
                        break;
                    case 10: // Subfield 10 RW
                        ushort rw_combined = (ushort)((body[k] << 8) | body[k + 1]);
                        ch.RW = rw_combined / 256d; // Range Width in NM
                        k += 2;
                        break;
                    case 11: // Subfield 11 AR
                        ushort ar_combined = (ushort)((body[k] << 8) | body[k + 1]);
                        ch.AR = ar_combined / 256d; // Ambiguous Range in NM
                        k += 2;
                        break;
                }
            }

            message.RadarPlotCharacteristics = ch;
            
        }

        public void DataItem8(ref int k, byte[] body) // I048/220 Aircraft Address
        {
            uint address = (uint)( body[k] << 16 | body[k+1] << 8 | body[k+2]);
            message.Address = address;
            k += 3; // 3 octets
        }

        public char DecodificarChar6bit(int val) // Mapejar IA-5 (6 bits) a caràcters
        {
            if (val >= 0 && val <= 25)
                return (char)('A' + val - 1);
            else if (val >= 48 && val <= 57)
                return (char)('0' + (val - 48));
            else if (val == 32)
                return ' ';
            else
                return ' '; // valor no definit
        }
        public void DataItem9(ref int k, byte[] body) // I048/240 Aircraft Identification
        {
            byte[] id = { body[k], body[k + 1], body[k + 2], body[k + 3], body[k + 4], body[k + 5] };
            ulong bits = 0; // Concatenar els 6 bytes en un nombre de 48 bits (ulong)
            for (int i = 0; i < 6; i++)
            {
                bits <<= 8;
                bits |= id[i];
            }
            StringBuilder sb = new StringBuilder();
            // Extraiem 8 blocs de 6 bits
            for (int i = 7; i >= 0; i--)
            {
                int shift = i * 6;
                int val6bit = (int)((bits >> shift) & 0x3F); // Màscara de 6 bits
                sb.Append(DecodificarChar6bit(val6bit));
            }
            message.Identification = sb.ToString().Trim();
            k += 6; // 6 octets
        }

        public void DataItem10(ref int k, byte[] body)
        {
            Console.WriteLine("(DF-10)");

            byte REP = body[k];
            Console.WriteLine("Number of REPs of BDS: " + REP);

            k += 1 + 8 * REP;
        }

        public void DataItem11(ref int k, byte[] body)
        {
            ushort TrackNum = (ushort)(body[k + 1] | (body[k] << 8));
            TrackNum &= 0x0FFF;
            message.TrackNum = TrackNum;
            k += 2;
        }

        public void DataItem12(ref int k, byte[] body)
        {
            k += 4;
        }

        public void DataItem13(ref int k, byte[] body)
        {
            ushort GS_raw = (ushort)(body[k + 1] | (body[k] << 8));
            ushort Head_raw = (ushort)(body[k + 3] | (body[k + 2] << 8));

            message.GS = GS_raw / Math.Pow(2, 14) * 3600;
            message.Heading = Head_raw * 360f / Math.Pow(2, 16);
            k += 4;
        }

        public void DataItem14(ref int k, byte[] body)
        {
            Console.WriteLine("(DF-14)");

            Queue<string> TrackStatus = new Queue<string>();

            string CNF = null;
            string RAD = null;
            string DOU = null;
            string MAH = null;
            string CDM = null;
            string TRE = null;
            string GHO = null;
            string SUP = null;
            string TCC = null;

            int bits;

            if (body[k] >> 7 == 1) CNF = "Tentative Track";
            else CNF = "Confirmed Track";
            TrackStatus.Enqueue(CNF);

            bits = (body[k] >> 5) & 0b11;
            switch (bits)
            {
                case 0b00:
                    RAD = "Combined Track";
                    break;
                case 0b01:
                    RAD = "PSR Track";
                    break;
                case 0b10:
                    RAD = "SSR/Mode S Track";
                    break;
                case 0b11:
                    RAD = "Invalid";
                    break;
            }
            TrackStatus.Enqueue(RAD);

            if (body[k] >> 4 == 1) DOU = "Low confidence in plot to track association";
            else DOU = "Normal confidence";
            TrackStatus.Enqueue(DOU);

            if (body[k] >> 3 == 1) MAH = "No horizontal man. sensed";
            else MAH = "Horizontal man. sensed";
            TrackStatus.Enqueue(MAH);

            bits = (body[k] >> 1) & 0b11;
            switch (bits)
            {
                case 0b00:
                    CDM = "Maintaining";
                    break;
                case 0b01:
                    CDM = "Climbing";
                    break;
                case 0b10:
                    CDM = "Descending";
                    break;
                case 0b11:
                    CDM = "Unknown";
                    break;
            }
            TrackStatus.Enqueue(CDM);

            if (body[k] >> 0 == 1)
            {
                k += 1;

                if (body[k] >> 7 == 1) TRE = "End of track lifetime(last report for this track)";
                else TRE = "Track still alive";
                TrackStatus.Enqueue(TRE);

                if (body[k] >> 6 == 1) GHO = "Ghost target Track";
                else GHO = "True target track";
                TrackStatus.Enqueue(GHO);

                if (body[k] >> 5 == 1) SUP = "No";
                else SUP = "Yes";
                TrackStatus.Enqueue(SUP);

                if (body[k] >> 4 == 1) TCC = "Slant range correction";
                else TCC = "Radar Plane";
                TrackStatus.Enqueue(TCC);
            }

            // Lo printeamos por consola, el puto Pau dice que no, pero me suda la polla
            Console.WriteLine("Track status data: ");
            foreach (string data in TrackStatus) Console.WriteLine(data);

            k += 1;
        }

        public void DataItem15(ref int k, byte[] body)
        {
            Console.WriteLine("(DF-15)");

            k += 4;
        }

        public void DataItem16(ref int k, byte[] body)
        {
            Console.WriteLine("(DF-16)");

            if (body[k] >> 1 == 1) k += 2;
            else k += 1;
        }

        public void DataItem17(ref int k, byte[] body)
        {
            Console.WriteLine("(DF-17)");

            k += 2;
        }

        public void DataItem18(ref int k, byte[] body)
        {
            Console.WriteLine("(DF-18)");

            k += 4;
        }

        public void DataItem19(ref int k, byte[] body)
        {
            Console.WriteLine("(DF-19)");

            k += 2;
        }

        public void DataItem20(ref int k, byte[] body)
        {
            Console.WriteLine("(DF-20)");
            int dk = 1; // Starts by 1 as it must jump the base octet

            if (body[k] >> 7 == 1) dk += 2; // Presence of subfield #1 (2 Octets)
            if (body[k] >> 6 == 1) dk += 7; // Presence of subfield #2 (7 octets)

            k += dk;
        }

        public void DataItem21(ref int k, byte[] body)
        {
            Console.WriteLine("(DF-21)");

            string[] Capability = new string[8];

            string COM = null;
            string STAT = null;
            string SI = null;
            string MSSC = null;
            string ARC = null;
            string AIC = null;
            int B1A = 0;
            int B1B = 0;


            ushort raw = (ushort)(body[k + 1] | (body[k] << 8));

            Console.WriteLine($"Valor binario : {Convert.ToString(raw, 2).PadLeft(16, '0')}");

            int bits;

            bits = (raw >> 13) & 0b111;
            switch (bits)
            {
                case 0:
                    COM = "No communications capability";
                    break;
                case 1:
                    COM = "Comm. A and Comm. B capability";
                    break;
                case 2:
                    COM = "Comm. A, Comm. B and Uplink ELM";
                    break;
                case 3:
                    COM = "Comm. A, Comm. B, Uplink ELM and Downlink ELM";
                    break;
                case 4:
                    COM = "Level 5 Transponder capability";
                    break;
                case 5:
                    break;
                case 6:
                    break;
                case 7:
                    break;
            }

            bits = (raw >> 10) & 0b111;
            switch (bits)
            {
                case 0:
                    STAT = "No alert, no SPI, aircraft airborne";
                    break;
                case 1:
                    STAT = "No alert, no SPI, aircraft on ground";
                    break;
                case 2:
                    STAT = "Alert, no SPI, aircraft airborne";
                    break;
                case 3:
                    STAT = "Alert, no SPI, aircraft on ground";
                    break;
                case 4:
                    STAT = "Alert, SPI, aircraft airborne or on ground";
                    break;
                case 5:
                    STAT = "No alert, SPI, aircraft airborne or on ground";
                    break;
                case 6:
                    STAT = "Not assigned";
                    break;
                case 7:
                    STAT = "Unknown";
                    break;
            }

            bits = (raw >> 9) & 0b1;
            if (bits == 1) SI = "II-Code";
            else SI = "SI-Code";

            bits = (raw >> 7) & 0b1;
            if (bits == 1) MSSC = "Yes";
            else MSSC = "No";

            bits = (raw >> 6) & 0b1;
            if (bits == 1) ARC = "25 ft resolution";
            else ARC = "100 ft resolution";

            bits = (raw >> 5) & 0b1;
            if (bits == 1) AIC = "Yes";
            else AIC = "No";

            B1A = (raw >> 4) & 0b1;

            B1B = raw & 0b1111;

            Capability = [ COM,STAT,SI,MSSC,ARC,AIC,Convert.ToString(B1A), Convert.ToString(B1B)];

            Console.WriteLine("Capability and Flight Status: ");
            foreach (string data in Capability) Console.WriteLine("· " + data);

            k += 2;
        }
    }
}
