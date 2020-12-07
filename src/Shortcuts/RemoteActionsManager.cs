using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RemoteActionsManager
{
    // NOTE: We'll want multiple actions for the same name, based on the last select atom for example.
    private readonly Dictionary<string, IAction> _actionsMap = new Dictionary<string, IAction>();

    public void Invoke(string name)
    {
        IAction action;
        if (!_actionsMap.TryGetValue(name, out action))
        {
            SuperController.LogError($"Action '{name}' was not found. Maybe the action this binding was mapped to is associated with an atom that is not present in the current scene.");
            return;
        }

        if (!ValidateReceiver(action.storable))
            return;

        try
        {
            action.Invoke();
        }
        catch (Exception exc)
        {
            SuperController.LogError($"Failed invoking {action.name}: {exc}");
        }
    }

    public void TryRegister(JSONStorable storable)
    {
        // TODO: Does not work afaik
        Remove(storable);

        var bindings = new List<object>();
        try
        {
            storable.SendMessage("OnBindingsListRequested", bindings, SendMessageOptions.DontRequireReceiver);
        }
        catch (Exception exc)
        {
            SuperController.LogError($"Shortcuts: Failed requesting bindings on {storable.name} in atom {storable.containingAtom.name}: {exc}");
            return;
        }

        if (bindings.Count <= 0)
            return;

        foreach (var binding in bindings)
        {
            var storableAction = binding as JSONStorableAction;
            if (storableAction != null)
            {
                var action = new JSONStorableActionAction {action = storableAction, storable = storable};
                _actionsMap[storableAction.name] = action;
                continue;
            }

            SuperController.LogError($"Shortcuts: Received unknown binding type {binding.GetType()} from {storable.name} in atom {(storable.containingAtom != null ? storable.containingAtom.name : "(destroyed)")}.");
        }
    }

    public void Remove(JSONStorable storable)
    {
        var actionsToRemove = new List<string>();
        foreach (var action in _actionsMap)
        {
            if (action.Value.storable == storable)
                actionsToRemove.Add(action.Key);
        }
        foreach (var action in actionsToRemove)
            _actionsMap.Remove(action);
    }

    private bool ValidateReceiver(JSONStorable storable)
    {
        if (storable == null)
        {
            Remove(storable);
            SuperController.LogError($"Shortcuts: The receiver does not exist anymore.");
            return false;
        }

        if (!storable.isActiveAndEnabled)
        {
            SuperController.LogError($"Shortcuts: The receiver {(storable.containingAtom != null ? storable.containingAtom.name : "(destroyed)")}/{storable.name} is disabled.");
            return false;
        }

        return true;
    }

    public IEnumerable<IAction> ToList()
    {
        return _actionsMap.Values.OrderBy(v => v.name);
    }
}
