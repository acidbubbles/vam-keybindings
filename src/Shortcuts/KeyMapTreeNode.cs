using System.Collections.Generic;
using UnityEngine;

public class KeyMapTreeNode
{
    public KeyChord keyChord;
    public string action;
    public readonly List<KeyMapTreeNode> next = new List<KeyMapTreeNode>();


    public KeyMapTreeNode DoMatch()
    {
        for (var i = 0; i < next.Count; i++)
        {
            var child = next[i];
            if (child.IsMatch())
                return child;
        }

        return null;
    }

    public bool IsMatch()
    {
        if (!Input.GetKeyDown(keyChord.key)) return false;
        // TODO: Handle Shift+Alt+Ctrl
        if (keyChord.modifier != KeyCode.None && !Input.GetKey(keyChord.modifier)) return false;
        return true;
    }

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
        return $"treemap {keyChord} {action}";
    }
};
