var file = File.ReadAllBytes(args[0]);

var messages = AsterixParser.Parser.ParseFile(file);
Console.WriteLine(messages);