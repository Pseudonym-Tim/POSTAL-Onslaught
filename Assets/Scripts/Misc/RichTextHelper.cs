using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// Helper class for rich text...
/// </summary>
public static class RichTextHelper
{
    /// <summary>
    /// Extracts rich text tags from a message and returns a list of tuples containing tag positions and tag strings...
    /// </summary>
    /// <param name="message">The message to extract tags from</param>
    /// <returns>A list of tuples where each tuple contains the position and the tag string</returns>
    public static List<(int, string)> ExtractRichTags(ref string message)
    {
        List<(int, string)> tags = new List<(int, string)>();
        int indexOffset = 0;

        foreach(Match match in Regex.Matches(message, @"<[^>]+>"))
        {
            tags.Add((match.Index - indexOffset, match.Value));
            message = message.Remove(match.Index - indexOffset, match.Length);
            indexOffset += match.Length;
        }

        return tags;
    }

    /// <summary>
    /// Reapplies rich text tags to a message at their respective positions...
    /// </summary>
    /// <param name="tags">The list of tuples containing tag positions and tag strings</param>
    /// <param name="message">The message to reapply tags to</param>
    public static void ReapplyRichTags(List<(int, string)> tags, ref string message)
    {
        StringBuilder typedMessage = new StringBuilder(message);

        foreach((int index, string tag) in tags.OrderByDescending(t => t.Item1))
        {
            typedMessage.Insert(index, tag);
        }

        message = typedMessage.ToString();
    }
}
