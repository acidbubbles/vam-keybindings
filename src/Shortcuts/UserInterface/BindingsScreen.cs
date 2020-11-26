using System;
using System.Collections.Generic;
using UnityEngine;

public class BindingsScreen : MonoBehaviour
{
    public IBindingsManager bindingsManager { get; set; }
    public IPrefabManager prefabManager { get; set; }

    public void OnEnable()
    {
        // TODO: Write this!
        /*
            CreateTextField(new JSONStorableString("", "Actions"), false);
            foreach (var action in _actions)
            {
                var a = action;
                CreateButton($"Edit {a.Key}", false).button.onClick.AddListener(() => { a.Value.Edit(); });
            }

            CreateTextField(new JSONStorableString("", "Bindings"), true);
            foreach (var action in _actions)
            {
                var a = action;
                CreateButton($"Map {a.Key}", true).button.onClick.AddListener(() => { SuperController.LogMessage("Not implemented"); });
            }
            */
    }

    public void OnDisable()
    {
    }
}
