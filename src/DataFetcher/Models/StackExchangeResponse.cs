using System;
using System.Collections.Generic;
using System.Text;

namespace DebugSense.DataFetcher.Models
{
    public class StackExchangeResponse<T>
    {
        public List<T> Items { get; set; } = new();
    }
}
