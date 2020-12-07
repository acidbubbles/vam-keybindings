using System;
using System.Collections;
using System.Linq;
using SimpleJSON;
using UnityEngine;

public class ShortcutsPlugin : MVRScript, IActionsInvoker
{
    private PrefabManager _prefabManager;
    private KeyMapManager _keyMapManager;
    private RemoteActionsManager _remoteActionsManager;
    private ShortcutsScreen _ui;
    private Coroutine _timeoutCoroutine;
    private KeyMapTreeNode _current;
    private bool _loaded;

    public override void Init()
    {
        _prefabManager = new PrefabManager();
        _keyMapManager = new KeyMapManager();
        _remoteActionsManager = new RemoteActionsManager();
        SuperController.singleton.StartCoroutine(_prefabManager.LoadUIAssets());
        SuperController.singleton.StartCoroutine(DeferredInit());

        AcquireAllAvailableBroadcastingPlugins();
    }

    private IEnumerator DeferredInit()
    {
        yield return new WaitForEndOfFrame();
        if (this == null) yield break;
        if (!_loaded) containingAtom.RestoreFromLast(this);

        // TODO: Remove this later, replace by levels (defaults, session, scene, atom)
        _keyMapManager.RestoreDefaults();
        // _bindingsManager.Debug(_bindingsManager.root);
    }

    public override void InitUI()
    {
        base.InitUI();
        if (UITransform == null) return;
        _prefabManager.triggerActionsParent = UITransform;

        var scriptUI = UITransform.GetComponentInChildren<MVRScriptUI>();

        var go = new GameObject();
        go.transform.SetParent(scriptUI.fullWidthUIContent, false);

        var active = go.activeInHierarchy;
        if (active) go.SetActive(false);
        _ui = go.AddComponent<ShortcutsScreen>();
        _ui.prefabManager = _prefabManager;
        _ui.bindingsManager = _keyMapManager;
        _ui.remoteActionsManager = _remoteActionsManager;
        _ui.Configure();
        if (active) go.SetActive(true);
    }

    public void AcquireAllAvailableBroadcastingPlugins()
    {
        foreach (var atom in SuperController.singleton.GetAtoms())
        {
            foreach (var storable in atom.GetStorableIDs()
                .Select(id => atom.GetStorableByID(id))
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
            // Don't waste resources
            if (!Input.anyKeyDown) return;

            // Do not listen while a keybinding is being recorded
            if (_ui.isRecording) return;

            // <C-*> shortcuts can work even in a text field, otherwise text fields have preference
            if (LookInputModule.singleton.inputFieldActive && !Input.GetKey(KeyCode.LeftControl)) return;

            if (_timeoutCoroutine != null)
                StopCoroutine(_timeoutCoroutine);

            var current = _current;
            _current = null;
            var next = current?.DoMatch();

            if (next == null)
            {
                next = _keyMapManager.root.DoMatch();
                if (next == null)
                    return;
            }

            if (next.next.Count == 0)
            {
                if (next.action != null)
                    _remoteActionsManager.Invoke(next.action);
                return;
            }

            _current = next;
            _timeoutCoroutine = StartCoroutine(TimeoutCoroutine());
        }
        catch (Exception e)
        {
            SuperController.LogError($"{nameof(ShortcutsPlugin)}.{nameof(Update)}: {e}");
        }
    }

    private IEnumerator TimeoutCoroutine()
    {
        yield return new WaitForSecondsRealtime(Settings.TimeoutLen);
        if (_current == null) yield break;
        try
        {
            if (_current.action != null)
            {
                _remoteActionsManager.Invoke(_current.action);
                _current = _keyMapManager.root;
            }
            _timeoutCoroutine = null;
        }
        catch (Exception e)
        {
            SuperController.LogError($"{nameof(ShortcutsPlugin)}.{nameof(TimeoutCoroutine)}: {e}");
        }
    }

    public override JSONClass GetJSON(bool includePhysical = true, bool includeAppearance = true, bool forceStore = false)
    {
        var json = base.GetJSON(includePhysical, includeAppearance, forceStore);

        try
        {
            json["maps"] = _keyMapManager.GetJSON();
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
            _keyMapManager.RestoreFromJSON(jc["maps"]);
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

    public void OnActionsProviderDestroyed(JSONStorable storable)
    {
        _remoteActionsManager.Remove(storable);
    }
}
