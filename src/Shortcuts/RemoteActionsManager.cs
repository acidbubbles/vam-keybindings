using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RemoteActionsManager
{
    private readonly List<Receiver> _receivers = new List<Receiver>();

    public void TryRegister(JSONStorable storable)
    {
        var existing = _receivers.FirstOrDefault(r => r.storable == storable);
        if (existing != null) _receivers.Remove(existing);

        var bindings = new List<object>();
        try
        {
            storable.SendMessage("OnBindingsListRequested", bindings, SendMessageOptions.DontRequireReceiver);
        }
        catch (Exception exc)
        {
            SuperController.LogError($"Failed requesting bindings on {storable.name} in atom {storable.containingAtom.name}: {exc}");
            return;
        }

        if (bindings.Count > 0)
        {
            var actions = new List<JSONStorableAction>();
            foreach (var binding in bindings)
            {
                if (!(TryAdd(actions, binding)))
                    SuperController.LogError($"Shortcuts: Received unknown binding type {binding.GetType()} from {storable.name} in atom {storable.containingAtom?.name ?? "(no containing atom)"}.");
            }

            _receivers.Add(new Receiver
            {
                storable = storable,
                actions = actions,
            });
        }
    }

    private static bool TryAdd<T>(List<T> list, object binding) where T : class
    {
        var typed = binding as T;
        if (typed != null)
        {
            list.Add(typed);
            return true;
        }

        return false;
    }

    private bool ValidateReceiver(Receiver receiver)
    {
        if (receiver.storable == null)
        {
            _receivers.Remove(receiver);
            SuperController.LogError($"Shortcuts: The receiver does not exist anymore.");
            return false;
        }

        if (!receiver.storable.isActiveAndEnabled)
        {
            SuperController.LogError($"Shortcuts: The receiver {receiver.storable.containingAtom?.name ?? "(unspecified)"}/{receiver.storable.name} is disabled.");
            return false;
        }

        return true;
    }

    private class Receiver
    {
        public JSONStorable storable;
        public List<JSONStorableAction> actions;
    }

    public List<string> ToList()
    {
        var list = new List<string>();
        foreach (var receiver in _receivers)
        {
            foreach (var p in receiver.actions)
                list.Add($"{receiver.storable.name}/{p.name}");
        }
        return list;
    }
}
