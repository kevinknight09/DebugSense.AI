using System.Text.Json.Serialization;

namespace DebugSense.DataFetcher.Models
{
    public class DatasetItem
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string Body { get; set; } = "";
        [JsonPropertyName("accepted_answer")]
        public string AcceptedAnswer { get; set; } = "";
        public List<string> Tags { get; set; } = new();
        public int Score { get; set; }
        public string Framework { get; set; } = "";
        public string Exception { get; set; } = "";
    }
}
