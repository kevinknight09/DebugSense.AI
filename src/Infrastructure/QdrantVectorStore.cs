using Qdrant.Client;
using Qdrant.Client.Grpc;
using System.Collections.Generic;
using System.Threading.Tasks;
using DebugSense.Parsers;
using System;
using DebugSense.Parsers.Models;

namespace DebugSense.Infrastructure;

public class QdrantVectorStore
{
    private readonly QdrantClient _client;
    private const string CollectionName = "stackoverflow_chunks";

    public QdrantVectorStore(string host = "localhost", int port = 6334)
    {
        _client = new QdrantClient(host, port);
    }

    /// <summary>
    /// Creates the Qdrant collection if it doesn't already exist.
    /// Default vector size is 768 for nomic-embed-text. 
    /// (OpenAI text-embedding-3-small uses 1536).
    /// </summary>
    public async Task EnsureCollectionExistsAsync(ulong vectorSize = 768)
    {
        var collections = await _client.ListCollectionsAsync();
        bool exists = false;
        foreach (var c in collections)
        {
            if (c == CollectionName)
            {
                exists = true;
                break;
            }
        }

        if (!exists)
        {
            await _client.CreateCollectionAsync(CollectionName, new VectorParams { Size = vectorSize, Distance = Distance.Cosine });
            await _client.CreatePayloadIndexAsync(CollectionName, fieldName: "Content", schemaType: PayloadSchemaType.Text, indexParams: new PayloadIndexParams { TextIndexParams = new TextIndexParams { Tokenizer = TokenizerType.Word, Lowercase = true } });
            Console.WriteLine($"Created Qdrant collection: {CollectionName}");
        }
    }

    /// <summary>
    /// Upserts a batch of parsed chunks and their corresponding embedding vectors into Qdrant.
    /// </summary>
    public async Task UpsertChunksAsync(List<(ParsedChunk Chunk, float[] Vector)> items)
    {
        var points = new List<PointStruct>();

        foreach (var item in items)
        {
            var point = new PointStruct
            {
                // We generate a deterministic UUID based on the SourceId and Chunk Content 
                // so we don't duplicate data if we run the script twice.
                Id = new PointId { Uuid = GenerateDeterministicGuid(item.Chunk.SourceId + item.Chunk.Content).ToString() },
                Vectors = item.Vector
            };

            // Store metadata so we can display it in the UI and filter by it
            point.Payload.Add("SourceId", item.Chunk.SourceId);
            point.Payload.Add("Title", item.Chunk.Title);
            point.Payload.Add("ChunkType", item.Chunk.ChunkType);
            point.Payload.Add("Content", item.Chunk.Content);
            point.Payload.Add("Exception", item.Chunk.Exception ?? "Unknown");

            points.Add(point);
        }

        if (points.Count > 0)
        {
            await _client.UpsertAsync(CollectionName, points);
            Console.WriteLine($"Successfully upserted {points.Count} points to Qdrant.");
        }
    }

    /// <summary>
    /// Searches the Qdrant collection for the vectors most similar to the given query vector.
    /// </summary>
    public async Task<IReadOnlyList<ScoredPoint>> SearchAsync(float[] queryVector, string? rawKeywordText = null, ulong limit = 5)
    {
        var searchParams = new SearchParams
        {
            Exact = false,
            HnswEf = 128
        };

        // Building the BM25 Filter
        Filter filter = null;

        if (!string.IsNullOrEmpty(rawKeywordText))
        {
            filter = new Filter();
            filter.Must.Add(Conditions.MatchText("Content", rawKeywordText));
        }

        var results = await _client.SearchAsync(
            CollectionName,
            queryVector,
            filter: filter,
            limit: limit,
            searchParams: searchParams
        );

        return results;
    }

    private Guid GenerateDeterministicGuid(string input)
    {
        using (var md5 = System.Security.Cryptography.MD5.Create())
        {
            byte[] hash = md5.ComputeHash(System.Text.Encoding.Default.GetBytes(input));
            return new Guid(hash);
        }
    }
}
