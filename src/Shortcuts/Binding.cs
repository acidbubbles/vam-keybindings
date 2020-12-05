using System;
using SimpleJSON;
using UnityEngine;

public struct Binding
{
    public static readonly Binding empty = new Binding(KeyCode.None, KeyCode.None);

    public readonly KeyCode modifier;
    public readonly KeyCode key;

    public Binding(KeyCode key, KeyCode modifier = KeyCode.None)
    {
        this.key = key;
        this.modifier = modifier;
    }

    public override string ToString()
    {
        return modifier == KeyCode.None ? $"{key}" : $"{modifier}+{key}";
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is Binding && Equals((Binding) obj);
    }

    private bool Equals(Binding other)
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

    public static Binding FromJSON(JSONNode jsonNode)
    {
        return new Binding(
            (KeyCode) Enum.Parse(typeof(KeyCode), jsonNode["key"].Value),
            (KeyCode) Enum.Parse(typeof(KeyCode), jsonNode["modifier"].Value)
        );
    }
}
