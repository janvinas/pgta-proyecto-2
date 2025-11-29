using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsterixParser
{
    internal class CAT21(byte[] body, AsterixMessage message)
    {
        // The list is defined statically because it's much faster to clear it than to generate a new one.
        // This has the downside that the reader cannot run concurrently.
        private static List<int> nFSPEC = new List<int>(40);

        public int CAT21Reader(int i, ushort length)
        {
            // because the same list is reused
            nFSPEC.Clear();
            FSPECReader();
            return DataItem();
        }

        int k = 0; //Byte en el que estas del body (como un punto de libro)

        public void FSPECReader()
        {
            int error = 0;

            bool fx = true;
            //nFSPEC es la vector de numeros de FSPEC aparecen de FSPEC(seguro que hay mejor manera)

            int FRN = 1;

            while (fx)
            {
                byte b = body[k];
                for (int i = 7; i > 0; i--)
                {
                    int v = (b >> i) & 1;
                    if (v == 1) nFSPEC.Add(FRN);
                    FRN++;
                }

                int bit = b & 1;
                if (bit == 0) fx = false;
                k++;
            }
        }

        private int DataItem()
        {
            int error = 0;

            foreach (int n in nFSPEC)
            {
                //En un array de funciones le pasa el numero del DataField que quiere decodificar
                DataItemParser21.functions[n - 1](ref message, ref k, body);
            }

            return error;
        }



    }
}
