using System;
using System.Collections;
using System.Collections.Generic;
using CustomActions;
using SimpleJSON;
using UnityEngine;

public class CustomCommands : MVRScript, ICommandsProvider
{
    private CustomCommandsRepository _customCommands;
    private PrefabManager _prefabManager;
    private CustomCommandsScreen _ui;
    private ParameterizedTriggers _parameterizedTriggers;
    private bool _loaded;

    public override void Init()
    {
        _prefabManager = new PrefabManager();
        _customCommands = new CustomCommandsRepository(containingAtom, _prefabManager);
        _customCommands.onChange.AddListener(OnActionsChanged);
        _parameterizedTriggers = new ParameterizedTriggers(this);
        SuperController.singleton.StartCoroutine(_prefabManager.LoadUIAssets());
        SuperController.singleton.StartCoroutine(DeferredInit());
        SuperController.singleton.onAtomUIDRenameHandlers += OnAtomRename;

        _parameterizedTriggers.Init();
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
        if (UITransform == null) return;
        _prefabManager.triggerActionsParent = UITransform;

        var scriptUI = UITransform.GetComponentInChildren<MVRScriptUI>();

        var go = new GameObject();
        go.transform.SetParent(scriptUI.fullWidthUIContent, false);

        var active = go.activeInHierarchy;
        if (active) go.SetActive(false);
        _ui = go.AddComponent<CustomCommandsScreen>();
        _ui.prefabManager = _prefabManager;
        _ui.customCommands = _customCommands;
        _ui.Configure();
        if (active) go.SetActive(true);
    }


    public override void Validate()
    {
        _customCommands.Validate();
    }

    public void OnAtomRename(string oldid, string newid)
    {
        _customCommands.SyncAtomNames();
    }

    public override JSONClass GetJSON(bool includePhysical = true, bool includeAppearance = true, bool forceStore = false)
    {
        var json = base.GetJSON(includePhysical, includeAppearance, forceStore);

        try
        {
            json["actions"] = _customCommands.GetJSON();
            needsStore = true;
        }
        catch (Exception exc)
        {
            SuperController.LogError($"Keybindings: {nameof(CustomActions)}.{nameof(GetJSON)}: {exc}");
        }

        return json;
    }

    public override void RestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, JSONArray presetAtoms = null, bool setMissingToDefault = true)
    {
        base.RestoreFromJSON(jc, restorePhysical, restoreAppearance, presetAtoms, setMissingToDefault);

        try
        {
            _loaded = true;
            _customCommands.RestoreFromJSON(jc["actions"]);
            if (_customCommands.count > 0)
                OnActionsChanged();

        }
        catch (Exception exc)
        {
            SuperController.LogError($"Keybindings: {nameof(CustomActions)}.{nameof(RestoreFromJSON)}: {exc}");
        }
    }

    private void OnActionsChanged()
    {
        SuperController.singleton.BroadcastMessage(nameof(IActionsInvoker.OnActionsProviderAvailable), this, SendMessageOptions.DontRequireReceiver);
    }

    public void OnDestroy()
    {
        SuperController.singleton.BroadcastMessage(nameof(IActionsInvoker.OnActionsProviderDestroyed), this, SendMessageOptions.DontRequireReceiver);
    }

    public void OnBindingsListRequested(ICollection<object> bindings)
    {
        foreach (ICustomCommand command in _customCommands)
        {
            if (command.bindable == null) continue;
            bindings.Add(command.bindable);
        }
    }
}
