using System.Text;
using System.Text.RegularExpressions;

static partial class HostAgentInstall
{
    const string ModelPlaceholder = "{{TACOS_MODEL}}";
    const string SkillsPrefixPlaceholder = "{{SKILLS_PREFIX}}";
    static readonly string TacosYamlRelative = Path.Combine("openspec", "tacos.yaml");
    static readonly Regex FrontmatterNameRegex = new(
        @"^name:\s*(.+)\s*$",
        RegexOptions.Multiline | RegexOptions.Compiled
    );
    static readonly Regex FrontmatterModelRegex = new(
        @"^model:\s*(.+)\s*$",
        RegexOptions.Multiline | RegexOptions.Compiled
    );

    static readonly Dictionary<string, string[]> AgentModelYamlPaths = new(
        StringComparer.OrdinalIgnoreCase
    )
    {
        ["agent-tacos-grill-gather.md"] = ["grill", "gather_models"],
        ["agent-tacos-grill-summarize.md"] = ["grill", "summarize_models"],
        ["agent-tacos-spec-review.md"] = ["review", "spec_review_models"],
        ["agent-tacos-apply-review.md"] = ["review", "apply_review_models"],
        ["agent-tacos-e2e-scenarios.md"] = ["orchestration", "e2e_models"],
        ["agent-tacos-test-plans.md"] = ["orchestration", "test_plans_models"],
        ["agent-tacos-test-plan-review.md"] = ["orchestration", "test_plans_models"],
        ["agent-tacos-spec-grounding.md"] = ["orchestration", "spec_grounding_models"],
        ["agent-tacos-audit-explore.md"] = ["orchestration", "audit_explore_models"],
        ["agent-tacos-audit-executor.md"] = ["orchestration", "audit_executor_models"],
        ["agent-tacos-apply-implement.md"] = ["orchestration", "apply_implement_models"],
        ["agent-tacos-orchestrator-fixes.md"] = ["orchestration", "orchestrator_fixes_models"],
        ["agent-tacos-gate-runner.md"] = ["orchestration", "gate_runner_models"],
        ["agent-tacos-dedrift-detect.md"] = ["orchestration", "dedrift_detect_models"],
        ["agent-tacos-jira-regenerate.md"] = ["jira", "generate_desc_models"],
        ["agent-tacos-additional-spec-review.md"] = ["review", "additional_spec_review_models"],
        ["agent-tacos-additional-apply-review.md"] = ["review", "additional_apply_review_models"],
    };

    static readonly Dictionary<string, Dictionary<string, string>> DefaultModels = new(
        StringComparer.OrdinalIgnoreCase
    )
    {
        ["agent-tacos-grill-gather.md"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["cursor"] = "inherit",
            ["claude"] = "haiku",
        },
        ["agent-tacos-grill-summarize.md"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["cursor"] = "inherit",
            ["claude"] = "sonnet",
        },
        ["agent-tacos-spec-review.md"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["cursor"] = "inherit",
            ["claude"] = "sonnet",
        },
        ["agent-tacos-apply-review.md"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["cursor"] = "inherit",
            ["claude"] = "sonnet",
        },
        ["agent-tacos-e2e-scenarios.md"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["cursor"] = "inherit",
            ["claude"] = "sonnet",
        },
        ["agent-tacos-test-plans.md"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["cursor"] = "inherit",
            ["claude"] = "sonnet",
        },
        ["agent-tacos-test-plan-review.md"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["cursor"] = "inherit",
            ["claude"] = "sonnet",
        },
        ["agent-tacos-spec-grounding.md"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["cursor"] = "inherit",
            ["claude"] = "sonnet",
        },
        ["agent-tacos-audit-explore.md"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["cursor"] = "inherit",
            ["claude"] = "haiku",
        },
        ["agent-tacos-audit-executor.md"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["cursor"] = "inherit",
            ["claude"] = "sonnet",
        },
        ["agent-tacos-apply-implement.md"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["cursor"] = "inherit",
            ["claude"] = "sonnet",
        },
        ["agent-tacos-orchestrator-fixes.md"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["cursor"] = "inherit",
            ["claude"] = "sonnet",
        },
        ["agent-tacos-gate-runner.md"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["cursor"] = "inherit",
            ["claude"] = "haiku",
        },
        ["agent-tacos-dedrift-detect.md"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["cursor"] = "inherit",
            ["claude"] = "haiku",
        },
        ["agent-tacos-jira-regenerate.md"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["cursor"] = "inherit",
            ["claude"] = "sonnet",
        },
        ["agent-tacos-additional-spec-review.md"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["cursor"] = "inherit",
            ["claude"] = "sonnet",
        },
        ["agent-tacos-additional-apply-review.md"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["cursor"] = "inherit",
            ["claude"] = "sonnet",
        },
    };

    static readonly string[] RequiredHostFlavors = ["cursor", "claude"];

    public readonly record struct DiagnosticLine(bool IsFailure, bool IsWarning, string Message);

    public static IReadOnlyList<string> ValidateBundleTemplates(string skillRoot)
    {
        var errors = new List<string>();
        var templatesDir = Path.Combine(skillRoot, "templates", "agents");
        if (!Directory.Exists(templatesDir))
        {
            errors.Add("missing templates/agents directory");
            return errors;
        }

        foreach (var (fileName, _) in AgentModelYamlPaths)
        {
            var path = Path.Combine(templatesDir, fileName);
            if (!File.Exists(path))
            {
                errors.Add($"missing template {fileName}");
                continue;
            }

            var text = File.ReadAllText(path, Encoding.UTF8);
            if (!text.Contains(ModelPlaceholder, StringComparison.Ordinal))
            {
                errors.Add($"template {fileName} missing {ModelPlaceholder}");
            }

            if (
                text.Contains("Read `", StringComparison.Ordinal)
                && !text.Contains(SkillsPrefixPlaceholder, StringComparison.Ordinal)
                && (
                    text.Contains(".agents/skills", StringComparison.Ordinal)
                    || text.Contains("../tacos-", StringComparison.Ordinal)
                )
            )
            {
                errors.Add(
                    $"template {fileName} hardcodes skill paths — use {SkillsPrefixPlaceholder}/tacos-<name>/…"
                );
            }

            if (!text.TrimStart().StartsWith("---", StringComparison.Ordinal))
            {
                errors.Add($"template {fileName} missing YAML frontmatter");
            }
        }

        foreach (
            var extra in Directory.EnumerateFiles(
                templatesDir,
                "*.md",
                SearchOption.TopDirectoryOnly
            )
        )
        {
            var name = Path.GetFileName(extra);
            if (name.Equals("README.md", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!AgentModelYamlPaths.ContainsKey(name))
            {
                errors.Add($"unexpected template {name} (not in agent registry)");
            }
        }

        return errors;
    }
}
