using System.Collections.Generic;
using UnityEngine;

public class Binding : List<KeyValuePair<KeyCode, Binding>>
{
    public KeyCode key;
    public string action;

    public Binding Add(Binding binding)
    {
        Add(new KeyValuePair<KeyCode, Binding>(binding.key, binding));
        return binding;
    }

    public Binding FromInput()
    {
        for (var i = 0; i < Count; i++)
        {
            var binding = this[i];
            if (Input.GetKeyDown(binding.Key)) return binding.Value;
        }

        return null;
    }
};
