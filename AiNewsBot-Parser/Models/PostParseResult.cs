namespace AiNewsBot_Parser.Models;

public class PostParseResult
{
    public string PostId { get; set; } = string.Empty;
    public List<string> ResultTexts { get; set; } = new List<string>();
    public List<string> ResultImages { get; set; } = new List<string>();
}