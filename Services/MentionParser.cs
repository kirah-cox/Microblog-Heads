using System.Text.RegularExpressions;

namespace microblog_heads.Services;

/// <summary>
/// Finds @mentions in post and comment text. A mention is "@" at the start of the text or after
/// whitespace, followed by everything up to the next whitespace ("kirah@snow.edu" is not a mention).
///
/// Because "up to the next space" also picks up punctuation ("@kirah!" gives "kirah!"), each
/// mention is matched against real usernames as typed first, then with trailing punctuation removed.
///
/// Usage:
///   var lookupNames = MentionParser.GetLookupNames(content);
///   var users = await db.RegularUsers.Where(u => lookupNames.Contains(u.Username.ToLower())).ToListAsync();
///   var mentioned = MentionParser.MatchUsernames(content, users.Select(u => u.Username));
/// </summary>
public static class MentionParser
{
    private static readonly Regex MentionRegex = new(@"(?<!\S)@(\S+)", RegexOptions.Compiled);

    private static readonly char[] TrailingPunctuation = { '.', ',', '!', '?', ':', ';', ')', ']', '}', '\'', '"' };

    /// <summary>Every "@word" in the text, without the "@", as typed. Duplicates removed (ignoring case).</summary>
    public static List<string> FindMentions(string? text) =>
        MentionRegex.Matches(text ?? "")
            .Select(m => m.Groups[1].Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    /// <summary>
    /// Lowercased names to look up in the database: each mention as typed, plus the version with
    /// trailing punctuation removed. Use this for a single query.
    /// </summary>
    public static List<string> GetLookupNames(string? text) =>
        FindMentions(text)
            .SelectMany(m => new[] { m, TrimTrailingPunctuation(m) })
            .Where(n => n.Length > 0)
            .Select(n => n.ToLowerInvariant())
            .Distinct()
            .ToList();

    /// <summary>
    /// Which of the given existing usernames are mentioned in the text. A mention matches a
    /// username exactly as typed (ignoring case) if one exists; otherwise the version without
    /// trailing punctuation is tried. Returns usernames as they are stored, no duplicates.
    /// </summary>
    public static List<string> MatchUsernames(string? text, IEnumerable<string> existingUsernames)
    {
        var byLower = new Dictionary<string, string>();
        foreach (var name in existingUsernames)
            byLower.TryAdd(name.ToLowerInvariant(), name);

        var matched = new List<string>();
        foreach (var mention in FindMentions(text))
        {
            if (byLower.TryGetValue(mention.ToLowerInvariant(), out var exact)
                || byLower.TryGetValue(TrimTrailingPunctuation(mention).ToLowerInvariant(), out exact))
            {
                if (!matched.Contains(exact)) matched.Add(exact);
            }
        }
        return matched;
    }

    private static string TrimTrailingPunctuation(string mention) => mention.TrimEnd(TrailingPunctuation);
}
