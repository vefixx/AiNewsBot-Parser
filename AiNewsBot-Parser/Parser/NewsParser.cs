using System.Text;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace AiNewsBot_Parser.Parser;

public class NewsParser
{
    private readonly Dictionary<string, Func<string, Task>> _urls;
    private readonly HtmlWeb _web;
    private readonly ILogger<NewsParser> _logger = Log.CreateLogger<NewsParser>();
    
    public NewsParser()
    {
        _web = new HtmlWeb();
        _web.OverrideEncoding = Encoding.UTF8;
        
        _urls = new Dictionary<string, Func<string, Task>>()
        {
            {"https://59.ru/", Parse59RuAsync}
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
                    await urlPair.Value(urlPair.Key);
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
    
    /// <summary>
    /// Парсинг сайта https://59.ru/
    /// </summary>
    private async Task Parse59RuAsync(string url)
    {
        try
        {
            var doc = _web.Load(url);

            // Парсинг li тегов из главных новостей
            var newsList = doc.DocumentNode
                .SelectNodes("//ol[@class='content_D5XNy fullHeight_D5XNy']/li");

            foreach (var liNode in newsList)
            {
                // Получение ссылки на новость
                var aNode = liNode.FirstChild;
                string postUrl = aNode.GetAttributeValue("href", string.Empty);
                if (postUrl != string.Empty)
                {
                    _logger.LogInformation($"Обработка поста url={postUrl}");
                    doc = _web.Load(postUrl);

                    // Конечный контент, который будет скормлен ИИ
                    List<string> resultContentText = new();
                    List<string> resultContentImageUrls = new();

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
                                resultContentText.Add(content);
                            }
                        }
                        else if (attributes.Contains("articleBlockImage_odoam")) // Блок изображения
                        {
                            var picture = block.SelectSingleNode(".//picture");
                            var img = picture.SelectSingleNode(".//img");
                            string src = img.GetAttributeValue("src", "");
                            resultContentImageUrls.Add(src);
                        }
                    }

                    _logger.LogInformation(
                        $"Конец обработки поста {postUrl} | Блоков текста: {resultContentText.Count} | Блоков изображений: {resultContentImageUrls.Count}");
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogInformation(e, $"Ошибка обработки сайта {url}");
        }
    }
}