using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DebugSense.Retrieval;

public static class KeywordExtractor
{
    // Matches PascalCase words ending in Exception, Error, or Fault (e.g. NullReferenceException)
    private static readonly Regex ExceptionRegex = new Regex(@"\b[A-Z][a-zA-Z]*(Exception|Error|Fault)\b", RegexOptions.Compiled);
    
    // Matches Hex Codes (e.g. 0x80004005)
    private static readonly Regex HexRegex = new Regex(@"\b0x[0-9A-Fa-f]+\b", RegexOptions.Compiled);
    
    // Matches Error Codes like CS0118 or HTTP 404
    private static readonly Regex ErrorCodeRegex = new Regex(@"\b(CS[0-9]+|HTTP\s*[0-9]{3})\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Extracts highly specific technical keywords from a raw natural language query.
    /// Returns null if no strict keywords are found, allowing graceful fallback to dense search.
    /// </summary>
    public static string? Extract(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        var extractedKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 1. Extract Exceptions
        foreach (Match match in ExceptionRegex.Matches(input))
        {
            extractedKeywords.Add(match.Value);
        }

        // 2. Extract Hex Codes
        foreach (Match match in HexRegex.Matches(input))
        {
            extractedKeywords.Add(match.Value);
        }

        // 3. Extract Error Codes
        foreach (Match match in ErrorCodeRegex.Matches(input))
        {
            extractedKeywords.Add(match.Value);
        }

        if (extractedKeywords.Count > 0)
        {
            return string.Join(" ", extractedKeywords);
        }

        return null;
    }
}
