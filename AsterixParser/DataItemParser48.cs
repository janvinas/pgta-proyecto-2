using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AsterixParser
{
    internal class DataItemParser48
    {
        public static void DataItem1(ref int k, byte[] body) // I048/010
        {
            Console.WriteLine("(DF-1)");
            byte SAC = body[k];
            if (SAC == 20) Console.WriteLine("ESP");
            else Console.WriteLine("Otro pais");
            byte SIC = body[k + 1];
            Console.WriteLine($"Identification Code {SIC}");
            k += 2;
        }

        public static void DataItem2(ref int k, byte[] body) //Comprobar si esta bien, ya que el primero que sale realmente en el de prueba no existe
        {
            Console.WriteLine("(DF-2)");
            int date = (body[k + 2] | body[k + 1] << 8 | (body[k] << 16));
            float hour = date/128f;
            Console.WriteLine("Time: " + TimeSpan.FromSeconds(hour));
            k += 3;
        }

        public static void DataItem3(ref int k, byte[] body) //No esta hecho, solo lo salto, no lo entiendo jeje (Pau)
        {
            Console.WriteLine("(DF-3)");
            bool fx = true;

            while (fx)
            {
                byte b = body[k];
                for (int i = 7; i > 0; i--)
                {
                    int v = (b >> i) & 1;
                    Console.Write(v); //Recorre byte y lo imprime
                }
                Console.WriteLine();
                int bit = b & 1;
                if (bit == 0) fx = false;
                k++;
            }

        }

        public static void DataItem4(ref int k, byte[] body) //Creo que no esta acabado
        {
            Console.WriteLine("(DF-4)");
            ushort rho_raw = (ushort)(body[k + 1] | (body[k] << 8));
            ushort theta_raw = (ushort)(body[k + 2] << 8| body[k + 3]);
            float rho = rho_raw / 256f;
            float theta = theta_raw * 360f / (float)Math.Pow(2,16);

            Console.WriteLine("Rho: " + rho);
            Console.WriteLine("Theta: " + theta);

            k += 4;
        }

        public static void DataItem5(ref int k, byte[] body) //No esta hecho, solo lo salto
        {
            Console.WriteLine("(DF-5)");
            k += 2;
        }

        public static void DataItem6(ref int k, byte[] body)
        {
            Console.WriteLine("(DF-6)");
            if (body[k] >> 7 == 1) Console.WriteLine("Code not validated");
            if (body[k] >> 6 == 1) Console.WriteLine("Garbled code");
            ushort FL_raw = (ushort)(body[k + 1] | (body[k] << 8));

            FL_raw &= 0x3FFF;

            float FL = FL_raw / 4f;

            Console.WriteLine(FL);

            k += 2;
        }

        public static Queue<int> PrimarySubfieldDecoder(ref int k, byte[] body) // Para leer subfield primario del I048/130 (por ahora)
        {
            bool fx = true;
            Queue<int> nSubfield = new Queue<int>();
            int subfieldindex = 1;
            Console.WriteLine("Lectura del subfield primario: ");
            while (fx)
            {
                byte b = body[k];
                for (int i = 7; i > 0; i--)
                {
                    int v = (b >> i) & 1;
                    if (v == 1) nSubfield.Enqueue(subfieldindex);
                    Console.Write(v); //Recorre byte y lo imprime
                    subfieldindex++;
                }

                int bit = b & 1;
                if (bit == 0) fx = false;
                k++;
            }

            Console.WriteLine();
            Console.WriteLine("Posición byte lectura: " + k);
            return nSubfield;
        }

        public static void DataItem7(ref int k, byte[] body) // I048/130 Radar Plot Characteristics
        {
            Console.WriteLine("(DF-7)");
            int n;
            Queue<int> nSubfield = PrimarySubfieldDecoder(ref k, body);
            int i = 0;
            while (i < nSubfield.Count)
            {

                switch (i)
                {
                    case 0: // Subfield 1 SRL
                        n = nSubfield.Dequeue();
                        if (n == 1)
                        {
                            double srl = body[k];
                            srl = srl * 360 / (2 ^ 13); // SRL in degrees
                            Console.WriteLine($"Range Covered (SRL): {srl} degrees.");
                            k++;
                        }
                        break;
                    case 1: // Subfield 2 SRR
                        n = nSubfield.Dequeue();
                        if (n == 1)
                        {
                            int srr = body[k]; // Number of Received Replies for (M)SSR
                            Console.WriteLine($"Number of Received Replies for (M)SSR: {srr}.");
                            k++;
                        }
                        break;
                    case 2: // Subfield 3 SAM
                        n = nSubfield.Dequeue();
                        if (n == 1)
                        {
                            sbyte sam = (sbyte)body[k]; // Amplitude of M(SSR) Reply in dBm
                            Console.WriteLine($"Amplitude of M(SSR) Reply in dBm: {sam} dBm.");
                            k++;
                        }
                        break;
                    case 3: // Subfield 4 PRL
                        n = nSubfield.Dequeue();
                        if (n == 1)
                        {
                            double prl = body[k];
                            prl = prl * 360 / (2 ^ 13); // PRL in degrees
                            Console.WriteLine($"Range Covered (PRL): {prl} degrees.");
                            k++;
                        }
                        break;
                    case 4: // Subfield 5 PAM
                        n = nSubfield.Dequeue();
                        if (n == 1)
                        {
                            sbyte pam = (sbyte)body[k]; // Amplitude of PSR Reply in dBm
                            Console.WriteLine($"Amplitude of PSR Reply in dBm: {pam} dBm.");
                            k++;
                        }
                        break;
                    case 5: // Subfield 6 RPD
                        n = nSubfield.Dequeue();
                        if (n == 1)
                        {
                            sbyte rpd_byte = (sbyte)body[k];
                            double rpd = rpd_byte / 256; // Difference in Range Betweeen PSR and SSR (PSR-SSR) in NM
                            Console.WriteLine($"Difference in Range Betweeen PSR and SSR (PSR-SSR): {rpd} NM.");
                            k++;
                        }
                        break;
                    case 6: // Subfield 7 APD
                        n = nSubfield.Dequeue();
                        if (n == 1)
                        {
                            double apd = body[k];
                            apd = apd * 360 / (2 ^ 14); // Difference in azimuth between PSR and SSR plot in degrees
                            Console.WriteLine($"Difference in azimuth between PSR and SSR plot: {apd} degrees.");
                            k++;
                        }
                        break;
                    case 7: // Subfield 8 SCO
                        n = nSubfield.Dequeue();
                        if (n == 1)
                        {
                            Console.WriteLine($"Score: {body[k]}");
                            k++;
                        }
                        break;
                    case 8: // Subfield 9 SCR
                        n = nSubfield.Dequeue();
                        if (n == 1)
                        {
                            ushort scr_combined = (ushort)((body[k] << 8) | body[k+1]);
                            double scr = scr_combined * 0.1; // Signal / Clutter Radio
                            Console.WriteLine($"Signal / Clutter Radio: {scr} dB.");
                            k += 2;
                        }
                        break;
                    case 9: // Subfield 10 RW
                        n = nSubfield.Dequeue();
                        if (n == 1)
                        {
                            ushort rw_combined = (ushort)((body[k] << 8) | body[k + 1]);
                            double rw = rw_combined / 256; // Range Width in NM
                            Console.WriteLine($"Range Width: {rw} NM.");
                            k += 2;
                        }
                        break;
                    case 10: // Subfield 11 AR
                        n = nSubfield.Dequeue();
                        if (n == 1)
                        {
                            ushort ar_combined = (ushort)((body[k] << 8) | body[k + 1]);
                            double ar = ar_combined / 256; // Ambiguous Range in NM
                            Console.WriteLine($"Ambiguous Range: {ar} NM.");
                            k += 2;
                        }
                        break;
                }
                i++;
            }
            
        }

        public static void DataItem8(ref int k, byte[] body) // I048/220 Aircraft Address
        {
            Console.WriteLine("(DF-8)");
            byte[] address = { body[k], body[k + 1], body[k + 2] };
            string hexAddress = BitConverter.ToString(address).Replace("-","");
            Console.WriteLine(hexAddress);
            k += 3; // 3 octets
        }

        public static char DecodificarChar6bit(int val) // Mapejar IA-5 (6 bits) a caràcters
        {
            if (val >= 0 && val <= 25)
                return (char)('A' + val);
            else if (val >= 26 && val <= 35)
                return (char)('0' + (val - 26));
            else if (val == 36)
                return ' ';
            else if (val == 37)
                return '-';
            else
                return ' '; // valor no definit
        }
        public static void DataItem9(ref int k, byte[] body) // I048/240 Aircraft Identification
        {
            Console.WriteLine("(DF-9)");
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
            k += 6; // 6 octets
        }

        public static void DataItem10(ref int k, byte[] body)
        {
            Console.WriteLine("(DF-10)");
        }

        public static void DataItem11(ref int k, byte[] body)
        {
            Console.WriteLine("(DF-11)");

            ushort TrackNum = (ushort)(body[k + 1] | (body[k] << 8));
            TrackNum &= 0x0FFF;

            Console.WriteLine(TrackNum);

            k += 2;
        }

        public static void DataItem12(ref int k, byte[] body)
        {
            Console.WriteLine("(DF-12)");

            k += 4;
        }

        public static void DataItem13(ref int k, byte[] body)
        {
            Console.WriteLine("(DF-13)");

            ushort GS_raw = (ushort)(body[k + 1] | (body[k] << 8));
            ushort Head_raw = (ushort)(body[k + 3] | (body[k + 2] << 8));

            float GS = GS_raw / (float)Math.Pow(2, 14);
            float Head = Head_raw * 360f / (float)Math.Pow(2, 16);

            Console.WriteLine("GroundSpeed: " + GS);
            Console.WriteLine("Heading: " +  Head);
            
            k += 4;
        }

        public static void DataItem14(ref int k, byte[] body)
        {
            Console.WriteLine("(DF-14)");
            int dk = 1; // Bytes de salto del datafield (dependiendo de FX)

            Queue<string> TrackStatus = new Queue<string>();

            string CNF;
            string RAD;
            string DOU;
            string MAH;
            string CDM;
            string TRE;
            string GHO;
            string SUP;
            string TCC;

            if (body[k] >> 7 == 1)
            {
                CNF = "Confirmed Track";
                Console.WriteLine("Code not validated");
            }
            else
            {
                CNF = "Tentative Track";
                Console.WriteLine("Code not validated");
            }
            if (body[k] >> 6 == 1) Console.WriteLine("Garbled code");
            else Console.WriteLine("Code not validated");

            k += dk;
        }

        public static void DataItem15(ref int k, byte[] body)
        {
            Console.WriteLine("(DF-15)");

            k += 4;
        }

        public static void DataItem16(ref int k, byte[] body)
        {
            Console.WriteLine("(DF-16)");
        }

        public static void DataItem17(ref int k, byte[] body)
        {
            Console.WriteLine("(DF-17)");

            k += 2;
        }

        public static void DataItem18(ref int k, byte[] body)
        {
            Console.WriteLine("(DF-18)");

            k += 4;
        }

        public static void DataItem19(ref int k, byte[] body)
        {
            Console.WriteLine("(DF-19)");

            k += 2;
        }

        public static void DataItem20(ref int k, byte[] body)
        {
            Console.WriteLine("(DF-20)");
        }

        public static void DataItem21(ref int k, byte[] body)
        {
            Console.WriteLine("(DF-21)");
        }


        /*Jan decia de hacerlo asi:
        public static Action[] functions = {}
        */
        //Para poner el valor de "k" y poderlo modificar se necesita poner ref y por lo cual crear el delegate 
        public delegate void DelegateFunctions(ref int k, byte[] body);

        public static DelegateFunctions[] functions = {
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

        };
    }
}
