static class TacosRepoRoot
{
    internal static string Find(string start)
    {
        var dir = new DirectoryInfo(Path.GetFullPath(start));
        while (dir is not null)
        {
            if (
                File.Exists(Path.Combine(dir.FullName, "AGENTS.md"))
                && Directory.Exists(Path.Combine(dir.FullName, ".agents", "skills", "tacos-doctor"))
            )
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException("tacos repo root not found");
    }
}

partial class Program
{
    static string FindRepoRoot(string start) => TacosRepoRoot.Find(start);
}
