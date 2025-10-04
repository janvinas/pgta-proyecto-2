namespace AsterixParser
{
    public class Parser
    {

        public static void ParseFile(byte[] file)
        {
            CAT21 CAT021;
            CAT48 CAT048;

            int error = 0;

            int i = 0;
            int k = 0;

            while (i < file.Length && k<5)
            {
                Console.WriteLine("------------------------------------------------");
                Console.WriteLine("------------------------------------------------");

                byte cat = file[i];
                ushort length =  (ushort) (file[i+2] | (file[i+1] << 8));

                //Body es el cuerpo sin CAT ni LEN
                byte[] body = new byte[length - 3];
                Array.Copy(file,i+3,body,0,length-3);
                
                Console.Write("Longitud del mensaje: " + length + " ");

                switch (cat)
                {
                    case 21:
                        Console.Write("CAT-" + cat + "\n");
                        
                        CAT021 = new CAT21(body);
                        error = CAT021.CAT21Reader(i, length);
                        
                        break;
                    case 48:
                        Console.Write("CAT-" + cat + "\n");

                        var message = new AsterixMessage();
                        CAT048 = new CAT48(body, message);
                        error = CAT048.CAT48Reader(i, length);

                        Console.WriteLine(message);
                        
                        break;
                }
                k ++;
                i += length;
            }
        }
    }
}
