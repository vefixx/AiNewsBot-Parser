using AiNewsBot_Parser.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace AiNewsBot_Parser.Parser;

public class Parser59Ru : IParser
{
    private readonly ILogger<Parser59Ru> _logger = Log.CreateLogger<Parser59Ru>();
    private readonly HtmlWeb _web;

    public Parser59Ru(HtmlWeb web)
    {
        _web = web;
    }
    
    public async Task<List<ParseResult>> ParseAsync(string url)
    {
        try
        {
            var doc = _web.Load(url);

            // Парсинг li тегов из главных новостей
            var newsList = doc.DocumentNode
                .SelectNodes("//ol[@class='content_D5XNy fullHeight_D5XNy']/li");
            
            List<ParseResult> parseResults = new List<ParseResult>();

            foreach (var liNode in newsList)
            {
                ParseResult parseResult = ProcessLiNode(liNode);
                parseResults.Add(parseResult);
            }

            return parseResults;
        }
        catch (Exception e)
        {
            _logger.LogInformation(e, $"Ошибка обработки сайта {url}");
            throw;
        }
    }

    private ParseResult ProcessLiNode(HtmlNode liNode)
    {
        // Получение ссылки на пост
        var aNode = liNode.FirstChild;
        string postUrl = aNode.GetAttributeValue("href", string.Empty);
        
        // Конечный контент, который будет скормлен ИИ
        ParseResult parseResult = new ParseResult();
        
        if (postUrl != string.Empty)
        {
            _logger.LogInformation($"Обработка поста url={postUrl}");
            
            FillResultsForPost(postUrl, parseResult);

            _logger.LogInformation(
                $"Конец обработки поста {postUrl} | Блоков текста: {parseResult.ResultTexts.Count} | Блоков изображений: {parseResult.ResultImages.Count}");
        }

        return parseResult;
    }

    private void FillResultsForPost(string postUrl, ParseResult parseResult)
    {
        var doc = _web.Load(postUrl);
        
        // Главный контент поста
        var contentBody = doc.GetElementbyId("articleBody");

        // Блоки контента (текста + изображения)
        var contentBlocks = contentBody.SelectNodes("./div");

        foreach (var block in contentBlocks)
        {
            string[] attributes = block.GetAttributeValue("class", string.Empty).Split(" ");
            if (attributes.Contains("uiArticleBlockText_NCfYZ")) // Если это блок текста
            {
                var firstParagraph = block.SelectSingleNode(".//p");
                if (firstParagraph != null)
                {
                    string content = firstParagraph.InnerText.Trim();
                    content = content.Replace("&nbsp;", " ");
                    parseResult.ResultTexts.Add(content);
                }
            }
            else if (attributes.Contains("articleBlockImage_odoam")) // Блок изображения
            {
                var picture = block.SelectSingleNode(".//picture");
                var img = picture.SelectSingleNode(".//img");
                string src = img.GetAttributeValue("src", "");
                parseResult.ResultImages.Add(src);
            }
        }
    }
}