using UnityEngine;
using UnityObject = UnityEngine.Object;

public class TriggerAction : IAction, TriggerHandler
{
    private readonly ITriggerUI _ui;
    private Trigger _trigger;

    public TriggerAction(ITriggerUI ui)
    {
        _ui = ui;
        _trigger = new Trigger {handler = this};
    }

    public void Validate()
    {
        _trigger?.Validate();
    }

    public void SyncAtomNames()
    {
        _trigger?.SyncAtomNames();
    }

    public void Invoke()
    {
        _trigger.active = true;
        _trigger.active = false;
    }

    public void Edit()
    {
        _trigger.triggerActionsParent = _ui.triggerActionsParent;
        _trigger.OpenTriggerActionsPanel();
    }

    public void RemoveTrigger(Trigger trigger)
    {
    }

    public void DuplicateTrigger(Trigger trigger)
    {
    }

    public RectTransform CreateTriggerActionsUI()
    {
        RectTransform rt = null;
        if (_ui.triggerActionsPrefab != null)
        {
            rt = (RectTransform) UnityObject.Instantiate(_ui.triggerActionsPrefab);
        }
        else
        {
            Debug.LogError("Attempted to make TriggerActionsUI when prefab was not set");
        }

        return (rt);
    }

    public RectTransform CreateTriggerActionMiniUI()
    {
        RectTransform rt = null;
        if (_ui.triggerActionMiniPrefab != null)
        {
            rt = (RectTransform) UnityObject.Instantiate(_ui.triggerActionMiniPrefab);
        }
        else
        {
            Debug.LogError("Attempted to make TriggerActionMiniUI when prefab was not set");
        }

        return (rt);
    }

    public RectTransform CreateTriggerActionDiscreteUI()
    {
        RectTransform rt = null;
        if (_ui.triggerActionDiscretePrefab != null)
        {
            rt = (RectTransform) UnityObject.Instantiate(_ui.triggerActionDiscretePrefab);
        }
        else
        {
            Debug.LogError("Attempted to make TriggerActionDiscreteUI when prefab was not set");
        }

        return (rt);
    }

    public RectTransform CreateTriggerActionTransitionUI()
    {
        RectTransform rt = null;
        if (_ui.triggerActionTransitionPrefab != null)
        {
            rt = (RectTransform) UnityObject.Instantiate(_ui.triggerActionTransitionPrefab);
        }
        else
        {
            Debug.LogError("Attempted to make TriggerActionTransitionUI when prefab was not set");
        }

        return (rt);
    }

    public void RemoveTriggerActionUI(RectTransform rt)
    {
        if (rt != null)
        {
            UnityObject.Destroy(rt.gameObject);
        }
    }
}
