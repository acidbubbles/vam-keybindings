using System.Collections.Generic;
using UnityEngine;

public class KeyMapTreeNode
{
    public KeyChord keyChord;
    public string boundCommandName;
    public readonly List<KeyMapTreeNode> next = new List<KeyMapTreeNode>();

    public bool TryGet(KeyChord source, out KeyMapTreeNode result)
    {
        for (var i = 0; i < next.Count; i++)
        {
            if (!next[i].keyChord.Equals(source)) continue;
            result = next[i];
            return true;
        }

        result = null;
        return false;
    }

    public override string ToString()
    {
        return $"treemap {keyChord} {boundCommandName}";
    }
};
