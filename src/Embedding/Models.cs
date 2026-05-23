using System.Text.Json.Serialization;

namespace DebugSense.Embedding;

public class OllamaEmbeddingRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = "nomic-embed-text";
    
    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = "";
}

public class OllamaEmbeddingResponse
{
    [JsonPropertyName("embedding")]
    public float[] Embedding { get; set; } = [];
}
