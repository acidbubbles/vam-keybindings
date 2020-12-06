using System;
using System.Collections.Generic;
using UnityEngine;

public class JSONStorableActionAction
{
    public JSONStorable storable;
    public JSONStorableAction action;
}

public class RemoteActionsManager
{
    private readonly Dictionary<string, JSONStorableActionAction> _actionsMap = new Dictionary<string, JSONStorableActionAction>();
    private readonly Dictionary<JSONStorable, List<JSONStorableActionAction>> _receiversMap = new Dictionary<JSONStorable, List<JSONStorableActionAction>>();

    public void Execute(string name)
    {
        JSONStorableActionAction action;
        if (!_actionsMap.TryGetValue(name, out action))
        {
            SuperController.LogError($"Action '{name}' was not found. Maybe the action this binding was mapped to is associated with an atom that is not present in the current scene.");
            return;
        }

        if (!ValidateReceiver(action.storable))
            return;

        action.action.actionCallback.Invoke();
    }

    public void TryRegister(JSONStorable storable)
    {
        RemoveReceiver(storable);

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

        if (bindings.Count > 0)
        {
            var actions = new List<JSONStorableActionAction>();
            foreach (var binding in bindings)
            {
                if (binding is JSONStorableAction)
                {
                    var actionBinding = binding as JSONStorableAction;
                    var action = new JSONStorableActionAction {action = actionBinding, storable = storable};
                    _actionsMap[actionBinding.name] = action;
                    SuperController.LogMessage($"Mapped {actionBinding.name}");
                    actions.Add(action);
                    continue;
                }

                SuperController.LogError($"Shortcuts: Received unknown binding type {binding.GetType()} from {storable.name} in atom {(storable.containingAtom != null ? storable.containingAtom.name : "(destroyed)")}.");
            }
            _receiversMap[storable] = actions;
        }
    }

    private void RemoveReceiver(JSONStorable storable)
    {
        List<JSONStorableActionAction> existing;
        if (!_receiversMap.TryGetValue(storable, out existing))
            return;

        foreach (var action in existing)
        {
            _actionsMap.Remove(action.action.name);
        }

        _receiversMap.Remove(storable);
    }

    private bool ValidateReceiver(JSONStorable storable)
    {
        if (storable == null)
        {
            RemoveReceiver(storable);
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

    public IEnumerable<string> ToList()
    {
        return _actionsMap.Keys.ToList();
    }
}
