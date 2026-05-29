using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using DebugSense.DataFetcher;
using DebugSense.DataFetcher.Models;
using DebugSense.Embedding;
using DebugSense.Infrastructure;
using DebugSense.Parsers;
using DebugSense.Parsers.Models;

namespace DebugSense.DataIngestor;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Starting Data Ingestion Pipeline...");

        // 1. Load the dataset
        string? currentDir = AppDomain.CurrentDomain.BaseDirectory;
        string dataDir = "";
        
        while (currentDir != null)
        {
            var potentialDataDir = Path.Combine(currentDir, "data");
            if (Directory.Exists(potentialDataDir) && File.Exists(Path.Combine(potentialDataDir, "sample_dataset.json")))
            {
                dataDir = potentialDataDir;
                break;
            }
            currentDir = Directory.GetParent(currentDir)?.FullName;
        }

        if (string.IsNullOrEmpty(dataDir))
        {
            // Fallback to absolute workspace path if running in a unique context
            dataDir = Path.Combine(Directory.GetCurrentDirectory(), "data");
        }

        string filePath = Path.Combine(dataDir, "sample_dataset.json");
        
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"Dataset not found at {filePath}. Please run the DataFetcher first.");
            return;
        }

        string jsonStr = await File.ReadAllTextAsync(filePath);
        var dataset = JsonSerializer.Deserialize<List<DatasetItem>>(jsonStr, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        if (dataset == null || dataset.Count == 0)
        {
            Console.WriteLine("Dataset is empty.");
            return;
        }

        Console.WriteLine($"Loaded {dataset.Count} items from dataset.");

        // 2. Initialize Services
        var parser = new DocumentParser();
        var httpClient = new HttpClient();
        var embeddingService = new OllamaEmbeddingService(httpClient, "nomic-embed-text");
        var vectorStore = new QdrantVectorStore("localhost", 6334);

        // Ensure Qdrant collection exists (vector size 768 for nomic-embed-text)
        await vectorStore.EnsureCollectionExistsAsync(768);

        var batch = new List<(ParsedChunk Chunk, float[] Vector)>();

        // 3. Process each item
        int count = 0;
        foreach (var item in dataset)
        {
            count++;
            Console.WriteLine($"Processing item {count}/{dataset.Count}: {item.Title}");

            // Map DatasetItem to RawDatasetItem for the parser
            var rawItem = new RawDatasetItem
            {
                Id = item.Id,
                Title = item.Title,
                Body = item.Body,
                AcceptedAnswer = item.AcceptedAnswer,
                Tags = item.Tags,
                Exception = item.Exception
            };

            var chunks = parser.Parse(rawItem);
            Console.WriteLine($"  - Generated {chunks.Count} chunks.");

            foreach (var chunk in chunks)
            {
                // Generate embedding
                var vector = await embeddingService.GenerateEmbeddingAsync(chunk.Content);
                
                if (vector.Length > 0)
                {
                    batch.Add((chunk, vector));
                }
            }

            // Upsert in batches of 50 to avoid overloading memory/API
            if (batch.Count >= 50)
            {
                await vectorStore.UpsertChunksAsync(batch);
                batch.Clear();
            }
        }

        // Upsert any remaining
        if (batch.Count > 0)
        {
            await vectorStore.UpsertChunksAsync(batch);
        }

        Console.WriteLine("Data Ingestion Pipeline completed successfully.");
    }
}
