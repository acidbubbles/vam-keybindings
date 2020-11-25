using System;
using UnityEngine;
using UnityObject = UnityEngine.Object;

public abstract class TriggerBoundAction : TriggerHandler
{
    private readonly IPrefabManager _prefabManager;

    protected readonly Trigger trigger;

    protected TriggerBoundAction(IPrefabManager prefabManager)
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

    public virtual void Edit()
    {
        trigger.triggerActionsParent = _prefabManager.triggerActionsParent;
        Open();
    }

    protected virtual void Open()
    {
        trigger.OpenTriggerActionsPanel();
    }

    public void RemoveTrigger(Trigger t)
    {
    }

    public void DuplicateTrigger(Trigger t)
    {
    }

    public RectTransform CreateTriggerActionsUI()
    {
        if (_prefabManager.triggerActionsPrefab == null) throw new NullReferenceException(nameof(_prefabManager.triggerActionsPrefab));
        return UnityObject.Instantiate(_prefabManager.triggerActionsPrefab);
    }

    public RectTransform CreateTriggerActionMiniUI()
    {
        if (_prefabManager.triggerActionMiniPrefab == null) throw new NullReferenceException(nameof(_prefabManager.triggerActionMiniPrefab));
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
}
