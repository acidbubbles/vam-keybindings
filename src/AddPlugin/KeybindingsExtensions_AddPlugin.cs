﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;
using UnityEngine;

// ReSharper disable once InconsistentNaming
public class KeybindingsExtensions_AddPlugin : MVRScript
{
    private readonly List<PluginReference> _plugins = new List<PluginReference>();
    private bool _loading;
    private bool _loaded;

    public override void Init()
    {
        base.Init();

        SuperController.singleton.StartCoroutine(DeferredInit());

        CreateButton("+ Add Plugin").button.onClick.AddListener(() => AddPlugin());
    }

    private IEnumerator DeferredInit()
    {
        yield return new WaitForEndOfFrame();
        if (this == null) yield break;
        if (!_loaded) containingAtom.RestoreFromLast(this);
        AddPlugin();
    }

    private PluginReference AddPlugin()
    {
        var plugin = new PluginReference(this);
        plugin.onChange.AddListener(OnPluginsListChanged);
        _plugins.Add(plugin);
        return plugin;
    }

    #region Save/Load

    public override JSONClass GetJSON(bool includePhysical = true, bool includeAppearance = true, bool forceStore = false)
    {
        var json = base.GetJSON(includePhysical, includeAppearance, forceStore);

        try
        {
            if (_plugins.Any(p => p.hasValue))
            {
                var pluginsJSON = new JSONArray();
                foreach (var plugin in _plugins)
                    pluginsJSON.Add(plugin.GetJSON());
                json["Plugins"] = pluginsJSON;
                needsStore = true;
            }
        }
        catch (Exception exc)
        {
            SuperController.LogError($"{nameof(KeybindingsExtensions_AddPlugin)}.{nameof(GetJSON)}: {exc}");
        }

        return json;
    }

    public override void RestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, JSONArray presetAtoms = null, bool setMissingToDefault = true)
    {
        base.RestoreFromJSON(jc, restorePhysical, restoreAppearance, presetAtoms, setMissingToDefault);
        try
        {
            _loading = true;
            var pluginsJSON = jc["Plugins"].AsArray;
            if (pluginsJSON.Count == 0) return;
            foreach (JSONNode pluginJSON in pluginsJSON)
            {
                var plugin = AddPlugin();
                plugin.RestoreFromJSON(pluginJSON.AsObject);
            }
            OnPluginsListChanged();
        }
        catch (Exception exc)
        {
            SuperController.LogError($"{nameof(KeybindingsExtensions_AddPlugin)}.{nameof(RestoreFromJSON)}: {exc}");
            _loading = false;
            throw;
        }
        finally
        {
            _loaded = true;
        }
    }


    #endregion

    #region Keybindings integration

    private void OnPluginsListChanged()
    {
        if (_loading) return;
        transform.parent.BroadcastMessage(nameof(IActionsInvoker.OnActionsProviderAvailable), this, SendMessageOptions.DontRequireReceiver);
    }

    public void OnDestroy()
    {
        transform.parent.BroadcastMessage(nameof(IActionsInvoker.OnActionsProviderDestroyed), this, SendMessageOptions.DontRequireReceiver);
    }

    public void OnBindingsListRequested(ICollection<object> bindings)
    {
        bindings.Add(new Dictionary<string, string>
        {
            {"Namespace", "AddPlugin"}
        });
        foreach (var plugin in _plugins)
        {
            if (!plugin.hasValue) continue;
            bindings.Add(plugin.CreateBinding());
        }
    }

    #endregion
}
