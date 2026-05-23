using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace DebugSense.Embedding.Models
{
    public class OllamaEmbeddingRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = "nomic-embed-text";

        [JsonPropertyName("prompt")]
        public string Prompt { get; set; } = "";
    }
}
