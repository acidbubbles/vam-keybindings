using System.Collections.Generic;
using UnityEngine;

public class Binding : List<KeyValuePair<KeyCode, Binding>>
{
    public KeyCode modifier;
    public KeyCode key;
    public string action;

    public Binding Add(Binding binding)
    {
        Add(new KeyValuePair<KeyCode, Binding>(binding.key, binding));
        return binding;
    }

    public Binding DoMatch()
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
