using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsterixParser
{
    internal class CAT48(byte[] body)
    {
        public int CAT48Reader(int i, ushort length) 
        {
            int error = 0;
            
            int errorFSPEC = FSPECReader();
            
            return error;
        }


        public int FSPECReader()
        {
            int error = 0;

            int k = 0;
            bool fx = true;
            while (fx) {
                byte b = body[k];
                for (int i = 7; i >= 0; i--) Console.Write((b >> i) & 1);

                int bit = b & 1;
                if (bit == 0) fx = false;

                k++;
            }
            Console.WriteLine();
            Console.WriteLine(k);
            return error;
        }

    }
}
