using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VimVam : MVRScript
{
    public const float TimeoutLen = 1.0f; // http://vimdoc.sourceforge.net/htmldoc/options.html#'timeoutlen'

    private class Binding : List<KeyValuePair<KeyCode, Binding>>
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

    private Binding _rootBindings = new Binding();
    private Binding _current;
    private Coroutine _coroutine;
    private readonly Queue<Binding> _keysToProcess = new Queue<Binding>();

    public override void Init()
    {
        try
        {
            _rootBindings = new Binding { action = null };
            _rootBindings.Add(new Binding
            {
                key = KeyCode.Alpha1,
                action = "print.1"
            });
            _rootBindings.Add(new Binding
            {
                key = KeyCode.Alpha2,
                action = "print.2"
            });
            var b3 = _rootBindings.Add(new Binding
            {
                key = KeyCode.Alpha3,
                action = "print.3"
            });
            b3.Add(new Binding
            {
                key = KeyCode.Alpha4,
                action = "print.3.4"
            });
            b3.Add(new Binding
            {
                key = KeyCode.Alpha5,
                action = "print.3.5"
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
            {
                if (_coroutine != null)
                    StopCoroutine(_coroutine);

                var next = (_current ?? _rootBindings).FromInput();

                if (next == null)
                {
                    if (_current != _rootBindings)
                        next = _rootBindings.FromInput();
                    _current = null;
                    if (next == null)
                        return;
                }

                if (next.Count == 0)
                {
                    if (next.action != null)
                        Execute(next);
                    _current = null;
                    return;
                }
                else
                {
                    _current = next;
                    _coroutine = StartCoroutine(TimeoutCoroutine());
                }
            }
        }
        catch (Exception e)
        {
            SuperController.LogError($"{nameof(VimVam)}.{nameof(Update)}: {e}");
        }
    }

    private IEnumerator TimeoutCoroutine()
    {
        SuperController.LogMessage($"Waiting...");
        yield return new WaitForSecondsRealtime(TimeoutLen);
        if (_current == null) yield break;
        try
        {
            if (_current.action != null)
            {
                Execute(_current);
                _current = _rootBindings;
            }
            _coroutine = null;
        }
        catch (Exception e)
        {
            SuperController.LogError($"{nameof(VimVam)}.{nameof(TimeoutCoroutine)}: {e}");
        }
    }

    private static void Execute(Binding current)
    {
        SuperController.LogMessage($"{current.action}");
    }
}
