using AiNewsBot_APILib;
using AiNewsBot_Parser.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace AiNewsBot_Parser.Parser;

public class Parser59Ru : IParser
{
    private readonly ILogger<Parser59Ru> _logger = Log.CreateLogger<Parser59Ru>();
    private readonly HtmlWeb _web;
    private readonly AiNewsApiClient _apiClient;

    public Parser59Ru(HtmlWeb web)
    {
        _web = web;
        _apiClient = new AiNewsApiClient();
    }
    
    public async Task<List<PostParseResult>> ParseAsync(string url)
    {
        try
        {
            var doc = _web.Load(url);

            // Парсинг li тегов из главных новостей
            var newsList = doc.DocumentNode
                .SelectNodes("//ol[@class='content_D5XNy fullHeight_D5XNy']/li");
            
            List<PostParseResult> parseResults = new List<PostParseResult>();
            List<string> alreadyProcessedIds = await _apiClient.AiGatewayEndpoint.GetAllPostIdsAsync();

            foreach (var liNode in newsList)
            {
                PostParseResult postParseResult = ProcessLiNode(liNode, alreadyProcessedIds);
                
                if (!string.IsNullOrEmpty(postParseResult.PostId))
                    parseResults.Add(postParseResult);
            }

            return parseResults;
        }
        catch (Exception e)
        {
            _logger.LogInformation(e, $"Ошибка обработки сайта {url}");
            throw;
        }
    }

    private PostParseResult ProcessLiNode(HtmlNode liNode, List<string> alreadyProcessedIds)
    {
        // Получение ссылки на пост
        var aNode = liNode.FirstChild;
        string postUrl = aNode.GetAttributeValue("href", string.Empty);
        
        // Конечный контент, который будет скормлен ИИ
        PostParseResult postParseResult = new PostParseResult();
        
        if (postUrl != string.Empty)
        {
            // Проверка, что пост уже был обработан
            string[] urlParts = postUrl.Split("/");
            string postId = urlParts[^2];
            if (alreadyProcessedIds.Contains(postId))
            {
                _logger.LogInformation($"Пост url={postUrl} уже был обработан");
                return postParseResult;
            }

            postParseResult.PostId = postId;
            
            _logger.LogInformation($"Обработка поста url={postUrl} postId={postId}");
            
            FillResultsForPost(postUrl, postParseResult);

            _logger.LogInformation(
                $"Конец обработки поста {postUrl} | Блоков текста: {postParseResult.ResultTexts.Count} | Блоков изображений: {postParseResult.ResultImages.Count}");
        }

        return postParseResult;
    }

    private void FillResultsForPost(string postUrl, PostParseResult postParseResult)
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
                    postParseResult.ResultTexts.Add(content);
                }
            }
            else if (attributes.Contains("articleBlockImage_odoam")) // Блок изображения
            {
                var picture = block.SelectSingleNode(".//picture");
                var img = picture.SelectSingleNode(".//img");
                string src = img.GetAttributeValue("src", "");
                postParseResult.ResultImages.Add(src);
            }
        }
    }
}