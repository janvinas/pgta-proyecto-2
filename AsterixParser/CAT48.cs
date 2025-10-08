using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace AsterixParser
{
    internal class CAT48(byte[] body, AsterixMessage message)
    {
        public int CAT48Reader(int i, ushort length) 
        {
            int error = 0;

            Queue<int> vFSPEC = FSPECReader();

            int errorDI = DataItem(vFSPEC);


            return error;
        }

        int k = 0; //Byte en el que estas del body (como un punto de libro)

        public Queue<int> FSPECReader()
        {
            int error = 0;

            bool fx = true;

            //vFSPEC es la vector de bits de FSPEC de mida variable (seguro que hay mejor manera)
            Queue<int> vFSPEC = new Queue<int>();
            //nFSPEC es la vector de numeros de FSPEC aparecen de FSPEC(seguro que hay mejor manera)
            Queue<int> nFSPEC = new Queue<int>();

            int FRN = 1;

            Console.WriteLine("Lectura de FSPEC:");
            while (fx || FRN > 22) {
                byte b = body[k];
                for (int i = 7; i > 0; i--)
                {
                    int v = (b >> i) & 1;
                    if(v == 1) nFSPEC.Enqueue(FRN);
                    vFSPEC.Enqueue(v);
                    Console.Write(v); //Recorre byte y lo imprime
                    FRN++;
                }

                int bit = b & 1;
                if (bit == 0) fx = false;
                k++;
            }
            
            Console.WriteLine();
            Console.WriteLine("Posición byte lectura: " + k);
            return nFSPEC;
        }

        public int DataItem(Queue<int> nFSPEC)
        {
            int error = 0;
            var parser = new DataItemParser48(message);
            
            while (nFSPEC.Count > 0)
            { 
                int n = nFSPEC.Dequeue();
                //Console.WriteLine(n);
                //En un array de funciones le pasa el numero del DataField que quiere decodificar
                parser.functions[n-1](ref k, body); 
                Console.WriteLine($"Siguiente Byte: {k}");
            }

            Console.WriteLine("Last byte: " + k);

            return error;
        }

        

    }
}
