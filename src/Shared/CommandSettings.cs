using System.Collections.Generic;

public static class CommandSettings
{
    public const string NamespaceKey = "Namespace";

    public static IEnumerable<KeyValuePair<string, string>> Create(string ns)
    {
        return new[]
        {
            new KeyValuePair<string, string>(NamespaceKey, ns)
        };
    }
}
