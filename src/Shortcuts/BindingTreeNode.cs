using System.Collections.Generic;
using UnityEngine;

public class BindingTreeNode
{
    public Binding binding;
    public string action;
    public readonly List<BindingTreeNode> next = new List<BindingTreeNode>();


    public BindingTreeNode DoMatch()
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
        if (!Input.GetKeyDown(binding.key)) return false;
        // TODO: Handle Shift+Alt+Ctrl
        if (binding.modifier != KeyCode.None && !Input.GetKey(binding.modifier)) return false;
        return true;
    }

    public bool TryGet(Binding source, out BindingTreeNode result)
    {
        for (var i = 0; i < next.Count; i++)
        {
            if (!next[i].binding.Equals(source)) continue;
            result = next[i];
            return true;
        }

        result = null;
        return false;
    }

    public override string ToString()
    {
        return $"treemap {binding} {action}";
    }
};
