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

            if (body[k] >> 0 == 1)
            {
                k += 1;
                if (body[k] >> 0 == 1)
                {
                    k += 1;
                    if (body[k] >> 0 == 1) k += 1;
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

            string V = null;
            string G = null;
            string L = null;

            ushort raw = (ushort)(body[k + 1] | (body[k] << 8));

            if (raw >> 15 == 1) V = "Not Validated";
            else V = "Validated";

            if (raw >> 14 == 1) G = "Garbled code";
            else G = "Default";

            if (raw >> 13 == 1) L = "Not Last Scan";
            else L = "Replay";

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
            if (body[k] >> 7 == 1) dk += 2;
            if (body[k] >> 6 == 1) dk += 2;
            if (body[k] >> 5 == 1) dk += 2;
            if (body[k] >> 4 == 1) dk += 1;

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

            int dk = 0;
            if (body[k] >> 7 == 1) dk += 1;
            if (body[k] >> 6 == 1)
            {
                int REP = body[k + 1 + dk];
                dk += 1 + 15 * k;
            }
            dk += 1;

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

        }

        public static void DataItem38(ref int k, byte[] body)
        {
            Console.WriteLine("(38)");
            k += 1;
        }

        public static void DataItem39(ref int k, byte[] body)
        {
            Console.WriteLine("(39)");
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
