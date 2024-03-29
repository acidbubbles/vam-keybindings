﻿using System;
using System.Diagnostics.CodeAnalysis;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Events;
using UnityObject = UnityEngine.Object;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public abstract class TriggerCommandBase : TriggerHandler
{
    private readonly IPrefabManager _prefabManager;

    protected readonly Trigger trigger;

    public virtual string name => trigger.displayName;

    protected TriggerCommandBase(IPrefabManager prefabManager)
    {
        _prefabManager = prefabManager;
        trigger = new Trigger {handler = this};
    }

    public void Validate()
    {
        trigger?.Validate();
    }

    public void SyncAtomNames()
    {
        trigger?.SyncAtomNames();
    }

    public virtual UnityEvent Edit()
    {
        trigger.triggerActionsParent = _prefabManager.triggerActionsParent;
        return Open();
    }

    protected virtual UnityEvent Open()
    {
        trigger.OpenTriggerActionsPanel();
        return null;
    }

    public void RemoveTrigger(Trigger t)
    {
    }

    public void DuplicateTrigger(Trigger t)
    {
    }

    public RectTransform CreateTriggerActionsUI()
    {
        if (_prefabManager.triggerActionsPrefab == null)
            throw new NullReferenceException(nameof(_prefabManager.triggerActionsPrefab));
        return UnityObject.Instantiate(_prefabManager.triggerActionsPrefab);
    }

    public RectTransform CreateTriggerActionMiniUI()
    {
        if (_prefabManager.triggerActionMiniPrefab == null)
            throw new NullReferenceException(nameof(_prefabManager.triggerActionMiniPrefab));
        return UnityObject.Instantiate(_prefabManager.triggerActionMiniPrefab);
    }

    public RectTransform CreateTriggerActionDiscreteUI()
    {
        if (_prefabManager.triggerActionDiscretePrefab == null)
            throw new NullReferenceException(nameof(_prefabManager.triggerActionDiscretePrefab));
        return UnityObject.Instantiate(_prefabManager.triggerActionDiscretePrefab);
    }

    public RectTransform CreateTriggerActionTransitionUI()
    {
        if (_prefabManager.triggerActionTransitionPrefab == null)
            throw new NullReferenceException(nameof(_prefabManager.triggerActionTransitionPrefab));
        return UnityObject.Instantiate(_prefabManager.triggerActionTransitionPrefab);
    }

    public void RemoveTriggerActionUI(RectTransform rt)
    {
        if (rt != null)
        {
            UnityObject.Destroy(rt.gameObject);
        }
    }

    public virtual JSONClass GetJSON()
    {
        return trigger.GetJSON();
    }

    public virtual void RestoreFromJSON(JSONClass json)
    {
        trigger.RestoreFromJSON(json);
    }
}
