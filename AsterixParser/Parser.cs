namespace AsterixParser
{
    public class Parser
    {

        public static void ParseFile(byte[] file)
        {
            int i = 0;

            while (i < file.Length)
            {
                byte cat = file[i];
                ushort length =  (ushort) (file[i+2] | (file[i+1] << 8));
                Console.WriteLine(length);

                i += length;
            }
        }
    }
}
