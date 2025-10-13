using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AsterixParser
{
    internal class DataItemParser21
    {
        public static void DataItem1(ref int k, byte[] body) // I021/010 Data Source Identification
        {
            Console.WriteLine("(DF-1)");
            byte SAC = body[k];
            if (SAC == 20) Console.WriteLine("ESP");
            else Console.WriteLine("Otro pais");
            byte SIC = body[k + 1];
            Console.WriteLine($"Identification Code {SIC}");
            k += 2;
        }

        public static void DataItem2(ref int k, byte[] body) // I021/040 Target Report Descriptor
        {
            Console.WriteLine("(DF-2)");
            int[] primarysubfield = new int[8];
            for (int i=0; i<8; i++)
            {
                primarysubfield[i] = (body[k] >> (7 - i)) & 1; // Vector de bits de l'octet del Subfield Primari
            }
            
            byte atp = 0;
            atp |= (byte)(primarysubfield[0] << 2);
            atp |= (byte)(primarysubfield[1] << 1);
            atp |= (byte)(primarysubfield[2] << 0); // Composem el byte ATP (Address Type)

            byte arc = 0;
            arc |= (byte)(primarysubfield[3] << 1); // Composem el byte ARC (Altitude Reporting Capability)
            arc |= (byte)(primarysubfield[4] << 0);

            Console.WriteLine("Address Type: ");
            if (atp == 0) Console.WriteLine("24-bit ICAO Address."); // Reportings del ATP
            else if (atp == 1) Console.WriteLine("Duplicate Address.");
            else if (atp == 2) Console.WriteLine("Surface Vehicle Address");
            else if (atp == 3) Console.WriteLine("Anonymous Address.");
            else Console.WriteLine("Reserved for future use.");

            Console.WriteLine("Altitude Reporting Capability: "); // Reportings del ARC
            if (arc == 0) Console.WriteLine("25 ft.");
            else if (arc == 1) Console.WriteLine("100 ft.");
            else if (arc == 2) Console.WriteLine("Unknown");
            else if (arc == 3) Console.WriteLine("Invalid");

            Console.WriteLine("Range Check"); // Reportings del RC
            if (primarysubfield[5] == 1) Console.WriteLine("Range Check passed, CPR validation pending");
            else if (primarysubfield[5] == 0) Console.WriteLine("Default");

            Console.WriteLine("Report Type"); // Reportings del RAB
            if (primarysubfield[6] == 1) Console.WriteLine("Report from field monitor (fixed transponder).");
            else if (primarysubfield[6] == 0) Console.WriteLine("Report from target transponder.");

            k++; // Primary Subfield END

            if (primarysubfield[7] == 1) // Anem a la First Extension
            {
                int[] firstextension = new int[8];
                for (int i = 0; i < 8; i++)
                {
                    firstextension[i] = (body[k] >> (7 - i)) & 1; // Vector de bits de l'octet de la primera extensió
                }

                byte cl = 0; // Composem el byte CL (Confidence Level)
                cl |= (byte)(firstextension[5] << 1);
                cl |= (byte)(firstextension[6] << 0);

                Console.WriteLine("Differential Correction: "); // Reportings del DCR
                if (firstextension[0] == 1) Console.WriteLine("Differential Correction (ADS-B)");
                else Console.WriteLine("No Differential Correction (ADS-B)");

                Console.WriteLine("Ground Bit Setting: "); // Reportings del GBS
                if (firstextension[1] == 1) Console.WriteLine("Set.");
                else Console.WriteLine("Not set.");

                Console.WriteLine("Simulated Target: "); // Reportings del SIM
                if (firstextension[2] == 1) Console.WriteLine("Simulated target report.");
                else Console.WriteLine("Actual target report.");

                Console.WriteLine("Test Target: "); // Reportings del TST
                if (firstextension[3] == 1) Console.WriteLine("Test Target.");
                else Console.WriteLine("Default.");

                Console.WriteLine("Selected Altitud Available: "); // Reportings del SAA
                if (firstextension[4] == 1) Console.WriteLine("Equipment not capable.");
                else Console.WriteLine("Equipment capable.");
                
                Console.WriteLine("Confidence Level: ");
                if (cl == 0) Console.WriteLine("Report valid.");
                else if (cl == 1) Console.WriteLine("Report suspect.");
                else if (cl == 2) Console.WriteLine("No information.");
                else if (cl == 3) Console.WriteLine("Reserved for future use.");

                k++; // First Extension END

                if (firstextension[7] == 1) // Anem a la Second Extension
                {
                    int[] secondextension = new int[8];
                    for (int i = 0; i < 8; i++)
                    {
                        secondextension[i] = (body[k] >> (7 - i)) & 1; // Vector de bits de l'octet de la segona extensió
                    }

                    Console.WriteLine("Independent Position Check: "); // Reportings del IPC
                    if (secondextension[2] == 1) Console.WriteLine("Failed.");
                    else Console.WriteLine("Default.");
                     
                    Console.WriteLine("No-go Bit Status: "); // Reportings del NOGO
                    if (secondextension[3] == 1) Console.WriteLine("Set.");
                    else Console.WriteLine("Not set.");

                    Console.WriteLine("Compact Position Reporting: "); // Reportings del CPR
                    if (secondextension[4] == 1) Console.WriteLine("Failed.");
                    else Console.WriteLine("Correct.");

                    Console.WriteLine("Local Decoding Position Jump: "); // Reportings del LDPJ
                    if (secondextension[5] == 1) Console.WriteLine("LDPJ detected.");
                    else Console.WriteLine("LDPJ not detected.");

                    Console.WriteLine("Range Check: "); // Reportings del RCF
                    if (secondextension[6] == 1) Console.WriteLine("Failed.");
                    else Console.WriteLine("Default.");

                    k++; // Second Extension END

                    if (secondextension[7] == 1) // Anem a la Third Extension
                    {
                        // Aquí què? :(
                    }
                }
            }
        }

        public static void DataItem3(ref int k, byte[] body)
        {
            Console.WriteLine("(3)");
            k += 2;
        }

        public static void DataItem4(ref int k, byte[] body)
        {
            Console.WriteLine("(4)");
            k++;
        }

        public static void DataItem5(ref int k, byte[] body)
        {
            Console.WriteLine("(5)");
            k += 3;
        }

        public static void DataItem6(ref int k, byte[] body)
        {
            Console.WriteLine("(6)");
            k += 6;
        }

        public static void DataItem7(ref int k, byte[] body) // I021/131 High-Resolution Position in WGS-84 Co-ordinates
        {
            Console.WriteLine("(DF-7)");
            int lat_i = (body[k] << 24) | (body[k + 1] << 16) | (body[k + 2] << 8) | body[k + 3]; // Composem el integer de latitud
            int lon_i = (body[k + 4] << 24) | (body[k + 5] << 16) | (body[k + 6] << 8) | body[k + 7]; // Composem el integer de longitud

            float lat = lat_i;
            lat = lat * 180 / (float)(Math.Pow(2, 30));
            float lon = lon_i;
            lon = lon * 180 / (float)(Math.Pow(2, 30));

            if (lat >= 0) Console.WriteLine($"Latitude: {lat} degrees North.");
            else Console.WriteLine($"Latitude: {lat} degrees South.");
            if (lon >= 0) Console.WriteLine($"Longitude: {lon} degrees East.");
            else Console.WriteLine($"Longitude: {lon} degrees West.");

            k += 8;
        }

        public static void DataItem8(ref int k, byte[] body)
        {
            Console.WriteLine("(8)");
            k += 3;
        }

        public static void DataItem9(ref int k, byte[] body)
        {
            Console.WriteLine("(9)");
            k += 2;
        }

        public static void DataItem10(ref int k, byte[] body)
        {
            Console.WriteLine("(10)");
            k += 2;
        }

        public static void DataItem11(ref int k, byte[] body) // I021/080 Target Address
        {
            Console.WriteLine("(DF-11)");
            byte[] address = { body[k], body[k + 1], body[k + 2] };
            string hexAddress = BitConverter.ToString(address).Replace("-", "");
            Console.WriteLine(hexAddress);
            k += 3; // 3 octets
        }

        public static void DataItem12(ref int k, byte[] body)
        {
            Console.WriteLine("(DF-12)");
            int date = (body[k + 2] | body[k + 1] << 8 | (body[k] << 16));
            float hour = date / 128f;
            Console.WriteLine("Time: " + TimeSpan.FromSeconds(hour));
            k += 3;
        }

        public static void DataItem13(ref int k, byte[] body)
        {
            Console.WriteLine("(13)");
            k += 4;
        }

        public static void DataItem14(ref int k, byte[] body)
        {
            Console.WriteLine("(14)");
            k += 3;
        }

        public static void DataItem15(ref int k, byte[] body)
        {
            Console.WriteLine("(15)");
            k += 4;
        }

        public static void DataItem16(ref int k, byte[] body)
        {
            Console.WriteLine("(16)");
            k += 2;
        }

        public static void DataItem17(ref int k, byte[] body)
        {
            Console.WriteLine("(17)");

            if ((body[k] >> 0 & 1) == 1)
            {
                k += 1;
                if ((body[k] >> 0 & 1) == 1)
                {
                    k += 1;
                    if ((body[k] >> 0 & 1) == 1) k += 1;
                }
            }
            k += 1;
        }

        public static void DataItem18(ref int k, byte[] body)
        {
            Console.WriteLine("(18)");
            k += 1;
        }

        public static void DataItem19(ref int k, byte[] body)
        {
            Console.WriteLine("(19)");

            ushort raw = (ushort)(body[k + 1] | (body[k] << 8));

            int mode3A = raw & 0x0FFF;

            string octalCode = Convert.ToString(mode3A, 8);
            Console.WriteLine($"Mode-3/A en octal: {octalCode}");

            k += 2;
        }

        public static void DataItem20(ref int k, byte[] body)
        {
            Console.WriteLine("(20)");
            k += 2;
        }

        public static void DataItem21(ref int k, byte[] body)
        {
            Console.WriteLine("(21)");

            ushort FL_raw = (ushort)(body[k + 1] | (body[k] << 8));
            float FL = FL_raw / 4f;

            k += 2;
        }

        public static void DataItem22(ref int k, byte[] body)
        {
            Console.WriteLine("(22)");
            k += 2;
        }

        public static void DataItem23(ref int k, byte[] body)
        {
            Console.WriteLine("(23)");
            k += 1;
        }

        public static void DataItem24(ref int k, byte[] body)
        {
            Console.WriteLine("(24)");
            k += 2;
        }

        public static void DataItem25(ref int k, byte[] body)
        {
            Console.WriteLine("(25)");
            k += 2;
        }

        public static void DataItem26(ref int k, byte[] body)
        {
            Console.WriteLine("(26)");
            k += 4;
        }

        public static void DataItem27(ref int k, byte[] body)
        {
            Console.WriteLine("(27)");
            k += 2;
        }

        public static void DataItem28(ref int k, byte[] body)
        {
            Console.WriteLine("(28)");
            k += 3;
        }

        public static char DecodificarChar6bit(int val)
        {
            if (val >= 0 && val <= 25)
                return (char)('A' + val - 1);
            else if (val >= 48 && val <= 57)
                return (char)('0' + (val - 48));
            else if (val == 32)
                return ' ';
            else
                return ' ';
        }
        public static void DataItem29(ref int k, byte[] body)
        {
            Console.WriteLine("(29)");

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
            Console.WriteLine(sb.ToString().Trim());

            k += 6;
        }

        public static void DataItem30(ref int k, byte[] body)
        {
            Console.WriteLine("(30)");
            k += 1;
        }

        public static void DataItem31(ref int k, byte[] body)
        {
            Console.WriteLine("(31)");

            int dk = 0;
            if ((body[k] >> 7 & 1)== 1) dk += 2;
            if ((body[k] >> 6 & 1)== 1) dk += 2;
            if ((body[k] >> 5 & 1)== 1) dk += 2;
            if ((body[k] >> 4 & 1)== 1) dk += 1;

            k += dk;
        }

        public static void DataItem32(ref int k, byte[] body)
        {
            Console.WriteLine("(32)");
            k += 2;
        }

        public static void DataItem33(ref int k, byte[] body)
        {
            Console.WriteLine("(33)");
            k += 2;
        }

        public static void DataItem34(ref int k, byte[] body)
        {
            Console.WriteLine("(34)");

            int dk = 1;
            byte b = body[k];

            if ((b >> 7 & 1) == 1) dk += 1;

            if ((b >> 6 & 1) == 1)
            {
                int REP = body[k + dk];
                dk += 1 + 15 * REP;
            }

            k += dk;
        }

        public static void DataItem35(ref int k, byte[] body)
        {
            Console.WriteLine("(35)");
            k += 1;
        }

        public static void DataItem36(ref int k, byte[] body)
        {
            Console.WriteLine("(36)");
            k += 1;
        }

        public static void DataItem37(ref int k, byte[] body)
        {
            Console.WriteLine("(37)");

            if ((body[k] >> 0 & 1) == 1) k += 1;
            k += 1;
        }

        public static void DataItem38(ref int k, byte[] body)
        {
            Console.WriteLine("(38)");
            k += 1;
        }

        public static void DataItem39(ref int k, byte[] body)
        {
            Console.WriteLine("(39)");

            byte REP = body[k];
            k += 1 + 8 * REP;
        }

        public static void DataItem40(ref int k, byte[] body)
        {
            Console.WriteLine("(40)");
            k += 7;
        }

        public static void DataItem41(ref int k, byte[] body)
        {
            Console.WriteLine("(41)");
            k += 1;
        }

        public static void DataItem42(ref int k, byte[] body)
        {
            Console.WriteLine("(42)");

            int length = body[k];
            k ++;

            int k_old = k;
            k++;
            if ((body[k_old] >> 7 & 1) == 1)
            {
                int BPS_raw = (ushort)(body[k + 1] | (body[k] << 8)) & 0x0FFF;
                float BPS = BPS_raw * 0.1f;

                k += 2;
            }
            if ((body[k_old] >> 6 & 1) == 1)
            {
                int SelH_raw = (ushort)(body[k + 1] | (body[k] << 8)) & 0x03FF;
                float SelH = SelH_raw * 0.703125f;

                string HRD;
                if ((body[k] >> 3 & 1) == 0) HRD = "True North";
                else HRD = "Magnetic North";

                string Stat;
                if ((body[k] >> 2 & 1) == 0) Stat = "Data Unavailable/Invalid";
                else Stat = "Data Available/Valid";

                k += 2;
            }
            if ((body[k_old] >> 5 & 1) == 1)
            {
                string AP;
                if ((body[k] >> 7 & 1) == 1) AP = "Autopilot engaged";
                else AP = "Autopilot not engaged";

                string VN;
                if ((body[k] >> 6 & 1) == 1) VN = "VNAV active";
                else VN = "VNAV not active";

                string AH;
                if ((body[k] >> 5 & 1) == 1) AH = "Altitude Hold engaged";
                else AH = "Altitude Hold not engaged";

                string AM;
                if ((body[k] >> 4 & 1) == 1) AM = "Approach Mode active";
                else AM = "Approach Mode not active";

                string MFM_EP;
                if ((body[k] >> 5 & 1) == 1) MFM_EP = "Element populated";
                else MFM_EP = "Element not populated";

                string MFM_VAL;
                if ((body[k] >> 4 & 1) == 1) MFM_VAL = "MCP/FCU ModeBits populated";
                else MFM_VAL = "MCP/FCU ModeBits not populated";

                k += 1;
            }
            if ((body[k_old] >> 4 & 1) == 1)
            {
                int GAOlat_raw = (body[k] & 0b1110_0000) >> 5;
                int GAOlat = GAOlat_raw * 2;

                int GAOlong_raw = body[k] & 0x1F;
                int GAOlong = GAOlong_raw * 2;

                k += 1;
            }
            if ((body[k_old] >> 3 & 1) == 1)
            {
                string STP;
                if ((body[k] >> 7 & 1) == 1) STP = "Aircraft has stopped";
                else STP = "Aircraft has not stopped";

                string HTS;
                if ((body[k] >> 6 & 1) == 1) HTS = "Heading/Ground Track data is valid";
                else HTS = "Heading/Ground Track data is not valid";

                string HTT;
                if ((body[k] >> 5 & 1) == 1) HTT = "Ground Track provided";
                else HTT = "Heading data provided";

                string HRD;
                if ((body[k] >> 4 & 1) == 1) HRD = "Magnetic North";
                else HRD = "True North";

                int GSS_raw = (ushort)(body[k + 1] | (body[k] << 8) >> 4) & 0x07FF;
                float GSS = GSS_raw * 0.125f;

                if (body[k+1] >> 0 == 1)
                {
                    k += 2;

                    int HGT_raw = (body[k] >> 1);
                    float HGT = HGT_raw * 2.8125f;

                    k += 1;
                }
                else k += 2;
            }
            // ESTE ES UN PALAZO INCREIBLE (2.8 STA)
            if ((body[k_old] >> 2 & 1) == 1)
            {
                Console.WriteLine("STA: Aircraft Status");

                // Primary subfield
                byte staByte = body[k];
                k++;

                string ES = (staByte >> 7 & 1) == 1 ? "1090ES IN capable" : "Not 1090ES IN capable";
                string UAT = (staByte >> 6 & 1) == 1 ? "UAT IN capable" : "Not UAT IN capable";

                string RCE_EP = (staByte >> 5 & 1) == 1 ? "RCE Element Populated" : "RCE Element Not Populated";
                int RCE = (staByte >> 4 & 0b11);
                string RCE_VAL;
                switch (RCE)
                {
                    case 1: RCE_VAL = "TABS"; break;
                    case 3: RCE_VAL = "Other RCE"; break;
                    default: RCE_VAL = "Not RCE"; break;
                }

                string RRL_EP = (staByte >> 2 & 1) == 1 ? "RRL Element Populated" : "RRL Element Not Populated";
                string RRL_VAL = (staByte >> 1 & 1) == 1 ? "Reply Rate Limiting active" : "Reply Rate Limiting not active";

                Console.WriteLine($"  ES: {ES} | UAT: {UAT} | {RCE_EP} ({RCE_VAL}) | {RRL_EP} ({RRL_VAL})");

                bool FX = (staByte & 0b0000_0001) == 1;
                // --- First extension ---
                if (FX)
                {
                    byte ext1 = body[k];
                    k++;

                    string PS3_EP = (ext1 >> 7 & 1) == 1 ? "PS3 Element Populated" : "PS3 Element Not Populated";
                    int PS3 = (ext1 >> 4 & 0b111);
                    string PS3_VAL;
                    switch (PS3)
                    {
                        case 1: PS3_VAL = "General emergency"; break;
                        case 2: PS3_VAL = "UAS/RPAS - Lost Link"; break;
                        case 3: PS3_VAL = "Minimum fuel"; break;
                        case 4: PS3_VAL = "No communications"; break;
                        case 5: PS3_VAL = "Unlawful interference"; break;
                        case 6: PS3_VAL = "Aircraft in Distress Automatic Activation"; break;
                        case 7: PS3_VAL = "Aircraft in Distress Manual Activation"; break;
                        default: PS3_VAL = "Not emergency"; break;
                    }

                    string TPW_EP = (ext1 >> 3 & 1) == 1 ? "TPW Element Populated" : "TPW Element Not Populated";
                    int TPW = (ext1 >> 1 & 0b11);
                    string TPW_VAL;
                    switch (TPW)
                    {
                        case 1: TPW_VAL = "70W"; break;
                        case 2: TPW_VAL = "125W"; break;
                        case 3: TPW_VAL = "200W"; break;
                        default: TPW_VAL = "Unavailable"; break;
                    }

                    Console.WriteLine($"  PS3: {PS3_EP} ({PS3_VAL}) | {TPW_EP} ({TPW_VAL})");

                    FX = (ext1 & 1) == 1;
                }

                // --- Second extension ---
                if (FX)
                {
                    byte ext2 = body[k];
                    k++;

                    string TSI_EP = (ext2 >> 7 & 1) == 1 ? "TSI Element Populated" : "TSI Element Not Populated";
                    int TSI = (ext2 >> 5 & 0b11);
                    string TSI_VAL;
                    switch (TSI)
                    {
                        case 1: TSI_VAL = "Transponder #1"; break;
                        case 2: TSI_VAL = "Transponder #2"; break;
                        case 3: TSI_VAL = "Transponder #3"; break;
                        default: TSI_VAL = "Unknown"; break;
                    }

                    string MUO_EP = (ext2 >> 4 & 1) == 1 ? "MUO Element Populated" : "MUO Element Not Populated";
                    string MUO_VAL = (ext2 >> 3 & 1) == 1 ? "Unmanned Operation" : "Manned Operation";

                    string RWC_EP = (ext2 >> 2 & 1) == 1 ? "RWC Element Populated" : "RWC Element Not Populated";
                    string RWC_VAL = (ext2 >> 1 & 1) == 1 ? "RWC Corrective Alert active" : "RWC Corrective Alert not active";

                    Console.WriteLine($"  TSI: {TSI_EP} ({TSI_VAL}) | {MUO_EP} ({MUO_VAL}) | {RWC_EP} ({RWC_VAL})");

                    FX = (ext2 & 1) == 1;
                }

                // --- Third extension ---
                if (FX)
                {
                    byte ext3 = body[k];
                    k++;

                    string DAA_EP = (ext3 >> 7 & 1) == 1 ? "DAA Element Populated" : "DAA Element Not Populated";
                    int DAA = (ext3 >> 5 & 0b11);
                    string DAA_VAL;
                    switch (DAA)
                    {
                        case 1: DAA_VAL = "RWC/RA/OCM Capability"; break;
                        case 2: DAA_VAL = "RWC/OCM Capability"; break;
                        case 3: DAA_VAL = "Invalid"; break;
                        default: DAA_VAL = "No RWC Capability"; break;
                    }

                    string DF17CA_EP = (ext3 >> 4 & 1) == 1 ? "DF17CA Element Populated" : "DF17CA Element Not Populated";
                    int DF17CA = (ext3 >> 1 & 0b1111);
                    string DF17CA_VAL = $"CA Code: {DF17CA}";

                    Console.WriteLine($"  DAA: {DAA_EP} ({DAA_VAL}) | {DF17CA_EP} ({DF17CA_VAL})");

                    FX = (ext3 & 1) == 1;
                }

                // --- Fourth extension ---
                if (FX)
                {
                    byte ext4 = body[k];
                    k++;

                    string SVH_EP = (ext4 >> 7 & 1) == 1 ? "SVH Element Populated" : "SVH Element Not Populated";
                    int SVH = (ext4 >> 5 & 0b11);
                    string SVH_VAL;
                    switch (SVH)
                    {
                        case 1: SVH_VAL = "Horizontal Only"; break;
                        case 2: SVH_VAL = "Blended"; break;
                        case 3: SVH_VAL = "Vertical/Horizontal per intruder"; break;
                        default: SVH_VAL = "Vertical Only"; break;
                    }

                    string CATC_EP = (ext4 >> 4 & 1) == 1 ? "CATC Element Populated" : "CATC Element Not Populated";
                    int CATC = (ext4 >> 1 & 0b1111);
                    string CATC_VAL;
                    switch (CATC)
                    {
                        case 1: CATC_VAL = "Active CAS"; break;
                        case 2: CATC_VAL = "Active CAS with OCM"; break;
                        case 3: CATC_VAL = "Active CAS of Junior Status"; break;
                        case 4: CATC_VAL = "Passive CAS with 1030 TCAS"; break;
                        case 5: CATC_VAL = "Passive CAS with only OCM"; break;
                        default: CATC_VAL = "TCAS II or no CAS"; break;
                    }

                    Console.WriteLine($"  SVH: {SVH_EP} ({SVH_VAL}) | {CATC_EP} ({CATC_VAL})");

                    FX = (ext4 & 1) == 1;
                }

                // --- Fifth extension ---
                if (FX)
                {
                    byte ext5 = body[k];
                    k++;

                    string TAO_EP = (ext5 >> 7 & 1) == 1 ? "TAO Element Populated" : "TAO Element Not Populated";
                    int TAO = (ext5 >> 2 & 0b1_1111);
                    string TAO_VAL = "N/A";
                    // FALTA -- PREGUNTAR AL PROFE COMO QUIERE QUE LO HAGAMOS

                    Console.WriteLine($"  TAO: {TAO_EP} (Approx {TAO_VAL} m offset)");
                }
            }
            if ((body[k_old] >> 1 & 1) == 1)
            {
                ushort TNH_raw = (ushort)(body[k + 2] << 8 | body[k + 3]);
                float TNH = TNH_raw * 360f / (float)Math.Pow(2, 16);

                k += 2;
            }
            // ESTE TAMBIEN ES UN PALAZO INCREIBLE (2.10 MES)
            if ((body[k_old] >> 0 & 1) == 1)
            {
                Console.WriteLine("MES: Military Extended Squitter");

                byte mesPrimary = body[k];
                k++;

                // Subfield presence bits
                bool SUM = (mesPrimary >> 7 & 1) == 1; // Mode 5 Summary
                bool PNO = (mesPrimary >> 6 & 1) == 1; // Mode 5 PIN / National Origin
                bool EM1 = (mesPrimary >> 5 & 1) == 1; // Extended Mode 1 Code
                bool XP  = (mesPrimary >> 4 & 1) == 1; // X Pulse Presence
                bool FOM = (mesPrimary >> 3 & 1) == 1; // Figure of Merit
                bool M2  = (mesPrimary >> 2 & 1) == 1; // Mode 2 Code
                bool FX  = (mesPrimary & 1) == 1;      // Extension

                Console.WriteLine($"  Subfields: SUM={SUM}, PNO={PNO}, EM1={EM1}, XP={XP}, FOM={FOM}, M2={M2}");

                // --- Subfield #1: Mode 5 Summary ---
                if (SUM)
                {
                    byte mode5 = body[k];
                    k++;

                    string M5  = (mode5 >> 7 & 1) == 1 ? "Mode 5 interrogation" : "No Mode 5 interrogation";
                    string ID  = (mode5 >> 6 & 1) == 1 ? "Authenticated Mode 5 ID reply" : "No authenticated Mode 5 ID reply";
                    string DA  = (mode5 >> 5 & 1) == 1 ? "Authenticated Mode 5 Data reply" : "No authenticated Mode 5 Data reply";
                    string M1  = (mode5 >> 4 & 1) == 1 ? "Mode 1 code from Mode 5" : "No Mode 1 code from Mode 5";
                    string M2s = (mode5 >> 3 & 1) == 1 ? "Mode 2 code from Mode 5" : "No Mode 2 code from Mode 5";
                    string M3  = (mode5 >> 2 & 1) == 1 ? "Mode 3 code from Mode 5" : "No Mode 3 code from Mode 5";
                    string MC  = (mode5 >> 1 & 1) == 1 ? "Flight Level from Mode 5" : "No Flight Level from Mode 5";
                    string PO  = (mode5 & 1) == 1 ? "Position from Mode 5" : "Position not from Mode 5";

                    Console.WriteLine($"  [SUM] {M5} | {ID} | {DA} | {M1} | {M2s} | {M3} | {MC} | {PO}");
                }

                // --- Subfield #2: Mode 5 PIN / National Origin ---
                if (PNO)
                {
                    // 4 octets total
                    int PIN = ((body[k] & 0b0011_1111) << 8) | body[k + 1];
                    int NO = ((body[k + 3] & 0b0000_0111) << 8) | body[k + 4];

                    Console.WriteLine($"  [PNO] PIN={PIN}, National Origin Code={NO}");
                    k += 4;
                }

                // --- Subfield #3: Extended Mode 1 Code in Octal Representation ---
                if (EM1)
                {
                    byte b1 = body[k];
                    byte b2 = body[k + 1];
                    k += 2;

                    string V = (b1 >> 7 & 1) == 1 ? "Code not validated" : "Code validated";
                    string L = (b1 >> 5 & 1) == 1 ? "Smoothed Mode 1 code" : "Mode 1 code as derived";

                    ushort EM1code = (ushort)(((b1 & 0x0F) << 8) | b2);
                    // FALTA -- AQUI HACER LA LECTURA OCTAL 
                }

                // --- Subfield #4: X Pulse Presence ---
                if (XP)
                {
                    byte xpByte = body[k];
                    k++;

                    string XP_PIN = (xpByte >> 5 & 1) == 1 ? "X-pulse from Mode 5 PIN present" : "No X-pulse from Mode 5 PIN";
                    string X5 = (xpByte >> 4 & 1) == 1 ? "X5 present" : "X5 not present";
                    string XC = (xpByte >> 3 & 1) == 1 ? "X from Mode C present" : "No Mode C X";
                    string X3 = (xpByte >> 2 & 1) == 1 ? "X from Mode 3/A present" : "No Mode 3/A X";
                    string X2 = (xpByte >> 1 & 1) == 1 ? "X from Mode 2 present" : "No Mode 2 X";
                    string X1 = (xpByte & 1) == 1 ? "X from Mode 1 present" : "No Mode 1 X";

                    Console.WriteLine($"  [XP] {XP_PIN} | {X5} | {XC} | {X3} | {X2} | {X1}");
                }

                // --- Subfield #5: Figure of Merit ---
                if (FOM)
                {
                    byte fomByte = body[k];
                    k++;

                    int FOM_VAL = fomByte & 0x1F;
                    Console.WriteLine($"  [FOM] Figure of Merit={FOM_VAL}");
                }

                // --- Subfield #6: Mode 2 Code in Octal Representation ---
                if (M2)
                {
                    byte b1 = body[k];
                    byte b2 = body[k + 1];
                    k += 2;

                    string V = (b1 >> 7 & 1) == 1 ? "Code not validated" : "Code validated";
                    string L = (b1 >> 5 & 1) == 1 ? "Smoothed Mode 2 code" : "Mode 2 code as derived";

                    ushort EM1code = (ushort)(((b1 & 0x0F) << 8) | b2);
                    // FALTA -- AQUI HACER LA LECTURA OCTAL 
                }
            }
        }

        public static void DataItem43(ref int k, byte[] body)
        {
            Console.WriteLine("(43)");
        }

        public static void DataItem44(ref int k, byte[] body)
        {
            Console.WriteLine("(44)");
        }

        public static void DataItem45(ref int k, byte[] body)
        {
            Console.WriteLine("(45)");
        }

        public static void DataItem46(ref int k, byte[] body)
        {
            Console.WriteLine("(46)");
        }

        public static void DataItem47(ref int k, byte[] body)
        {
            Console.WriteLine("(47)");
        }

        public static void DataItem48(ref int k, byte[] body)
        {
            Console.WriteLine("(48)");


        }




        /*Jan decia de hacerlo asi:
        public static Action[] functions = {}
        */
        //Para poner el valor de "k" y poderlo modificar se necesita poner ref y por lo cual crear el delegate 
        public delegate void DelegateFunctions(ref int k, byte[] body);

        public static DelegateFunctions[] functions = {
            DataItem1, DataItem2, DataItem3, DataItem4, DataItem5, DataItem6, DataItem7, DataItem8, DataItem9, DataItem10,
            DataItem11, DataItem12, DataItem13, DataItem14, DataItem15, DataItem16, DataItem17, DataItem18, DataItem19,
            DataItem20, DataItem21, DataItem22, DataItem23, DataItem24, DataItem25, DataItem26, DataItem27, DataItem28, DataItem29,
            DataItem30, DataItem31, DataItem32, DataItem33, DataItem34, DataItem35, DataItem36, DataItem37, DataItem38, DataItem39,
            DataItem40, DataItem41, DataItem42, DataItem43, DataItem44, DataItem45, DataItem46, DataItem47, DataItem48

        };
    }
}
