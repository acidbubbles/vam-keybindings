using System;
using SimpleJSON;
using UnityEngine;

public struct KeyChord
{
    public static readonly KeyChord empty = new KeyChord(KeyCode.None);

    public readonly KeyCode modifier;
    public readonly KeyCode key;

    public KeyChord(KeyCode key, KeyCode modifier = KeyCode.None)
    {
        this.key = key;
        this.modifier = modifier;
    }

    public override string ToString()
    {
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (modifier)
        {
            case KeyCode.LeftControl:
                return $"Ctrl+{key}";
            case KeyCode.None:
                return $"{key}";
            default:
                return $"?+{key}";
        }
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is KeyChord && Equals((KeyChord) obj);
    }

    private bool Equals(KeyChord other)
    {
        return modifier == other.modifier && key == other.key;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return ((int) modifier * 397) ^ (int) key;
        }
    }

    public JSONNode GetJSON()
    {
        return new JSONClass
        {
            {"key", key.ToString()},
            {"modifier", modifier.ToString()}
        };
    }

    public static KeyChord FromJSON(JSONNode jsonNode)
    {
        return new KeyChord(
            (KeyCode) Enum.Parse(typeof(KeyCode), jsonNode["key"].Value),
            (KeyCode) Enum.Parse(typeof(KeyCode), jsonNode["modifier"].Value)
        );
    }
}
