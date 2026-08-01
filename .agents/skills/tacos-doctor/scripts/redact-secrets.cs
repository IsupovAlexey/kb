using System.Text.RegularExpressions;

internal static class SecretRedaction
{
    private static readonly Regex[] SecretPatterns =
    [
        new(@"sk-[A-Za-z0-9]{10,}", RegexOptions.Compiled),
        new(@"sk_(?:live|test)_[A-Za-z0-9]{10,}", RegexOptions.Compiled),
        new(@"AKIA[0-9A-Z]{16}", RegexOptions.Compiled),
        new(@"Bearer\s+eyJ[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+", RegexOptions.Compiled),
        new(@"Bearer\s+(?!eyJ)[A-Za-z0-9._~+/=-]{16,}", RegexOptions.Compiled),
        new(@"ghp_[A-Za-z0-9]{20,}", RegexOptions.Compiled),
        new(@"github_pat_[A-Za-z0-9_]{20,}", RegexOptions.Compiled),
        new(@"xox[bpar]-[0-9A-Za-z-]{10,}", RegexOptions.Compiled),
        new(@"xapp-[0-9A-Za-z-]{10,}", RegexOptions.Compiled),
        new(
            @"(?i)(?:api[_-]?key|secret[_-]?key|access[_-]?token)\s*[=:]\s*['""]?[A-Za-z0-9_+/=-]{20,}['""]?",
            RegexOptions.Compiled
        ),
        new(@"(?i)(password|pwd)\s*[=:]\s*\S+", RegexOptions.Compiled),
        new(@"mongodb(\+srv)?://[^\s""']+", RegexOptions.Compiled),
        new(@"(?i)(?:postgres(?:ql)?|mysql|redis|amqp):\/\/[^\s""']+", RegexOptions.Compiled),
        new(@"(?i)Server=[^;]+;[^\n]*Password=[^;""'\s]+", RegexOptions.Compiled),
        new(
            @"-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----.*?-----END (?:RSA |EC |OPENSSH )?PRIVATE KEY-----",
            RegexOptions.Compiled | RegexOptions.Singleline
        ),
    ];

    public static string RedactSecrets(string content)
    {
        foreach (var pattern in SecretPatterns)
        {
            content = pattern.Replace(content, "[REDACTED]");
        }

        return content;
    }
}
