using AiNewsBot_Parser.Models;

namespace AiNewsBot_Parser.Parser;

public interface IParser
{
    Task<List<PostParseResult>> ParseAsync(string url);
}