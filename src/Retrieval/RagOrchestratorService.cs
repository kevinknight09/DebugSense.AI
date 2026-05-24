using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DebugSense.Infrastructure;

namespace DebugSense.Retrieval;

public class RagOrchestratorService
{
    private readonly SemanticSearchService _searchService;
    private readonly OllamaChatService _chatService;

    public RagOrchestratorService(SemanticSearchService searchService, OllamaChatService chatService)
    {
        _searchService = searchService;
        _chatService = chatService;
    }

    public async Task<string> AnswerQuestionAsync(string queryText)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        // 1. Retrieve the top 3 most relevant chunks
        var searchResults = await _searchService.SearchAsync(queryText, limit: 3);

        if (searchResults.Count == 0)
        {
            return "I couldn't find any relevant StackOverflow solutions in the database for that error.";
        }
        stopwatch.Stop();
        Console.WriteLine($"[Metrics] Retrieval time: {stopwatch.ElapsedMilliseconds} ms ({stopwatch.Elapsed.TotalSeconds:F2} seconds)");

        // 2. Build the context block from the chunks
        var contextBuilder = new StringBuilder();
        foreach (var result in searchResults)
        {
            contextBuilder.AppendLine($"[Title: {result.Title}]");
            contextBuilder.AppendLine($"[Type: {result.ChunkType}]");
            contextBuilder.AppendLine(result.Content);
            contextBuilder.AppendLine("---");
        }

        string context = contextBuilder.ToString();

        // 3. Build the prompt
        string prompt = $@"You are 'DebugSense', an expert C# debugging assistant. 
Your goal is to help the user fix their code error using ONLY the provided StackOverflow context. 

If the context does not contain the answer, say 'I don't have enough context to answer this'. 
Do not hallucinate external information. 

CONTEXT:
{context}

USER ERROR / QUESTION:
{queryText}

EXPERT ANSWER:
";

        // 4. Generate the response
        string response = await _chatService.GenerateResponseAsync(prompt);
        return response;
    }
}
