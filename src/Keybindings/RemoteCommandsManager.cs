using System;
using System.Collections.Generic;
using UnityEngine;

public class RemoteCommandsManager
{
    // NOTE: We'll want multiple actions for the same name, based on the last select atom for example.
    private readonly Dictionary<string, ICommandInvoker> _actionsMap = new Dictionary<string, ICommandInvoker>();
    public List<string> names { get; } = new List<string>();

    public bool TryGetAction(string name, out ICommandInvoker commandInvoker)
    {
        return _actionsMap.TryGetValue(name, out commandInvoker);
    }

    public bool Invoke(string name)
    {
        ICommandInvoker commandInvoker;
        if (!_actionsMap.TryGetValue(name, out commandInvoker))
        {
            return false;
        }

        if (!ValidateReceiver(commandInvoker.storable))
            return false;

        try
        {
            commandInvoker.Invoke();
            return true;
        }
        catch (Exception exc)
        {
            SuperController.LogError($"Failed invoking {commandInvoker.name}: {exc}");
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
            SuperController.LogError($"Keybindings: Failed requesting bindings on {storable.name} in atom {storable.containingAtom.name}: {exc}");
            return;
        }

        if (bindings.Count <= 0)
            return;

        foreach (var binding in bindings)
        {
            var storableAction = binding as JSONStorableAction;
            if (storableAction != null)
            {
                var action = new JSONStorableActionCommandInvoker {action = storableAction, storable = storable};
                _actionsMap[storableAction.name] = action;
                names.Add(storableAction.name);
                continue;
            }

            SuperController.LogError($"Keybindings: Received unknown binding type {binding.GetType()} from {storable.name} in atom {(storable.containingAtom != null ? storable.containingAtom.name : "(destroyed)")}.");
        }

        names.Sort();
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
        {
            _actionsMap.Remove(action);
            // TODO: When we map multiple targets to an action name, check if it's the last
            names.Remove(action);
        }
    }

    private bool ValidateReceiver(JSONStorable storable)
    {
        if (storable == null)
        {
            Remove(storable);
            SuperController.LogError($"Keybindings: The receiver does not exist anymore.");
            return false;
        }

        if (!storable.isActiveAndEnabled)
        {
            SuperController.LogError($"Keybindings: The receiver {(storable.containingAtom != null ? storable.containingAtom.name : "(destroyed)")}/{storable.name} is disabled.");
            return false;
        }

        return true;
    }
}
