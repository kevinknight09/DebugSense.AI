using System;
using System.Collections.Generic;
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

    //Used for streaming responses, rather than waiting for the full response to be generated.
    public async IAsyncEnumerable<string> GenerateResponseStreamAsync(string prompt)
    {
        var request = new OllamaGenerateRequest
        {
            Model = _modelName,
            Prompt = prompt,
            Stream = true
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "http://localhost:11434/api/generate")
        {
            Content = JsonContent.Create(request)
        };

        HttpResponseMessage? response = null;
        System.IO.Stream? stream = null;

        Exception? networkException = null;
        try
        {
            response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            stream = await response.Content.ReadAsStreamAsync();
        }
        catch (Exception ex)
        {
            networkException = ex;
        }

        if (networkException != null || stream == null)
        {
            yield return $"\n[Error connecting to Ollama ({_modelName}): {networkException?.Message ?? "Unknown error"}]\n";
            yield break;
        }

        using var reader = new System.IO.StreamReader(stream);
        
        while (!reader.EndOfStream)
        {
            string? line = null;
            Exception? streamException = null;
            
            try
            {
                line = await reader.ReadLineAsync();
            }
            catch (Exception ex)
            {
                streamException = ex;
            }

            if (streamException != null)
            {
                yield return $"\n[Error reading stream: {streamException.Message}]\n";
                yield break;
            }

            if (string.IsNullOrWhiteSpace(line)) continue;

            OllamaGenerateResponse? chunk = null;
            try
            {
                chunk = JsonSerializer.Deserialize<OllamaGenerateResponse>(
                    line, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
            }
            catch (Exception ex)
            {
                // If a specific line fails to parse, we can log it or yield an error, 
                // but we might want to just skip it and keep the stream alive.
                continue; 
            }

            if (chunk != null && !string.IsNullOrEmpty(chunk.Response))
            {
                yield return chunk.Response;
            }
        }
    }
}
