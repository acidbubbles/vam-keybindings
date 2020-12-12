using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class RemoteCommandsManager
{
    // NOTE: We'll want multiple actions for the same name, based on the last select atom for example.
    private readonly Dictionary<string, List<ICommandInvoker>> _commandsMap = new Dictionary<string, List<ICommandInvoker>>();
    public List<string> names { get; } = new List<string>();
    public IEnumerable<ICommandInvoker> commands => _commandsMap.Select(kvp => kvp.Value[0]);

    public bool Invoke(string name)
    {
        List<ICommandInvoker> commandInvokers;
        if (!_commandsMap.TryGetValue(name, out commandInvokers))
        {
            return false;
        }

        // TODO: Use the SelectionHistoryManager to find the best match if there's more than one
        var commandInvoker = commandInvokers[0];

        // TODO: This can probably be merged with SelectionHistoryManager
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
                var invoker = new JSONStorableActionCommandInvoker(storable, commandNamespace, storableAction.name, storableAction);
                Add(invoker);
                continue;
            }

            SuperController.LogError($"Keybindings: Received unknown binding type {binding.GetType()} from {storable.name} in atom {(storable.containingAtom != null ? storable.containingAtom.name : "(destroyed)")}.");
        }

        names.Clear();
        names.AddRange(_commandsMap.Select(kvp => kvp.Key));
        names.Sort();
    }

    public void Add(ICommandInvoker invoker)
    {
        List<ICommandInvoker> commandInvokers;
        if (!_commandsMap.TryGetValue(invoker.commandName, out commandInvokers))
        {
            commandInvokers = new List<ICommandInvoker>(1);
            _commandsMap.Add(invoker.commandName, commandInvokers);
        }
        commandInvokers.Add(invoker);
        // TODO: This line is weird
        names.Add(invoker.commandName);
    }

    private static readonly Regex _pluginNameRegex = new Regex("^plugin#[0-9]+_(.+)$", RegexOptions.Compiled);

    private static string GetNamespace(string storableName)
    {
        var pluginName = _pluginNameRegex.Match(storableName);
        return pluginName.Success ? pluginName.Groups[1].Value : storableName;
    }

    public void Remove(JSONStorable storable)
    {
        var commandToRemove = new List<string>();
        foreach (var commandInvokers in _commandsMap)
        {
            // TODO: Single might be overkill
            commandInvokers.Value.Remove(commandInvokers.Value.SingleOrDefault(v => v.storable == storable));
            if (commandInvokers.Value.Count == 0)
                commandToRemove.Add(commandInvokers.Key);
        }

        foreach (var commandName in commandToRemove)
        {
            _commandsMap.Remove(commandName);
            // TODO: When we map multiple targets to an action name, check if it's the last
            names.Remove(commandName);
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
