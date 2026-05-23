using HtmlAgilityPack;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using DebugSense.Parsers.Models;

namespace DebugSense.Parsers;

public class DocumentParser
{
    /// <summary>
    /// Parses a raw StackOverflow dataset item into multiple clean text/code chunks.
    /// </summary>
    public List<ParsedChunk> Parse(RawDatasetItem item)
    {
        var chunks = new List<ParsedChunk>();

        // Parse Question
        var questionChunks = ExtractChunksFromHtml(item.Body, "Question");
        foreach (var qc in questionChunks)
        {
            chunks.Add(new ParsedChunk
            {
                SourceId = item.Id,
                Title = item.Title,
                ChunkType = qc.Type,
                Content = qc.Content,
                Tags = item.Tags,
                Exception = item.Exception
            });
        }

        // Parse Accepted Answer
        var answerChunks = ExtractChunksFromHtml(item.AcceptedAnswer, "Answer");
        foreach (var ac in answerChunks)
        {
            chunks.Add(new ParsedChunk
            {
                SourceId = item.Id,
                Title = item.Title,
                ChunkType = ac.Type,
                Content = ac.Content,
                Tags = item.Tags,
                Exception = item.Exception
            });
        }

        return chunks;
    }

    /// <summary>
    /// Iterates through HTML nodes and separates code blocks from standard textual body blocks.
    /// </summary>
    private List<(string Type, string Content)> ExtractChunksFromHtml(string html, string sectionPrefix)
    {
        var result = new List<(string Type, string Content)>();
        if (string.IsNullOrWhiteSpace(html)) return result;

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var textBuilder = new StringBuilder();

        // StackOverflow structures content as top-level children (p, pre, ul, etc.)
        foreach (var node in doc.DocumentNode.ChildNodes)
        {
            if (node.Name.ToLower() == "pre" || node.Name.ToLower() == "code")
            {
                // If we have accumulated text before this code block, flush it as a Body chunk
                if (textBuilder.Length > 0)
                {
                    string bodyContent = CleanText(textBuilder.ToString());
                    if (!string.IsNullOrWhiteSpace(bodyContent))
                    {
                        result.Add(($"{sectionPrefix}Body", bodyContent));
                    }
                    textBuilder.Clear();
                }
                
                // Add the code chunk (preserving its line breaks but decoding HTML entities)
                string codeText = WebUtility.HtmlDecode(node.InnerText).Trim();
                if (!string.IsNullOrWhiteSpace(codeText))
                {
                    result.Add(($"{sectionPrefix}Code", codeText));
                }
            }
            else
            {
                // Standard text (p, h1, ul, etc.)
                textBuilder.AppendLine(node.InnerText);
            }
        }

        // Flush any remaining text
        if (textBuilder.Length > 0)
        {
            string finalBody = CleanText(textBuilder.ToString());
            if (!string.IsNullOrWhiteSpace(finalBody))
            {
                result.Add(($"{sectionPrefix}Body", finalBody));
            }
        }

        return result;
    }

    /// <summary>
    /// Removes HTML entities, strips newlines from paragraph text, and normalizes spacing.
    /// </summary>
    private string CleanText(string text)
    {
        text = WebUtility.HtmlDecode(text);
        text = text.Replace("\r", "").Replace("\n", " ");
        // Collapse multiple spaces into one
        return Regex.Replace(text, @"\s+", " ").Trim();
    }
}
