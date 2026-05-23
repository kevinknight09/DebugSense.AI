using System;
using System.Collections.Generic;
using System.Text;

namespace DebugSense.Parsers.Models
{
    public class ParsedChunk
    {
        public string SourceId { get; set; } = "";
        public string Title { get; set; } = "";
        public string ChunkType { get; set; } = ""; // e.g., "QuestionBody", "QuestionCode", "AnswerBody", "AnswerCode"
        public string Content { get; set; } = "";
        public List<string> Tags { get; set; } = new();
        public string Exception { get; set; } = "";
    }
}
