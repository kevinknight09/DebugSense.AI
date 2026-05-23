using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Text.Json;

namespace DebugSense.Embedding;

public class OllamaEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly string _modelName;

    public OllamaEmbeddingService(HttpClient httpClient, string modelName = "nomic-embed-text")
    {
        _httpClient = httpClient;
        _modelName = modelName;
    }

    /// <summary>
    /// Generates an embedding vector for the provided text using a local Ollama model.
    /// </summary>
    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return [];

        var request = new OllamaEmbeddingRequest
        {
            Model = _modelName,
            Prompt = text
        };

        // Ollama usually runs on port 11434
        var response = await _httpClient.PostAsJsonAsync("http://localhost:11434/api/embeddings", request);
        response.EnsureSuccessStatusCode();

        var jsonStr = await response.Content.ReadAsStringAsync();
        var embeddingResponse = JsonSerializer.Deserialize<OllamaEmbeddingResponse>(jsonStr);

        return embeddingResponse?.Embedding ?? [];
    }
}
