using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RemoteActionsManager
{
    // NOTE: We'll want multiple actions for the same name, based on the last select atom for example.
    private readonly Dictionary<string, IAction> _actionsMap = new Dictionary<string, IAction>();

    public bool Invoke(string name)
    {
        IAction action;
        if (!_actionsMap.TryGetValue(name, out action))
        {
            return false;
        }

        if (!ValidateReceiver(action.storable))
            return false;

        try
        {
            action.Invoke();
            return true;
        }
        catch (Exception exc)
        {
            SuperController.LogError($"Failed invoking {action.name}: {exc}");
            return false;
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

    public IAction FuzzyFind(string query)
    {
        if (string.IsNullOrEmpty(query))
            return null;

        // TODO: Optimize
        foreach (var kvp in _actionsMap)
        {
            var action = kvp.Key;
            if(action.Length < query.Length) continue;
            var queryIndex = 0;
            for(var actionIndex = 0; actionIndex < action.Length; actionIndex++)
            {
                var queryChar = query[queryIndex];
                var actionChar = action[actionIndex];
                var isMatch = char.IsLower(queryChar) ? queryChar == char.ToLowerInvariant(actionChar) : queryChar == actionChar;
                if (!isMatch) continue;

                queryIndex++;
                if (queryIndex > query.Length - 1)
                    return kvp.Value;
            }
        }
        return null;
    }
}
