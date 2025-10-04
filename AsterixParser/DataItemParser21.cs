using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AsterixParser
{
    internal class DataItemParser21
    {
        public static void DataItem1(ref int k, byte[] body)
        {
            Console.WriteLine("(1)");
        }

        public static void DataItem2(ref int k, byte[] body)
        {
            Console.WriteLine("(2)");
        }

        public static void DataItem3(ref int k, byte[] body)
        {
            Console.WriteLine("(3)");
        }

        public static void DataItem4(ref int k, byte[] body)
        {
            Console.WriteLine("(4)");
        }

        public static void DataItem5(ref int k, byte[] body)
        {
            Console.WriteLine("(5)");
        }

        public static void DataItem6(ref int k, byte[] body)
        {
            Console.WriteLine("(6)");
        }

        public static void DataItem7(ref int k, byte[] body)
        {
            Console.WriteLine("(7)");
        }

        public static void DataItem8(ref int k, byte[] body)
        {
            Console.WriteLine("(8)");
        }

        public static void DataItem9(ref int k, byte[] body)
        {
            Console.WriteLine("(9)");
        }

        public static void DataItem10(ref int k, byte[] body)
        {
            Console.WriteLine("(10)");
        }

        public static void DataItem11(ref int k, byte[] body)
        {
            Console.WriteLine("(11)");
        }

        public static void DataItem12(ref int k, byte[] body)
        {
            Console.WriteLine("(12)");
        }

        public static void DataItem13(ref int k, byte[] body)
        {
            Console.WriteLine("(13)");
        }

        public static void DataItem14(ref int k, byte[] body)
        {
            Console.WriteLine("(14)");
        }

        public static void DataItem15(ref int k, byte[] body)
        {
            Console.WriteLine("(15)");
        }

        public static void DataItem16(ref int k, byte[] body)
        {
            Console.WriteLine("(16)");
        }

        public static void DataItem17(ref int k, byte[] body)
        {
            Console.WriteLine("(17)");
        }

        public static void DataItem18(ref int k, byte[] body)
        {
            Console.WriteLine("(18)");
        }

        public static void DataItem19(ref int k, byte[] body)
        {
            Console.WriteLine("(19)");
        }

        public static void DataItem20(ref int k, byte[] body)
        {
            Console.WriteLine("(20)");
        }

        public static void DataItem21(ref int k, byte[] body)
        {
            Console.WriteLine("(21)");
        }

        public static void DataItem22(ref int k, byte[] body)
        {
            Console.WriteLine("(22)");
        }

        public static void DataItem23(ref int k, byte[] body)
        {
            Console.WriteLine("(23)");
        }

        public static void DataItem24(ref int k, byte[] body)
        {
            Console.WriteLine("(24)");
        }

        public static void DataItem25(ref int k, byte[] body)
        {
            Console.WriteLine("(25)");
        }

        public static void DataItem26(ref int k, byte[] body)
        {
            Console.WriteLine("(26)");
        }

        public static void DataItem27(ref int k, byte[] body)
        {
            Console.WriteLine("(27)");
        }

        public static void DataItem28(ref int k, byte[] body)
        {
            Console.WriteLine("(28)");
        }

        public static void DataItem29(ref int k, byte[] body)
        {
            Console.WriteLine("(29)");
        }

        public static void DataItem30(ref int k, byte[] body)
        {
            Console.WriteLine("(30)");
        }

        public static void DataItem31(ref int k, byte[] body)
        {
            Console.WriteLine("(31)");
        }

        public static void DataItem32(ref int k, byte[] body)
        {
            Console.WriteLine("(32)");
        }

        public static void DataItem33(ref int k, byte[] body)
        {
            Console.WriteLine("(33)");
        }

        public static void DataItem34(ref int k, byte[] body)
        {
            Console.WriteLine("(34)");
        }

        public static void DataItem35(ref int k, byte[] body)
        {
            Console.WriteLine("(35)");
        }

        public static void DataItem36(ref int k, byte[] body)
        {
            Console.WriteLine("(36)");
        }

        public static void DataItem37(ref int k, byte[] body)
        {
            Console.WriteLine("(37)");
        }

        public static void DataItem38(ref int k, byte[] body)
        {
            Console.WriteLine("(38)");
        }

        public static void DataItem39(ref int k, byte[] body)
        {
            Console.WriteLine("(39)");
        }

        public static void DataItem40(ref int k, byte[] body)
        {
            Console.WriteLine("(40)");
        }

        public static void DataItem41(ref int k, byte[] body)
        {
            Console.WriteLine("(41)");
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
