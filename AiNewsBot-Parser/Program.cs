using AiNewsBot_Parser.Parser;

namespace AiNewsBot_Parser;

class Program
{
    static async Task Main(string[] args)
    {
        ParserClient parserClient = new ParserClient();
        await parserClient.StartParseAsync();
    }
}