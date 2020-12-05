using System.Collections.Generic;
using UnityEngine;

public class BindingTreeNode : List<KeyValuePair<KeyCode, BindingTreeNode>>
{
    public KeyCode modifier;
    public KeyCode key;
    public string action;

    public BindingTreeNode Add(BindingTreeNode bindingTreeNode)
    {
        Add(new KeyValuePair<KeyCode, BindingTreeNode>(bindingTreeNode.key, bindingTreeNode));
        return bindingTreeNode;
    }

    public BindingTreeNode DoMatch()
    {
        for (var i = 0; i < Count; i++)
        {
            var binding = this[i];
            if (binding.Value.IsMatch())
                return binding.Value;
        }

        return null;
    }

    public bool IsMatch()
    {
        if (!Input.GetKeyDown(key)) return false;
        if (modifier != KeyCode.None && !Input.GetKey(modifier)) return false;
        return true;
    }
};
