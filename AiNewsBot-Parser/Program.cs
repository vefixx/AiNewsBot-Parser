using AiNewsBot_Parser.Parser;

namespace AiNewsBot_Parser;

class Program
{
    static async Task Main(string[] args)
    {
        NewsParser parser = new NewsParser();
        await parser.StartParseAsync();
    }
}