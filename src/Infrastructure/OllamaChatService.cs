using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using DebugSense.Embedding.Models;

namespace DebugSense.Infrastructure;

public class OllamaChatService
{
    private readonly HttpClient _httpClient;
    private readonly string _modelName;

    public OllamaChatService(HttpClient httpClient, string modelName = "phi3")
    {
        _httpClient = httpClient;
        _modelName = modelName;
    }

    /// <summary>
    /// Generates a response using the local Ollama LLM.
    /// </summary>
    public async Task<string> GenerateResponseAsync(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            return string.Empty;

        var request = new OllamaGenerateRequest
        {
            Model = _modelName,
            Prompt = prompt,
            Stream = false
        };

        try
        {
            // The default generation endpoint for Ollama
            var response = await _httpClient.PostAsJsonAsync("http://localhost:11434/api/generate", request);
            response.EnsureSuccessStatusCode();

            var jsonStr = await response.Content.ReadAsStringAsync();
            var generateResponse = JsonSerializer.Deserialize<OllamaGenerateResponse>(jsonStr);

            return generateResponse?.Response ?? "No response generated.";
        }
        catch (Exception ex)
        {
            return $"Error generating response from Ollama ({_modelName}): {ex.Message}";
        }
    }
}
