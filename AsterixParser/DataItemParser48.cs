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
