using System;
using System.Collections.Generic;
using UnityEngine;

public class VimVam : MVRScript
{
    private class Binding : List<KeyValuePair<KeyCode, Binding>>
    {
        public string action;

        public void Add(KeyCode key, Binding binding)
        {
            Add(new KeyValuePair<KeyCode, Binding>(key, binding));
        }
    };
    private Binding _rootBindings = new Binding();
    private Binding _current;
    private readonly Queue<KeyCode> _keysToProcess = new Queue<KeyCode>();

    public override void Init()
    {
        try
        {
            _rootBindings = new Binding { action = null };
            _rootBindings.Add(KeyCode.Alpha1, new Binding
            {
                action = "print.1"
            });
            _rootBindings.Add(KeyCode.Alpha2, new Binding
            {
                action = "print.2"
            });
        }
        catch (Exception e)
        {
            SuperController.LogError($"{nameof(VimVam)}.{nameof(Init)}: {e}");
        }
    }

    public void Update()
    {
        try
        {
            if (Input.anyKeyDown)
                _current = Process(_current ?? _rootBindings);
        }
        catch (Exception e)
        {
            SuperController.LogError($"{nameof(VimVam)}.{nameof(Update)}: {e}");
        }
    }

    private static Binding Process(Binding current)
    {
        for (var i = 0; i < current.Count; i++)
        {
            var binding = current[i];
            if (!Input.GetKeyDown(binding.Key)) continue;
            return binding.Value;
        }
        if (current.action != null)
        {
            SuperController.LogMessage($"{current.action}");
        }
        return null;
    }
}

public class Dictionary<T>
{
}
