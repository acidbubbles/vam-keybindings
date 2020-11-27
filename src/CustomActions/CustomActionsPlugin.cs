using System;
using System.Collections;
using System.Collections.Generic;
using CustomActions;
using SimpleJSON;
using UnityEngine;

public class CustomActionsPlugin : MVRScript, IActionsProvider
{
    private ActionsRepository _actions;
    private PrefabManager _prefabManager;
    private bool _loaded;
    private CustomActionsScreen _ui;

    public override void Init()
    {
        _prefabManager = new PrefabManager();
        _actions = new ActionsRepository(containingAtom, _prefabManager);
        SuperController.singleton.StartCoroutine(_prefabManager.LoadUIAssets());
        SuperController.singleton.StartCoroutine(DeferredInit());
        SuperController.singleton.onAtomUIDRenameHandlers += OnAtomRename;

        _actions.Add("save", new DiscreteTriggerBoundAction(containingAtom, _prefabManager));
        _actions.Add("print.1", new DiscreteTriggerBoundAction(containingAtom, _prefabManager));
        _actions.Add("print.2", new DiscreteTriggerBoundAction(containingAtom, _prefabManager));
        _actions.Add("print.3", new DiscreteTriggerBoundAction(containingAtom, _prefabManager));
        _actions.Add("print.3.4", new DebugBoundAction("debug 3.4"));
        _actions.Add("print.3.5", new DebugBoundAction("debug 3.5"));

        BroadcastingUtil.BroadcastActionsAvailable(this);
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
        _ui = go.AddComponent<CustomActionsScreen>();
        _ui.prefabManager = _prefabManager;
        _ui.actions = _actions;
        _ui.Configure();
        if (active) go.SetActive(true);
    }


    public override void Validate()
    {
        _actions.Validate();
    }

    public void OnAtomRename(string oldid, string newid)
    {
        _actions.SyncAtomNames();
    }

    public override JSONClass GetJSON(bool includePhysical = true, bool includeAppearance = true,
        bool forceStore = false)
    {
        var json = base.GetJSON(includePhysical, includeAppearance, forceStore);

        try
        {
            json["actions"] = _actions.GetJSON();
            needsStore = true;
        }
        catch (Exception exc)
        {
            SuperController.LogError($"{nameof(CustomActions)}.{nameof(GetJSON)} (Serialize): {exc}");
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
            _actions.RestoreFromJSON(jc["actions"]?.AsObject);
        }
        catch (Exception exc)
        {
            SuperController.LogError($"{nameof(CustomActions)}.{nameof(RestoreFromJSON)}: {exc}");
        }
    }

    public void OnBindingsListRequested(ICollection<object> bindings)
    {
    }
}
