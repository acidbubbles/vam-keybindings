using System;
using System.Collections;
using System.Linq;
using SimpleJSON;
using UnityEngine;

public class ShortcutsPlugin : MVRScript, IActionsInvoker
{
    private const float _timeoutLen = 1.0f; // http://vimdoc.sourceforge.net/htmldoc/options.html#'timeoutlen'

    private Binding _current;
    private PrefabManager _prefabManager;
    private BindingsManager _bindingsManager;
    private RemoteActionsManager _remoteActionsManager;
    private ShortcutsScreen _ui;
    private Coroutine _coroutine;
    private bool _loaded;

    public override void Init()
    {
        _prefabManager = new PrefabManager();
        _bindingsManager = new BindingsManager();
        _remoteActionsManager = new RemoteActionsManager();
        SuperController.singleton.StartCoroutine(_prefabManager.LoadUIAssets());
        SuperController.singleton.StartCoroutine(DeferredInit());

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

        AcquireAllAvailableBroadcastingPlugins();
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
            _ui = go.AddComponent<ShortcutsScreen>();
            _ui.prefabManager = _prefabManager;
            _ui.bindingsManager = _bindingsManager;
            _ui.remoteActionsManager = _remoteActionsManager;
            if (active) go.SetActive(true);
        }
    }

    public void AcquireAllAvailableBroadcastingPlugins()
    {
        foreach (var atom in SuperController.singleton.GetAtoms())
        {
            foreach (var storable in atom.GetStorableIDs().Select(id => atom.GetStorableByID(id))
                .Where(s => s is MVRScript))
            {
                _remoteActionsManager.TryRegister(storable);
            }
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
                    Execute(next.action);
                _current = null;
                return;
            }

            _current = next;
            _coroutine = StartCoroutine(TimeoutCoroutine());
        }
        catch (Exception e)
        {
            SuperController.LogError($"{nameof(ShortcutsPlugin)}.{nameof(Update)}: {e}");
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
                Execute(_current.action);
                _current = _bindingsManager.rootBinding;
            }

            _coroutine = null;
        }
        catch (Exception e)
        {
            SuperController.LogError($"{nameof(ShortcutsPlugin)}.{nameof(TimeoutCoroutine)}: {e}");
        }
    }

    private void Execute(string action)
    {
        // TODO: Find the matching action and execute it (build and maintain a list)
        SuperController.LogMessage(
            $"Shortcut: Action '{action}' does not exist. Maybe it was assigned to a destroyed atom storable.");
    }

    public override JSONClass GetJSON(bool includePhysical = true, bool includeAppearance = true,
        bool forceStore = false)
    {
        var json = base.GetJSON(includePhysical, includeAppearance, forceStore);

        try
        {
            json["bindings"] = _bindingsManager.GetJSON();
            needsStore = true;
        }
        catch (Exception exc)
        {
            SuperController.LogError($"{nameof(ShortcutsPlugin)}.{nameof(GetJSON)} (Serialize): {exc}");
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
            _bindingsManager.RestoreFromJSON(jc["bindings"]?.AsObject);
        }
        catch (Exception exc)
        {
            SuperController.LogError($"{nameof(ShortcutsPlugin)}.{nameof(RestoreFromJSON)}: {exc}");
        }
    }

    public void OnActionsProviderAvailable(JSONStorable storable)
    {
        _remoteActionsManager.TryRegister(storable);
    }
}
