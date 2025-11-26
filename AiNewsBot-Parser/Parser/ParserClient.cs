using System.Text;
using AiNewsBot_APILib;
using AiNewsBot_APILib.Models;
using AiNewsBot_Backend.API.Models;
using AiNewsBot_Parser.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace AiNewsBot_Parser.Parser;

public class ParserClient
{
    private readonly Dictionary<string, IParser> _urls;
    private readonly HtmlWeb _web;
    private readonly AiNewsApiClient _apiClient;
    private readonly ILogger<ParserClient> _logger = Log.CreateLogger<ParserClient>();

    public ParserClient()
    {
        _apiClient = new AiNewsApiClient();

        _web = new HtmlWeb();
        _web.OverrideEncoding = Encoding.UTF8;

        _urls = new Dictionary<string, IParser>()
        {
            { "https://59.ru/", new Parser59Ru(_web) }
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
                    List<PostParseResult> parseResults = await urlPair.Value.ParseAsync(urlPair.Key);

                    // Отправка текстов ИИ
                    foreach (var post in parseResults)
                    {
                        _logger.LogInformation($"Отправка поста на обработку postId={post.PostId}");
                        string fullText = string.Join("\n", post.ResultTexts);
                        try
                        {
                            JobIdData jobIdData = await _apiClient.AiGatewayEndpoint.SummarizePostAsync(new PostCreateInfo()
                                { PostId = post.PostId, Text = fullText });
                            _logger.LogInformation($"JobID поста {post.PostId} установлен {jobIdData.JobId}");
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, $"Не удалось отправить пост на обработку");
                        }
                    }
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