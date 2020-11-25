using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VimVam : MVRScript
{
    private const float _timeoutLen = 1.0f; // http://vimdoc.sourceforge.net/htmldoc/options.html#'timeoutlen'

    private Binding _rootBindings = new Binding();
    private Binding _current;
    private Coroutine _coroutine;
    private readonly Dictionary<string, IBoundAction> _actions = new Dictionary<string, IBoundAction>();
    private PrefabManager _prefabManager;

    public override void Init()
    {
        try
        {
            _prefabManager = new PrefabManager();
            StartCoroutine(_prefabManager.LoadUIAssets());
            SuperController.singleton.onAtomUIDRenameHandlers += OnAtomRename;

            _actions.Add("print.1", new DiscreteTriggerBoundAction(_prefabManager));
            _actions.Add("print.2", new PrintBoundAction(() => "print.2"));

            _rootBindings = new Binding {action = null};
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

            CreateButton("Edit print.1").button.onClick.AddListener(() => { _actions["print.1"].Edit(); });
        }
        catch (Exception e)
        {
            SuperController.LogError($"{nameof(VimVam)}.{nameof(Init)}: {e}");
        }
    }

    public override void InitUI()
    {
        base.InitUI();
        if (UITransform != null)
        {
            _prefabManager.triggerActionsParent = UITransform;
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

                _current = next;
                _coroutine = StartCoroutine(TimeoutCoroutine());
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
        yield return new WaitForSecondsRealtime(_timeoutLen);
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

    public override void Validate()
    {
        base.Validate();
        foreach (var kvp in _actions)
            kvp.Value.Validate();
    }

    public void OnAtomRename(string oldid, string newid)
    {
        foreach (var kvp in _actions)
            kvp.Value.SyncAtomNames();
    }

    public void OnDestroy()
    {
        if (SuperController.singleton != null)
        {
            SuperController.singleton.onAtomUIDRenameHandlers -= OnAtomRename;
        }
    }

    private void Execute(Binding current)
    {
        IBoundAction boundAction;
        if (!_actions.TryGetValue(current.action, out boundAction))
        {
            SuperController.LogError(
                $"Binding was mapped to {current.action} but there was no action matching this name available.");
            return;
        }

        boundAction.Invoke();
    }
}

public interface IBoundAction
{
    void Validate();
    void SyncAtomNames();
    void Invoke();
    void Edit();
}
