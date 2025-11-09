using System.Text;
using AiNewsBot_Parser.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace AiNewsBot_Parser.Parser;

public class ParserClient
{
    private readonly Dictionary<string, IParser> _urls;
    private readonly HtmlWeb _web;
    private readonly ILogger<ParserClient> _logger = Log.CreateLogger<ParserClient>();
    
    public ParserClient()
    {
        _web = new HtmlWeb();
        _web.OverrideEncoding = Encoding.UTF8;
        
        _urls = new Dictionary<string, IParser>()
        {
            {"https://59.ru/", new Parser59Ru(_web)}
        };
    }
    
    public async Task StartParseAsync()
    {
        while (true)
        {
            try
            {
                foreach (var urlPair in _urls)
                {
                    _logger.LogInformation($"Обработка сайта {urlPair.Key}");
                    List<ParseResult> parseResults = await urlPair.Value.ParseAsync(urlPair.Key);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Ошибка обработки");
            }
            finally
            {
                await Task.Delay(TimeSpan.FromMinutes(1));
            }
        }
    }
}