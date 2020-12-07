using System.Collections.Generic;
using System.Linq;

public static class BindingExtensions
{
    public static string GetBindingsAsString(this IEnumerable<KeyChord> bindings)
    {
        return string.Join(", ", bindings.Select(b => b.ToString()).ToArray());
    }

    public static bool SameBinding(this KeyChord[] bindings, KeyChord[] other)
    {
        if (other.Length != bindings.Length) return false;
        for (var i = 0; i < bindings.Length; i++)
        {
            if (!other[i].Equals(bindings[i])) return false;
        }
        return true;
    }
}
