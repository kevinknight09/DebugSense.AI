using System.Text.Json.Serialization;

namespace DebugSense.DataFetcher.Models
{
    public class Answer
    {
        [JsonPropertyName("answer_id")]
        public int AnswerId { get; set; }
        public string Body { get; set; } = "";
    }
}
