using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class RemoteCommandsManager
{
    // NOTE: We'll want multiple actions for the same name, based on the last select atom for example.
    private readonly Dictionary<string, ICommandInvoker> _actionsMap = new Dictionary<string, ICommandInvoker>();
    public List<string> names { get; } = new List<string>();
    public IEnumerable<ICommandInvoker> commands => _actionsMap.Select(kvp => kvp.Value);

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
            SuperController.LogError($"Failed invoking {commandInvoker.commandName}: {exc}");
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

        var commandNamespace = GetNamespace(storable.name);

        foreach (var binding in bindings)
        {
            var storableAction = binding as JSONStorableAction;
            if (storableAction != null)
            {
                var commandName = $"{commandNamespace}.{storableAction.name}";
                var invoker = new JSONStorableActionCommandInvoker {action = storableAction, storable = storable, commandName = commandName, ns = commandNamespace, localName = storableAction.name};
                _actionsMap[commandName] = invoker;
                names.Add(commandName);
                continue;
            }

            SuperController.LogError($"Keybindings: Received unknown binding type {binding.GetType()} from {storable.name} in atom {(storable.containingAtom != null ? storable.containingAtom.name : "(destroyed)")}.");
        }

        names.Sort();
    }

    private static readonly Regex _pluginNameRegex = new Regex("^plugin#[0-9]+_(.+)$", RegexOptions.Compiled);

    private static string GetNamespace(string storableName)
    {
        var pluginName = _pluginNameRegex.Match(storableName);
        return pluginName.Success ? pluginName.Groups[1].Value : storableName;
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
