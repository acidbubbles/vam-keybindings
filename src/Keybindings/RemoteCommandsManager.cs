﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class RemoteCommandsManager
{
    private static readonly Regex _pluginNameRegex = new Regex("^plugin#[0-9]+_(.+)$", RegexOptions.Compiled);

    private readonly MonoBehaviour _owner;
    private readonly SelectionHistoryManager _selectionHistoryManager;

    private readonly Dictionary<string, List<IActionCommandInvoker>> _actionCommandsByName = new Dictionary<string, List<IActionCommandInvoker>>();
    private readonly Dictionary<string, List<IAnalogCommandInvoker>> _analogCommandsByName = new Dictionary<string, List<IAnalogCommandInvoker>>();
    public List<string> names { get; } = new List<string>();
    public IEnumerable<IActionCommandInvoker> actionCommands => _actionCommandsByName.Select(kvp => kvp.Value[0]);
    public IEnumerable<IAnalogCommandInvoker> analogCommands => _analogCommandsByName.Select(kvp => kvp.Value[0]);

    public RemoteCommandsManager(MonoBehaviour owner, SelectionHistoryManager selectionHistoryManager)
    {
        _owner = owner;
        _selectionHistoryManager = selectionHistoryManager;
    }

    public bool TryGetInvoker(string name, out IActionCommandInvoker commandInvoker)
    {
        List<IActionCommandInvoker> commandInvokers;
        if (!_actionCommandsByName.TryGetValue(name, out commandInvokers))
        {
            commandInvoker = null;
            return false;
        }

        commandInvoker = SelectCommandInvoker(commandInvokers);
        return true;
    }

    public ICommandReleaser Invoke(string name)
    {
        IActionCommandInvoker commandInvoker;
        if (!TryGetInvoker(name, out commandInvoker)) return null;

        try
        {
            return commandInvoker.Invoke();
        }
        catch (Exception exc)
        {
            SuperController.LogError($"Keybindings: Failed invoking {commandInvoker.commandName}: {exc}");
            return null;
        }
    }


    public bool UpdateValue(string name, float value)
    {
        List<IAnalogCommandInvoker> commandInvokers;
        if (!_analogCommandsByName.TryGetValue(name, out commandInvokers))
        {
            return false;
        }

        var commandInvoker = SelectCommandInvoker(commandInvokers);

        try
        {
            commandInvoker.UpdateValue(value);
            return true;
        }
        catch (Exception exc)
        {
            SuperController.LogError($"Keybindings: Failed invoking {commandInvoker.commandName}: {exc}");
            return false;
        }
    }

    private T SelectCommandInvoker<T>(IList<T> commandInvokers) where T : ICommandInvoker
    {
        for (var i = _selectionHistoryManager.history.Count - 1; i >= 0; i--)
        {
            var atom = _selectionHistoryManager.history[i];
            var latestScript = _selectionHistoryManager.GetLatestScriptPerAtom(atom);
            var hasLatestScript = !ReferenceEquals(latestScript, null);

            for (var invokerIndex = 0; invokerIndex < commandInvokers.Count; invokerIndex++)
            {
                var commandInvoker = commandInvokers[invokerIndex];
                // TODO: Re-order invokers to get most recent at the top?
                // TODO: Scan list twice to map exact storable?
                if (hasLatestScript ? latestScript == commandInvoker.storable : commandInvoker.storable.containingAtom == atom)
                {
                    if (commandInvoker.storable == null)
                    {
                        commandInvokers.RemoveAt(invokerIndex);
                        if (hasLatestScript) _selectionHistoryManager.Clear(atom);
                        invokerIndex--;
                        continue;
                    }

                    if (!commandInvoker.storable.isActiveAndEnabled)
                        continue;

                    return commandInvoker;
                }
            }
        }

        return commandInvokers[0];
    }

    public void TryRegister(JSONStorable storable)
    {
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

        TryRegister(storable, bindings);
    }

    public void TryRegister(JSONStorable storable, ICollection bindings)
    {
        if (bindings.Count <= 0)
            return;

        var commandNamespace = GetNamespace(storable.name);

        var first = true;
        foreach (var binding in bindings)
        {
            if (binding == null) continue;

            if (first)
            {
                first = false;
                var settings = binding as IEnumerable<KeyValuePair<string, string>>;
                if (settings != null)
                {
                    foreach (var setting in settings)
                    {
                        switch (setting.Key)
                        {
                            case Constants.NamespaceKey:
                                commandNamespace = setting.Value;
                                break;
                            default:
                                continue;
                        }
                    }

                    continue;
                }
            }

            var storableAction = binding as JSONStorableAction;
            if (storableAction != null)
            {
                var invoker = new JSONStorableActionCommandInvoker(storable, commandNamespace, storableAction.name, storableAction);
                Add(invoker);
                continue;
            }

            var storableBool = binding as JSONStorableBool;
            if (storableBool != null)
            {
                var invoker = new JSONStorableBoolCommandInvoker(storable, commandNamespace, storableBool.name, storableBool);
                Add(invoker);
                continue;
            }

            var storableFloat = binding as JSONStorableFloat;
            if (storableFloat != null)
            {
                var invoker = new JSONStorableFloatCommandInvoker(storable, commandNamespace, storableFloat.name, storableFloat);
                Add(invoker);
                continue;
            }

            SuperController.LogError(
                $"Keybindings: Received unknown binding type {binding.GetType()} from {storable.name} in atom {(storable.containingAtom != null ? storable.containingAtom.name : "(destroyed)")}.");
        }

        names.Clear();
        names.AddRange(_actionCommandsByName.Select(kvp => kvp.Key));
        names.Sort();

        var script = storable as MVRScript;
        if (script != null && !InjectUISpyNow(script))
            _owner.StartCoroutine(InjectUISpy(script));
    }

    private IEnumerator InjectUISpy(MVRScript script)
    {
        yield return new WaitForEndOfFrame();
        InjectUISpyNow(script);
    }

    private bool InjectUISpyNow(MVRScript script)
    {
        if(script.UITransform == null) return false;
        if (script.UITransform.gameObject.GetComponent<PluginUISpy>() != null) return true;
        var spy = script.UITransform.gameObject.AddComponent<PluginUISpy>();
        if (spy != null) spy.onSelected.AddListener(() => { _selectionHistoryManager.SetLatestScriptPerAtom(script); });
        return true;
    }

    public void Add(IActionCommandInvoker invoker)
    {
        Add(_actionCommandsByName, invoker, true);
    }

    public void Add(IAnalogCommandInvoker invoker)
    {
        Add(_analogCommandsByName, invoker, false);
    }

    private void Add<T>(Dictionary<string, List<T>> commandsByName, T invoker, bool findable) where T : ICommandInvoker
    {
        List<T> commandInvokers;
        if (!commandsByName.TryGetValue(invoker.commandName, out commandInvokers))
        {
            commandInvokers = new List<T>(1);
            commandsByName.Add(invoker.commandName, commandInvokers);
        }
        commandInvokers.Add(invoker);
        if (findable)
        {
            // TODO: This line is weird
            names.Add(invoker.commandName);
        }
    }

    private static string GetNamespace(string storableName)
    {
        var pluginName = _pluginNameRegex.Match(storableName);
        return pluginName.Success ? pluginName.Groups[1].Value : storableName;
    }

    public void Remove(JSONStorable storable)
    {
        {
            var commandToRemove = new List<string>();
            foreach (var commandInvokers in _actionCommandsByName)
            {
                commandInvokers.Value.Remove(commandInvokers.Value.FirstOrDefault(v => v.storable == storable));
                if (commandInvokers.Value.Count == 0)
                    commandToRemove.Add(commandInvokers.Key);
            }

            foreach (var commandName in commandToRemove)
            {
                _actionCommandsByName.Remove(commandName);
                names.Remove(commandName);
            }
        }

        {
            var commandToRemove = new List<string>();
            foreach (var commandInvokers in _analogCommandsByName)
            {
                commandInvokers.Value.Remove(commandInvokers.Value.FirstOrDefault(v => v.storable == storable));
                if (commandInvokers.Value.Count == 0)
                    commandToRemove.Add(commandInvokers.Key);
            }

            foreach (var commandName in commandToRemove)
            {
                _analogCommandsByName.Remove(commandName);
                names.Remove(commandName);
            }
        }

        _selectionHistoryManager.Clear(storable);
    }

    public void Dispose()
    {
        foreach (var command in _actionCommandsByName)
        {
            foreach (var invoker in command.Value)
            {
                var script = invoker.storable as MVRScript;
                if (script == null || script.UITransform == null) continue;
                UnityEngine.Object.Destroy(script.UITransform.GetComponent<PluginUISpy>());
            }
        }

        foreach (var command in _analogCommandsByName)
        {
            foreach (var invoker in command.Value)
            {
                var script = invoker.storable as MVRScript;
                if (script == null || script.UITransform == null) continue;
                UnityEngine.Object.Destroy(script.UITransform.GetComponent<PluginUISpy>());
            }
        }
    }
}
