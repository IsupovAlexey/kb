using System.Collections;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

static partial class YamlMergeAddOnly
{
    static List<string> ExtractFragment(IReadOnlyList<string> templateLines, string[] segments)
    {
        var (start, end) = FindNodeRange(templateLines, segments, 0);
        return start < 0 ? [] : templateLines.Skip(start).Take(end - start).ToList();
    }

    static List<string> ExtractMissingListItemLines(
        IReadOnlyList<string> templateLines,
        string[] segments,
        Dictionary<string, object?> hostRoot
    )
    {
        var hostList = GetHostList(hostRoot, segments);
        if (hostList is null)
        {
            return [];
        }

        var hostItems = hostList.Cast<object?>().ToList();
        var (start, end) = FindNodeRange(templateLines, segments, 0);
        if (start < 0)
        {
            return [];
        }

        var missing = new List<string>();
        for (var i = start + 1; i < end; i++)
        {
            if (!YamlListItemRegex.IsMatch(templateLines[i]))
            {
                continue;
            }

            var itemYaml = templateLines[i].TrimStart()[2..].Trim();
            var parsed = Deserializer.Deserialize<object?>(itemYaml);
            if (!hostItems.Any(h => ValuesEqual(h, parsed)))
            {
                missing.Add(templateLines[i]);
            }
        }

        return missing;
    }

    static IList? GetHostList(Dictionary<string, object?> hostRoot, string[] segments)
    {
        object? current = hostRoot;
        foreach (var seg in segments)
        {
            var dict = AsDict(current);
            if (dict is null || !dict.TryGetValue(seg, out current))
            {
                return null;
            }
        }

        return current as IList;
    }

    static (int Start, int EndExclusive) FindNodeRange(
        IReadOnlyList<string> lines,
        string[] segments,
        int searchFrom
    )
    {
        var lineIdx = searchFrom;
        var parentIndent = -1;

        for (var s = 0; s < segments.Length; s++)
        {
            var key = segments[s];
            var found = false;
            for (var i = lineIdx; i < lines.Count; i++)
            {
                var m = YamlKeyLineRegex.Match(lines[i]);
                if (!m.Success)
                {
                    continue;
                }

                var indent = m.Groups["indent"].Value.Length;
                if (s == 0)
                {
                    if (indent != 0 || m.Groups["key"].Value != key)
                    {
                        continue;
                    }
                }
                else if (indent <= parentIndent || m.Groups["key"].Value != key)
                {
                    continue;
                }

                lineIdx = i;
                parentIndent = indent;
                found = true;
                break;
            }

            if (!found)
            {
                return (-1, -1);
            }
        }

        var keyIndent = parentIndent;
        var end = lineIdx + 1;
        while (end < lines.Count)
        {
            var line = lines[end];
            if (string.IsNullOrWhiteSpace(line))
            {
                end++;
                continue;
            }

            var m = YamlKeyLineRegex.Match(line);
            if (m.Success && m.Groups["indent"].Value.Length <= keyIndent)
            {
                break;
            }

            end++;
        }

        return (lineIdx, end);
    }

    static List<string> InsertFragment(
        List<string> hostLines,
        string[] segments,
        List<string> fragment,
        bool isListItem
    )
    {
        if (isListItem)
        {
            var (listStart, listEnd) = FindNodeRange(hostLines, segments, 0);
            if (listStart < 0)
            {
                return hostLines.Concat(fragment).ToList();
            }

            hostLines.InsertRange(listEnd, fragment);
            return hostLines;
        }

        if (FindNodeRange(hostLines, segments, 0).Start >= 0)
        {
            return hostLines;
        }

        if (segments.Length == 1)
        {
            if (hostLines.Count > 0 && !string.IsNullOrWhiteSpace(hostLines[^1]))
            {
                hostLines.Add("");
            }

            hostLines.AddRange(fragment);
            return hostLines;
        }

        var parentSegments = segments[..^1];
        var (parentStart, parentEnd) = FindNodeRange(hostLines, parentSegments, 0);
        if (parentStart < 0)
        {
            if (hostLines.Count > 0 && !string.IsNullOrWhiteSpace(hostLines[^1]))
            {
                hostLines.Add("");
            }

            hostLines.AddRange(fragment);
            return hostLines;
        }

        var parentKeyLine = hostLines[parentStart];
        var parentIndent = YamlKeyLineRegex.Match(parentKeyLine).Groups["indent"].Value.Length;
        var childIndent = parentIndent + 2;
        var insertAt = parentEnd;

        for (var i = parentStart + 1; i < parentEnd; i++)
        {
            var m = YamlKeyLineRegex.Match(hostLines[i]);
            if (!m.Success || m.Groups["indent"].Value.Length != childIndent)
            {
                continue;
            }

            var siblingKey = m.Groups["key"].Value;
            if (string.Compare(siblingKey, segments[^1], StringComparison.Ordinal) > 0)
            {
                insertAt = i;
                break;
            }

            insertAt = FindNodeRange(
                hostLines,
                parentSegments.Concat([siblingKey]).ToArray(),
                parentStart
            ).EndExclusive;
        }

        hostLines.InsertRange(insertAt, fragment);
        return hostLines;
    }

    static List<string> RemoveKeyFragment(List<string> hostLines, string[] segments)
    {
        var (start, end) = FindNodeRange(hostLines, segments, 0);
        if (start < 0)
        {
            return hostLines;
        }

        hostLines.RemoveRange(start, end - start);
        if (
            start < hostLines.Count
            && string.IsNullOrWhiteSpace(hostLines[start])
            && start > 0
            && string.IsNullOrWhiteSpace(hostLines[start - 1])
        )
        {
            hostLines.RemoveAt(start);
        }

        return hostLines;
    }

    static List<string> RemoveObsoleteListItems(
        List<string> hostLines,
        string[] segments,
        Dictionary<string, object?> templateRoot
    )
    {
        var templateList = GetHostList(templateRoot, segments);
        if (templateList is null)
        {
            return hostLines;
        }

        var templateItems = templateList.Cast<object?>().ToList();
        var (start, end) = FindNodeRange(hostLines, segments, 0);
        if (start < 0)
        {
            return hostLines;
        }

        for (var i = end - 1; i > start; i--)
        {
            if (!YamlListItemRegex.IsMatch(hostLines[i]))
            {
                continue;
            }

            var itemYaml = hostLines[i].TrimStart()[2..].Trim();
            var parsed = Deserializer.Deserialize<object?>(itemYaml);
            if (!templateItems.Any(t => ValuesEqual(t, parsed)))
            {
                hostLines.RemoveAt(i);
            }
        }

        return hostLines;
    }
}
