using System.Collections;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

static partial class YamlMergeAddOnly
{
    sealed record Patch(string Path, bool IsListItem, bool Remove);

    static readonly IDeserializer Deserializer = new DeserializerBuilder().Build();
    static readonly Regex YamlKeyLineRegex = new(
        @"^(?<indent>[ \t]*)(?<key>[\w.-]+)\s*:",
        RegexOptions.Compiled
    );
    static readonly Regex YamlListItemRegex = new(@"^(?<indent>[ \t]*)-\s+", RegexOptions.Compiled);

    public static (
        string Merged,
        IReadOnlyList<string> AddedPaths,
        IReadOnlyList<string> RemovedPaths
    ) Merge(
        string hostYaml,
        string templateYaml,
        bool removeHostOnlyKeys = false,
        IReadOnlyList<string>? removePaths = null
    )
    {
        var hostDict = ParseRoot(hostYaml);
        var templateDict = ParseRoot(templateYaml);
        var patches = new List<Patch>();
        CollectMissingPatches(templateDict, hostDict, "", patches);
        if (removeHostOnlyKeys)
        {
            CollectObsoletePatches(templateDict, hostDict, "", patches);
        }

        if (removePaths is { Count: > 0 })
        {
            CollectExplicitRemovals(hostDict, removePaths, patches);
        }

        if (patches.Count == 0)
        {
            return (hostYaml, Array.Empty<string>(), Array.Empty<string>());
        }

        var hostLines = SplitLines(hostYaml);
        var templateLines = SplitLines(templateYaml);

        var removals = patches
            .Where(p => p.Remove)
            .OrderByDescending(p => p.Path.Count(c => c == '.'))
            .ThenByDescending(p => p.Path, StringComparer.Ordinal)
            .ToList();

        foreach (var patch in removals)
        {
            var segments = patch.Path.Split('.');
            hostLines = patch.IsListItem
                ? RemoveObsoleteListItems(hostLines, segments, templateDict)
                : RemoveKeyFragment(hostLines, segments);
        }

        var additions = patches
            .Where(p => !p.Remove)
            .OrderBy(p => p.Path.Count(c => c == '.'))
            .ThenBy(p => p.Path, StringComparer.Ordinal)
            .ToList();

        foreach (var patch in additions)
        {
            var segments = patch.Path.Split('.');
            var fragment = patch.IsListItem
                ? ExtractMissingListItemLines(templateLines, segments, hostDict)
                : ExtractFragment(templateLines, segments);

            if (fragment.Count == 0)
            {
                continue;
            }

            hostLines = InsertFragment(hostLines, segments, fragment, patch.IsListItem);
        }

        var merged = string.Join("\n", hostLines);
        if (!hostYaml.EndsWith('\n') && merged.Length > 0)
        {
            merged += "\n";
        }

        var addedPaths = patches.Where(p => !p.Remove).Select(p => p.Path).Distinct().ToList();
        var removedPaths = patches.Where(p => p.Remove).Select(p => p.Path).Distinct().ToList();
        return (merged, addedPaths, removedPaths);
    }

    static List<string> SplitLines(string yaml) =>
        yaml.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n').ToList();

    public static Dictionary<string, object?> ParseRoot(string yaml)
    {
        var root = Deserializer.Deserialize<object?>(new StringReader(yaml));
        return AsDict(root) ?? new Dictionary<string, object?>(StringComparer.Ordinal);
    }

    static Dictionary<string, object?>? AsDict(object? node)
    {
        if (node is Dictionary<string, object?> sd)
        {
            return sd;
        }

        if (node is Dictionary<object, object?> odd)
        {
            return odd.ToDictionary(
                kv => kv.Key.ToString() ?? "",
                kv => NormalizeNode(kv.Value),
                StringComparer.Ordinal
            );
        }

        if (node is IDictionary<object, object> id)
        {
            return id.ToDictionary(
                kv => kv.Key.ToString() ?? "",
                kv => NormalizeNode(kv.Value),
                StringComparer.Ordinal
            );
        }

        return null;
    }

