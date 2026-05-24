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

        // Initialize our search engine pipeline
        var httpClient = new HttpClient();
        var embeddingService = new OllamaEmbeddingService(httpClient, "nomic-embed-text");
        var vectorStore = new QdrantVectorStore("localhost", 6334);
        var searchService = new SemanticSearchService(embeddingService, vectorStore);

        Console.WriteLine("System ready! Type your C# error or question.");
        Console.WriteLine("(Type 'exit' to quit)\n");

        while (true)
        {
            Console.Write("> ");
            var query = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(query)) continue;
            if (query.Trim().ToLower() == "exit") break;

            Console.WriteLine("\n[Searching vector database...]\n");
            
            var results = await searchService.SearchAsync(query, limit: 3);

            if (results.Count == 0)
            {
                Console.WriteLine("No relevant results found.\n");
                continue;
            }

            for (int i = 0; i < results.Count; i++)
            {
                var r = results[i];
                Console.WriteLine($"--- Result #{i + 1} (Score: {r.Score:F4}) ---");
                Console.WriteLine($"Title: {r.Title}");
                Console.WriteLine($"Type:  {r.ChunkType}");
                
                // Truncate long content for display purposes
                var displayContent = r.Content.Length > 300 
                    ? r.Content.Substring(0, 300) + "..." 
                    : r.Content;
                
                Console.WriteLine($"Content:\n{displayContent}\n");
            }
        }
    }
}
