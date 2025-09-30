var file = File.ReadAllBytes(args[0]);

AsterixParser.Parser.ParseFile(file);