    static object? NormalizeNode(object? node)
    {
        if (
            node
            is Dictionary<string, object?>
                or Dictionary<object, object?>
                or IDictionary<object, object>
        )
        {
            return AsDict(node);
        }

        if (node is IList list)
        {
            return list.Cast<object?>().Select(NormalizeNode).ToList();
        }

        return node;
    }

    static void CollectMissingPatches(
        Dictionary<string, object?> template,
        Dictionary<string, object?> host,
        string prefix,
        List<Patch> patches
    )
    {
        foreach (var (key, templateVal) in template)
        {
            var path = string.IsNullOrEmpty(prefix) ? key : $"{prefix}.{key}";
            if (!host.TryGetValue(key, out var hostVal))
            {
                patches.Add(new Patch(path, IsListItem: false, Remove: false));
                continue;
            }

            if (
                templateVal is Dictionary<string, object?> tDict
                && hostVal is Dictionary<string, object?> hDict
            )
            {
                CollectMissingPatches(tDict, hDict, path, patches);
                continue;
            }

            if (templateVal is IList tList && hostVal is IList hList)
            {
                foreach (var item in tList)
                {
                    if (!hList.Cast<object?>().Any(h => ValuesEqual(h, item)))
                    {
                        patches.Add(new Patch(path, IsListItem: true, Remove: false));
                    }
                }
            }
        }
    }

    static void CollectExplicitRemovals(
        Dictionary<string, object?> host,
        IReadOnlyList<string> paths,
        List<Patch> patches
    )
    {
        foreach (var path in paths)
        {
            if (PathExists(host, path.Split('.')) && patches.All(p => p.Path != path))
            {
                patches.Add(new Patch(path, IsListItem: false, Remove: true));
            }
        }
    }

    static bool PathExists(Dictionary<string, object?> root, string[] segments)
    {
        object? current = root;
        foreach (var seg in segments)
        {
            var dict = AsDict(current);
            if (dict is null || !dict.TryGetValue(seg, out current))
            {
                return false;
            }
        }

        return true;
    }

    static void CollectObsoletePatches(
        Dictionary<string, object?> template,
        Dictionary<string, object?> host,
        string prefix,
        List<Patch> patches
    )
    {
        foreach (var (key, hostVal) in host)
        {
            var path = string.IsNullOrEmpty(prefix) ? key : $"{prefix}.{key}";
            if (!template.ContainsKey(key))
            {
                patches.Add(new Patch(path, IsListItem: false, Remove: true));
                continue;
            }

            var templateVal = template[key];
            if (
                hostVal is Dictionary<string, object?> hDict
                && templateVal is Dictionary<string, object?> tDict
            )
            {
                CollectObsoletePatches(tDict, hDict, path, patches);
                continue;
            }

            if (hostVal is IList hList && templateVal is IList tList)
            {
                foreach (var item in hList.Cast<object?>())
                {
                    if (!tList.Cast<object?>().Any(t => ValuesEqual(t, item)))
                    {
                        patches.Add(new Patch(path, IsListItem: true, Remove: true));
                    }
                }
            }
        }
    }

    static bool ValuesEqual(object? a, object? b)
    {
        a = NormalizeNode(a);
        b = NormalizeNode(b);
        if (a is null && b is null)
        {
            return true;
        }

        if (a is null || b is null)
        {
            return false;
        }

        if (a is Dictionary<string, object?> ad && b is Dictionary<string, object?> bd)
        {
            if (ad.Count != bd.Count)
            {
                return false;
            }

            foreach (var (k, av) in ad)
            {
                if (!bd.TryGetValue(k, out var bv) || !ValuesEqual(av, bv))
                {
                    return false;
                }
            }

            return true;
        }

        if (a is IList al && b is IList bl)
        {
            if (al.Count != bl.Count)
            {
                return false;
            }

            for (var i = 0; i < al.Count; i++)
            {
                if (!ValuesEqual(al[i], bl[i]))
                {
                    return false;
                }
            }

            return true;
        }

        return Equals(a, b);
    }
}
