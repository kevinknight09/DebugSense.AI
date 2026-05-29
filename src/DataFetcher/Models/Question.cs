using System.Text.Json.Serialization;

namespace DebugSense.DataFetcher.Models
{
    public class Question
    {
        [JsonPropertyName("question_id")]
        public int QuestionId { get; set; }
        public string Title { get; set; } = "";
        public string Body { get; set; } = "";
        [JsonPropertyName("accepted_answer_id")]
        public int AcceptedAnswerId { get; set; }
        public List<string> Tags { get; set; } = new();
        public int Score { get; set; }
    }
}
