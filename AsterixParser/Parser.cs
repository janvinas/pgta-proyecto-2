using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AsterixParser
{
    public class Parser
    {

        public static List<AsterixMessage> ParseFile(byte[] file)
        {
            List<AsterixMessage> messages = [];

            CAT21 CAT021;
            CAT48 CAT048;

            int error = 0;

            int i = 0;
            int k = 0;

            while (i < file.Length && k<10)
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

                        var message = new AsterixMessage
                        {
                            Cat = CAT.CAT048
                        };
                        CAT048 = new CAT48(body, message);
                        error = CAT048.CAT48Reader(i, length);

                        if (error == 0) messages.Add(message); 

                        Console.WriteLine(message);
                        
                        break;
                }
                k ++;
                i += length;
            }

            return messages;
        }

        public static async Task<List<AsterixMessage>> ParseFileAsync(byte[] file, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() =>
            {
                List<AsterixMessage> messages = [];

                int i = 0;
                int k = 0;

                while (i < file.Length && k < 10)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    byte cat = file[i];
                    ushort length = (ushort)(file[i + 2] | (file[i + 1] << 8));

                    //Body es el cuerpo sin CAT ni LEN
                    byte[] body = new byte[length - 3];
                    Array.Copy(file, i + 3, body, 0, length - 3);

                    switch (cat)
                    {
                        case 21:
                            {
                                var CAT021 = new CAT21(body);
                                var error = CAT021.CAT21Reader(i, length);
                                break;
                            }
                        case 48:
                            {
                                var message = new AsterixMessage
                                {
                                    Cat = CAT.CAT048
                                };
                                var CAT048 = new CAT48(body, message);
                                var error = CAT048.CAT48Reader(i, length);

                                if (error == 0) messages.Add(message);
                                break;
                            }
                    }

                    // report progress:
                    i += length;
                    k++;

                    progress?.Report(i / file.Length);
                }

                return messages;
            });

        }
    }
}
