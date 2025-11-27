using AsterixParser.Utils;
using System.Threading.Tasks.Dataflow;
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
            int k = 1;



            while (i < file.Length)
            {
                byte cat = file[i];
                ushort length =  (ushort) (file[i+2] | (file[i+1] << 8));

                //Body es el cuerpo sin CAT ni LEN
                byte[] body = new byte[length - 3];
                Array.Copy(file,i+3,body,0,length-3);
                
                switch (cat)
                {
                    case 21:
                        {
                            var message = new AsterixMessage
                            {
                                Cat = CAT.CAT021
                            };
                            CAT021 = new CAT21(body, message);
                            error = CAT021.CAT21Reader(i, length);

                            break;
                        }
                    case 48:
                        {
                            var message = new AsterixMessage
                            {
                                Cat = CAT.CAT048
                            };
                            CAT048 = new CAT48(body, message);
                            error = CAT048.CAT48Reader(i, length);

                            if (error == 0) messages.Add(message);

                            break;
                        }
                }
                k ++;
                i += length;
            }

            return messages;
        }

        public class ParsingResult(List<AsterixMessage> messages, Dictionary<uint, Flight> flights)
        {
            public List<AsterixMessage> messages = messages;
            public Dictionary<uint, Flight> flights = flights;
        }

        public static async Task<ParsingResult> ParseFileAsync(byte[] file, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() =>
            {
                List<AsterixMessage> messages = [];
                Dictionary<uint, Flight> flights = [];

                int i = 0;
                double prog = 0;

                while (i < file.Length)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    byte cat = file[i];
                    ushort length = (ushort)(file[i + 2] | (file[i + 1] << 8));

                    //Body es el cuerpo sin CAT ni LEN
                    byte[] body = new byte[length - 3];
                    Array.Copy(file, i + 3, body, 0, length - 3);

                    AsterixMessage message = null;
                    int error = 0;
                    switch (cat)
                    {
                        case 21:
                            {
                                message = new AsterixMessage
                                {
                                    Cat = CAT.CAT021
                                };
                                var CAT021 = new CAT21(body,message);
                                error = CAT021.CAT21Reader(i, length);
                                break;
                            }
                        case 48:
                            {
                                message = new AsterixMessage
                                {
                                    Cat = CAT.CAT048
                                };
                                var CAT048 = new CAT48(body, message);
                                error = CAT048.CAT48Reader(i, length);
                                break;
                            }
                    }
                    if (error == 0 && message != null) messages.Add(message);
                    if (message?.Address is uint address && message?.Latitude != null && message?.Longitude != null)
                    {
                        if(message.BDS != null && message.BDS.BARO != null && message.FlightLevel?.flightLevel * 100 <= 6000)
                            {
                            double FLcorrected = (double)(message.FlightLevel.flightLevel * 100 + (message.BDS.BARO - 1013.2) * 30);
                            message.QNHcorrection = FLcorrected;
                        }

                        var found = flights.TryGetValue(address, out var flight);
                        if (!found) {
                            flight = new Flight();
                            flights.Add(address, flight);
                        }

                        if (cat == 21)
                        {
                            flight!.cat21Messages.Add(message);
                        } else if (cat == 48)
                        {
                            flight!.cat48Messages.Add(message);
                        }
                    }



                    // report progress:
                    i += length;
                    var newProg = (double)i / (double)file.Length;
                    if (newProg - prog > 0.01)
                    {
                        progress?.Report(newProg);
                        prog = newProg;
                    }
                }
                progress?.Report(0); // finished
                return new ParsingResult(messages, flights);
            });

        }
    }
}
