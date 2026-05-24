using System;
using System.Net.Http;
using System.Threading.Tasks;
using DebugSense.Embedding;
using DebugSense.Infrastructure;
using DebugSense.Retrieval;

namespace DebugSense.Playground;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=============================================");
        Console.WriteLine(" Welcome to DebugSense AI - Search Playground");
        Console.WriteLine("=============================================\n");

        // Initialize our search and chat engine pipeline
        var httpClient = new HttpClient();
        var embeddingService = new OllamaEmbeddingService(httpClient, "nomic-embed-text");
        var vectorStore = new QdrantVectorStore("localhost", 6334);
        var searchService = new SemanticSearchService(embeddingService, vectorStore);
        
        // Add Chat and Orchestrator
        var chatService = new OllamaChatService(httpClient, "phi3");
        var orchestrator = new RagOrchestratorService(searchService, chatService);

        Console.WriteLine("System ready! Type your C# error or question.");
        Console.WriteLine("(Type 'exit' to quit)\n");

        while (true)
        {
            Console.Write("> ");
            var query = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(query)) continue;
            if (query.Trim().ToLower() == "exit") break;

            Console.WriteLine("\n[Searching vector database & Generating Answer...]\n");
            
            //var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            //var answer = await orchestrator.AnswerQuestionAsync(query);
            //stopwatch.Stop();

            Console.WriteLine("================ AI RESPONSE ================");
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            // Loop over the stream as the AI generates it
            await foreach (var token in orchestrator.AnswerQuestionStreamAsync(query))
            {
                // Use Write (not WriteLine) to print words side-by-side
                Console.Write(token);
            }
            stopwatch.Stop();
            Console.WriteLine("=============================================\n");
            Console.WriteLine($"[Metrics] Time taken: {stopwatch.ElapsedMilliseconds} ms ({stopwatch.Elapsed.TotalSeconds:F2} seconds)\n");
        }
    }
}
