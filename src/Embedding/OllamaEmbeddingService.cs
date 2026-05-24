using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Text.Json;
using DebugSense.Embedding.Models;

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
    public async Task<float[]> GenerateEmbeddingAsync(string text, bool isQuery = false)
    {
        if (string.IsNullOrWhiteSpace(text))
            return [];

        string promptText = text;

        // Apply required prefixes specifically for the nomic-embed-text model
        if (_modelName == "nomic-embed-text")
        {
            string prefix = isQuery ? "search_query: " : "search_document: ";
            promptText = prefix + promptText;
        }

        var request = new OllamaEmbeddingRequest
        {
            Model = _modelName,
            Prompt = promptText
        };

        // Ollama usually runs on port 11434
        var response = await _httpClient.PostAsJsonAsync("http://localhost:11434/api/embeddings", request);
        response.EnsureSuccessStatusCode();

        var jsonStr = await response.Content.ReadAsStringAsync();
        var embeddingResponse = JsonSerializer.Deserialize<OllamaEmbeddingResponse>(jsonStr);

        return embeddingResponse?.Embedding ?? [];
    }
}
