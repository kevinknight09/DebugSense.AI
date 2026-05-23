using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace DebugSense.Embedding.Models
{
    public class OllamaEmbeddingResponse
    {
        [JsonPropertyName("embedding")]
        public float[] Embedding { get; set; } = [];
    }
}
