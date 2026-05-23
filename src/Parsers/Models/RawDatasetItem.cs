using System;
using System.Collections.Generic;
using System.Text;

namespace DebugSense.Parsers.Models
{
    public class RawDatasetItem
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string Body { get; set; } = "";
        public string AcceptedAnswer { get; set; } = "";
        public List<string> Tags { get; set; } = new();
        public int Score { get; set; }
        public string Framework { get; set; } = "";
        public string Exception { get; set; } = "";
    }
}
