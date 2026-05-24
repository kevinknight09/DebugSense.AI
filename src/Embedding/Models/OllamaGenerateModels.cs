using System.Text.Json.Serialization;

namespace DebugSense.Embedding.Models;

public class OllamaGenerateRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = "";

    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = "";

    [JsonPropertyName("stream")]
    public bool Stream { get; set; } = false;
}

public class OllamaGenerateResponse
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = "";

    [JsonPropertyName("response")]
    public string Response { get; set; } = "";

    [JsonPropertyName("done")]
    public bool Done { get; set; }
}
