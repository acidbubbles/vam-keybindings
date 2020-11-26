using System;
using System.Collections;
using SimpleJSON;
using UnityEngine;

public class Shortcuts : MVRScript
{
    private const float _timeoutLen = 1.0f; // http://vimdoc.sourceforge.net/htmldoc/options.html#'timeoutlen'

    private Binding _current;
    private PrefabManager _prefabManager;
    private BindingsManager _bindingsManager;
    private ActionsManager _actionsManager;
    private BindingsScreen _ui;
    private Coroutine _coroutine;
    private bool _loaded;

    public override void Init()
    {
        try
        {
            _prefabManager = new PrefabManager();
            _bindingsManager = new BindingsManager();
            _actionsManager = new ActionsManager(containingAtom, _prefabManager);
            StartCoroutine(_prefabManager.LoadUIAssets());
            SuperController.singleton.StartCoroutine(DeferredInit());
            SuperController.singleton.onAtomUIDRenameHandlers += OnAtomRename;

            // TODO: Replace by a dynamically generated list
            _actionsManager.Add("save", new DiscreteTriggerBoundAction(_prefabManager, containingAtom));
            _actionsManager.Add("print.1", new DiscreteTriggerBoundAction(_prefabManager, containingAtom));
            _actionsManager.Add("print.2", new DiscreteTriggerBoundAction(_prefabManager, containingAtom));
            _actionsManager.Add("print.3", new DiscreteTriggerBoundAction(_prefabManager, containingAtom));
            _actionsManager.Add("print.3.4", new DebugBoundAction("debug 3.4"));
            _actionsManager.Add("print.3.5", new DebugBoundAction("debug 3.5"));

            // TODO: Build from maps, e.g. ^s or :w
            _bindingsManager.Add(new Binding
            {
                modifier = KeyCode.LeftControl,
                key = KeyCode.S,
                action = "save"
            });
            _bindingsManager.Add(new Binding
            {
                key = KeyCode.Alpha1,
                action = "print.1"
            });
            _bindingsManager.Add(new Binding
            {
                key = KeyCode.Alpha2,
                action = "print.2"
            });
            var b3 = _bindingsManager.Add(new Binding
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
            SuperController.LogError($"{nameof(Shortcuts)}.{nameof(Init)}: {e}");
        }
    }

    private IEnumerator DeferredInit()
    {
        yield return new WaitForEndOfFrame();
        if (this == null) yield break;
        if (!_loaded) containingAtom.RestoreFromLast(this);
    }

    public override void InitUI()
    {
        base.InitUI();
        if (UITransform != null)
        {
            _prefabManager.triggerActionsParent = UITransform;
            var scriptUI = UITransform.GetComponentInChildren<MVRScriptUI>();

            var go = new GameObject();
            go.transform.SetParent(scriptUI.fullWidthUIContent);
            var active = go.activeInHierarchy;
            if (active) go.SetActive(false);
            _ui = go.AddComponent<BindingsScreen>();
            _ui.bindingsManager = _bindingsManager;
            _ui.prefabManager = _prefabManager;
            if (active) go.SetActive(true);
        }
    }

    public void Update()
    {
        try
        {
            if (!Input.anyKeyDown) return;
            if (LookInputModule.singleton.inputFieldActive && !Input.GetKey(KeyCode.LeftControl)) return;

            if (_coroutine != null)
                StopCoroutine(_coroutine);

            var next = (_current ?? _bindingsManager.rootBinding).DoMatch();

            if (next == null)
            {
                if (_current != _bindingsManager.rootBinding)
                    next = _bindingsManager.rootBinding.DoMatch();
                _current = null;
                if (next == null)
                    return;
            }

            if (next.Count == 0)
            {
                if (next.action != null)
                    _actionsManager.Execute(next.action);
                _current = null;
                return;
            }

            _current = next;
            _coroutine = StartCoroutine(TimeoutCoroutine());
        }
        catch (Exception e)
        {
            SuperController.LogError($"{nameof(Shortcuts)}.{nameof(Update)}: {e}");
        }
    }

    private IEnumerator TimeoutCoroutine()
    {
        yield return new WaitForSecondsRealtime(_timeoutLen);
        if (_current == null) yield break;
        try
        {
            if (_current.action != null)
            {
                _actionsManager.Execute(_current.action);
                _current = _bindingsManager.rootBinding;
            }

            _coroutine = null;
        }
        catch (Exception e)
        {
            SuperController.LogError($"{nameof(Shortcuts)}.{nameof(TimeoutCoroutine)}: {e}");
        }
    }

    public override void Validate()
    {
        base.Validate();
        _actionsManager.Validate();
    }

    public void OnAtomRename(string oldid, string newid)
    {
        _actionsManager.SyncAtomNames();
    }

    public override JSONClass GetJSON(bool includePhysical = true, bool includeAppearance = true,
        bool forceStore = false)
    {
        var json = base.GetJSON(includePhysical, includeAppearance, forceStore);

        try
        {
            json["actions"] = _actionsManager.GetJSON();
            json["bindings"] = _bindingsManager.GetJSON();
            needsStore = true;
        }
        catch (Exception exc)
        {
            SuperController.LogError($"{nameof(Shortcuts)}.{nameof(GetJSON)} (Serialize): {exc}");
        }

        return json;
    }

    public override void RestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true,
        JSONArray presetAtoms = null, bool setMissingToDefault = true)
    {
        base.RestoreFromJSON(jc, restorePhysical, restoreAppearance, presetAtoms, setMissingToDefault);

        try
        {
            _loaded = true;
            _actionsManager.RestoreFromJSON(jc["actions"]?.AsObject);
            _bindingsManager.RestoreFromJSON(jc["bindings"]?.AsObject);
        }
        catch (Exception exc)
        {
            SuperController.LogError($"{nameof(Shortcuts)}.{nameof(RestoreFromJSON)}: {exc}");
        }
    }

    public void OnDestroy()
    {
        if (SuperController.singleton != null) SuperController.singleton.onAtomUIDRenameHandlers -= OnAtomRename;
    }
}
