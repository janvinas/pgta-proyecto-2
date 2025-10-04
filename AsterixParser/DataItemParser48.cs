using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AsterixParser
{
    internal class DataItemParser48
    {
        public static void DataItem1(ref int k, byte[] body) // 010
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

        public static void DataItem7(ref int k, byte[] body)
        {
            Console.WriteLine("(DF-7)");
        }

        public static void DataItem8(ref int k, byte[] body)
        {
            Console.WriteLine("(DF-8)");
        }

        public static void DataItem9(ref int k, byte[] body)
        {
            Console.WriteLine("(DF-9)");
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
