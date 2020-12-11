using System.Collections.Generic;
using System.Text;

public static class KeyChordExtensions
{
    private static readonly StringBuilder _sb = new StringBuilder();

    public static string GetKeyChordsAsString(this IEnumerable<KeyChord> chords)
    {
        _sb.Length = 0;
        using (var enumerator = chords.GetEnumerator())
        {
            if (!enumerator.MoveNext())
                return "";

            _sb.Append(enumerator.Current.ToString());

            while (enumerator.MoveNext())
            {
                _sb.Append(" ");
                _sb.Append(enumerator.Current.ToString());
            }
        }

        return _sb.ToString();
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
