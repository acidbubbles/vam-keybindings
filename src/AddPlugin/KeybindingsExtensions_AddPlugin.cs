using System;
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
        if (containingAtom.type != "SessionPluginManager")
        {
            SuperController.LogError($"{nameof(KeybindingsExtensions_AddPlugin)} plugin can only be installed as a session plugin.");
            CreateTextField(new JSONStorableString("Error", $"{nameof(KeybindingsExtensions_AddPlugin)} plugin can only be installed as a session plugin."));
            enabledJSON.val = false;
            return;
        }

        SuperController.singleton.StartCoroutine(DeferredInit());

        var addButton = CreateButton("+ Add Plugin");
        addButton.buttonColor = Color.green;
        addButton.button.onClick.AddListener(() => AddPlugin());
        var clearButton = CreateButton("Clear", true);
        clearButton.textColor = Color.white;
        clearButton.buttonColor = Color.red;
        clearButton.button.onClick.AddListener(() =>
        {
            try
            {
                _loading = true;
                foreach (var plugin in _plugins)
                    plugin.onRemove.Invoke();
            }
            finally
            {
                _plugins.Clear();
                _loading = false;
                OnPluginsListChanged();
            }
        });
    }

    public override void InitUI()
    {
        base.InitUI();

        if (UITransform == null) return;

        var leftRect = leftUIContent.GetComponent<RectTransform>();
        var rightRect = rightUIContent.GetComponent<RectTransform>();

        leftRect.anchorMax = new Vector2(0.85f, 1f);
        rightRect.anchorMin = new Vector2(0.85f, 1f);
    }

    private IEnumerator DeferredInit()
    {
        yield return new WaitForEndOfFrame();
        if (this == null) yield break;
        if (!_loaded) containingAtom.RestoreFromLast(this);
    }

    private PluginReference AddPlugin()
    {
        var plugin = new PluginReference(this);
        plugin.onChange.AddListener(() =>
        {
            if (_plugins.Where(p => p.hasValue).GroupBy(p => p.commandName).Any(g => g.Count() > 1))
            {
                SuperController.LogError($"Keybindings: Plugin {plugin.commandName} cannot be registered twice");
                plugin.Clear();
            }
            OnPluginsListChanged();
        });
        plugin.onRemove.AddListener(() =>
        {
            plugin.Unregister();
            _plugins.Remove(plugin);
        });
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
        }
        finally
        {
            _loading = false;
            _loaded = true;
        }
    }

    #endregion

    #region Keybindings integration

    private void OnPluginsListChanged()
    {
        if (_loading) return;
        transform.parent.parent.BroadcastMessage(nameof(IActionsInvoker.OnActionsProviderAvailable), this, SendMessageOptions.DontRequireReceiver);
    }

    public void OnDestroy()
    {
        transform.parent.parent.BroadcastMessage(nameof(IActionsInvoker.OnActionsProviderDestroyed), this, SendMessageOptions.DontRequireReceiver);
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
