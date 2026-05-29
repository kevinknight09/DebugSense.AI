using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DebugSense.Embedding;
using DebugSense.Infrastructure;

namespace DebugSense.Retrieval;

public class SearchResult
{
    public float Score { get; set; }
    public string SourceId { get; set; } = "";
    public string Title { get; set; } = "";
    public string ChunkType { get; set; } = "";
    public string Content { get; set; } = "";
    public string Exception { get; set; } = "";
}

public class SemanticSearchService
{
    private readonly OllamaEmbeddingService _embeddingService;
    private readonly QdrantVectorStore _vectorStore;

    public SemanticSearchService(OllamaEmbeddingService embeddingService, QdrantVectorStore vectorStore)
    {
        _embeddingService = embeddingService;
        _vectorStore = vectorStore;
    }

    /// <summary>
    /// Converts the user's natural language query into a vector and searches the database.
    /// </summary>
    public async Task<List<SearchResult>> SearchAsync(string queryText, ulong limit = 3)
    {
        // 1. Convert text to vector
        var queryVector = await _embeddingService.GenerateEmbeddingAsync(queryText, isQuery: true);

        if (queryVector == null || queryVector.Length == 0)
        {
            return new List<SearchResult>();
        }

        // 2. Perform vector search in Qdrant
        var rawResults = await _vectorStore.SearchAsync(queryVector, rawKeywordText:queryText,  limit);

        // 3. Map Qdrant points to SearchResult DTOs
        var searchResults = rawResults.Select(r => new SearchResult
        {
            Score = r.Score,
            SourceId = r.Payload.TryGetValue("SourceId", out var sid) ? sid.StringValue : "",
            Title = r.Payload.TryGetValue("Title", out var t) ? t.StringValue : "",
            ChunkType = r.Payload.TryGetValue("ChunkType", out var ct) ? ct.StringValue : "",
            Content = r.Payload.TryGetValue("Content", out var c) ? c.StringValue : "",
            Exception = r.Payload.TryGetValue("Exception", out var e) ? e.StringValue : ""
        }).ToList();

        return searchResults;
    }
}
