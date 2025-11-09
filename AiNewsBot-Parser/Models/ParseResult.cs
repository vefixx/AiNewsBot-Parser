namespace AiNewsBot_Parser.Models;

public class ParseResult
{
    public List<string> ResultTexts { get; set; } = new List<string>();
    public List<string> ResultImages { get; set; } = new List<string>();
}