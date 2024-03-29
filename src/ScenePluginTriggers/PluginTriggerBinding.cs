﻿using System.Collections.Generic;
using System.Linq;

public abstract class PluginTriggerBinding
{
    public JSONStorableBool enabledJSON { get; set; }
    public abstract JSONStorableAction action { get; }
}

public class PluginTriggerActionBinding : PluginTriggerBinding
{
    public override JSONStorableAction action { get; }
    private readonly List<JSONStorableAction> _targets = new List<JSONStorableAction>();

    public PluginTriggerActionBinding(string name)
    {
        action = new JSONStorableAction(name, () =>
        {
            if (_targets.Count == 1)
            {
                _targets[0].actionCallback.Invoke();
                return;
            }

            var selected = SuperController.singleton.GetSelectedAtom();
            var target = _targets.FirstOrDefault(t => t.storable.containingAtom == selected);
            if (target == null)
            {
                SuperController.LogError($"Keybindings/ScenePluginTriggers More than one atom found with binding {name}. Select the desired atom first.");
                return;
            }
            target.actionCallback.Invoke();
        });
    }

    public void Add(JSONStorableAction value)
    {
        _targets.Add(value);
    }
}

public class PluginTriggerBoolBinding : PluginTriggerBinding
{
    public override JSONStorableAction action { get; }
    private readonly List<JSONStorableBool> _targets = new List<JSONStorableBool>();

    public PluginTriggerBoolBinding(string name)
    {
        action = new JSONStorableAction(name, () =>
        {
            if (_targets.Count == 1)
            {
                _targets[0].val = !_targets[0].val;
                return;
            }

            var selected = SuperController.singleton.GetSelectedAtom();
            var target = _targets.FirstOrDefault(t => t.storable.containingAtom == selected);
            if (target == null)
            {
                SuperController.LogError($"Keybindings/ScenePluginTriggers More than one atom found with binding {name}. Select the desired atom first.");
                return;
            }
            target.val = !target.val;
        });
    }

    public void Add(JSONStorableBool param)
    {
        _targets.Add(param);
    }
}

public class PluginTriggerMissingBinding : PluginTriggerBinding
{
    public override JSONStorableAction action { get; }

    public PluginTriggerMissingBinding(string name)
    {
        action = new JSONStorableAction(name, () =>
        {
            SuperController.LogError($"Keybindings/ScenePluginTriggers There was no atoms with binding {name}. You can try refreshing with [ScenePluginTriggers.RefreshList].");
        });
    }
}

