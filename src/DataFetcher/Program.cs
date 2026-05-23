using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DebugSense.DataFetcher;

class Program
{
    private static readonly HttpClient _httpClient = new HttpClient(new HttpClientHandler
    {
        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
    });

    static async Task Main(string[] args)
    {
        Console.WriteLine("Fetching top 100 C# exception questions from StackOverflow...");
        
        string questionsUrl = "https://api.stackexchange.com/2.3/search/advanced?pagesize=100&order=desc&sort=votes&accepted=True&tagged=c%23;exception&site=stackoverflow&filter=withbody";
        
        var request = new HttpRequestMessage(HttpMethod.Get, questionsUrl);
        // StackExchange API requires a user-agent
        request.Headers.Add("User-Agent", "DebugSenseDataFetcher/1.0");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var jsonStr = await response.Content.ReadAsStringAsync();
        var questionsResponse = JsonSerializer.Deserialize<StackExchangeResponse<Question>>(jsonStr, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var questions = questionsResponse?.Items ?? new List<Question>();
        Console.WriteLine($"Found {questions.Count} questions. Fetching their accepted answers...");

        // Extract accepted answer IDs
        var answerIds = questions.Select(q => q.AcceptedAnswerId).Where(id => id > 0).ToList();
        
        var answersUrl = $"https://api.stackexchange.com/2.3/answers/{string.Join(";", answerIds)}?site=stackoverflow&filter=withbody";
        var answerRequest = new HttpRequestMessage(HttpMethod.Get, answersUrl);
        answerRequest.Headers.Add("User-Agent", "DebugSenseDataFetcher/1.0");
        
        var answerResponse = await _httpClient.SendAsync(answerRequest);
        answerResponse.EnsureSuccessStatusCode();
        
        var answerJsonStr = await answerResponse.Content.ReadAsStringAsync();
        var answersResponse = JsonSerializer.Deserialize<StackExchangeResponse<Answer>>(answerJsonStr, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        var answers = answersResponse?.Items ?? new List<Answer>();
        var answerMap = answers.ToDictionary(a => a.AnswerId, a => a.Body);

        // Map everything together
        var dataset = new List<DatasetItem>();
        foreach (var q in questions)
        {
            if (answerMap.TryGetValue(q.AcceptedAnswerId, out var answerBody))
            {
                dataset.Add(new DatasetItem
                {
                    Id = q.QuestionId.ToString(),
                    Title = q.Title,
                    Body = q.Body,
                    AcceptedAnswer = answerBody,
                    Tags = q.Tags,
                    Score = q.Score,
                    Framework = "C# / .NET",
                    Exception = "Unknown" // We can parse this later
                });
            }
        }

        Console.WriteLine($"Successfully matched {dataset.Count} questions with answers.");
        
        string dataDir = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "data");
        if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);
        
        string filePath = Path.Combine(dataDir, "sample_dataset.json");
        var saveJson = JsonSerializer.Serialize(dataset, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(filePath, saveJson);

        Console.WriteLine($"Dataset saved to {filePath}");
    }
}

public class StackExchangeResponse<T>
{
    public List<T> Items { get; set; } = new();
}

public class Question
{
    [JsonPropertyName("question_id")]
    public int QuestionId { get; set; }
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
    [JsonPropertyName("accepted_answer_id")]
    public int AcceptedAnswerId { get; set; }
    public List<string> Tags { get; set; } = new();
    public int Score { get; set; }
}

public class Answer
{
    [JsonPropertyName("answer_id")]
    public int AnswerId { get; set; }
    public string Body { get; set; } = "";
}

public class DatasetItem
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
    [JsonPropertyName("accepted_answer")]
    public string AcceptedAnswer { get; set; } = "";
    public List<string> Tags { get; set; } = new();
    public int Score { get; set; }
    public string Framework { get; set; } = "";
    public string Exception { get; set; } = "";
}
