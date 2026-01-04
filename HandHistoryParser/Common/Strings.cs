namespace HandHistoryParser.Common;

public static class 
Strings {
    
    public static string[]
    GetLines(this string @string) =>
        @string.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

    public static IEnumerable<string>
    SplitByEmptyLines(this IEnumerable<string> lines) {
        var reader = lines.ToList().ToLineReader();
        var current = string.Empty;
        foreach (var line in lines) {
            if (line.IsNullOrWhiteSpace())
                if (current.IsNullOrWhiteSpace())
                    continue;
                else {
                    yield return current;
                    current = string.Empty;
                }
            else
                current = current.AppendLine(line);
        }
        if (!current.IsNullOrWhiteSpace())
            yield return current;
    }

    public static bool
    IsNullOrWhiteSpace(this string? @string) =>
        string.IsNullOrWhiteSpace(@string);

    public static string
    AppendLine(this string @string, string line) =>
        string.IsNullOrEmpty(@string) ? line : $"{@string}\n{line}";

    public static bool
    EqualsIgnoreCase(this string @string, string other) =>
        string.Equals(@string, other, StringComparison.OrdinalIgnoreCase);
}