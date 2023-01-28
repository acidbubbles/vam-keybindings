using System.Collections.Generic;

public class KeyMapTreeNode
{
    public KeyChord keyChord;
    public KeyMap map;
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
}
