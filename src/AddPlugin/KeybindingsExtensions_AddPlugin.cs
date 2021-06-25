using System;
using System.Collections.Generic;
using System.Linq;
using MVR.FileManagementSecure;
using SimpleJSON;
using UnityEngine;

// ReSharper disable once InconsistentNaming
public class KeybindingsExtensions_AddPlugin : MVRScript
{
    private static readonly string _configPath = SuperController.singleton.savesDir + @"\PluginData\keybindingextensions_addplugin.config";

    private readonly List<PluginReference> _plugins = new List<PluginReference>();
    private bool _pauseListChangedEvent;

    public override void Init()
    {
        if (containingAtom.type != "SessionPluginManager")
        {
            SuperController.LogError($"{nameof(KeybindingsExtensions_AddPlugin)} plugin can only be installed as a session plugin.");
            CreateTextField(new JSONStorableString("Error", $"{nameof(KeybindingsExtensions_AddPlugin)} plugin can only be installed as a session plugin."));
            enabledJSON.val = false;
            return;
        }

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
                _pauseListChangedEvent = true;
                foreach (var plugin in _plugins)
                    plugin.onRemove.Invoke();
            }
            finally
            {
                _plugins.Clear();
                _pauseListChangedEvent = false;
                OnPluginsListChanged();
            }
        });

        if (FileManagerSecure.FileExists(_configPath))
            RestoreFromJSON(LoadJSON(_configPath).AsObject);
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

    private PluginReference AddPlugin()
    {
        var plugin = new PluginReference(this);
        plugin.onChange.AddListener(() =>
        {
            if (_plugins.Where(p => p.hasValue).GroupBy(p => p.commandName).Any(g => g.Count() > 1))
            {
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
                {
                    if (!plugin.hasValue) continue;
                    pluginsJSON.Add(plugin.GetJSON());
                }
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
            _pauseListChangedEvent = true;
            var pluginsJSON = jc["Plugins"].AsArray;
            if (pluginsJSON.Count == 0) return;
            foreach (JSONNode pluginJSON in pluginsJSON)
            {
                var plugin = AddPlugin();
                plugin.RestoreFromJSON(pluginJSON.AsObject);
                if (!plugin.hasValue)
                {
                    plugin.Unregister();
                    _plugins.Remove(plugin);
                }
            }
            transform.parent.parent.BroadcastMessage(nameof(IActionsInvoker.OnActionsProviderAvailable), this, SendMessageOptions.DontRequireReceiver);
        }
        catch (Exception exc)
        {
            SuperController.LogError($"{nameof(KeybindingsExtensions_AddPlugin)}.{nameof(RestoreFromJSON)}: {exc}");
        }
        finally
        {
            _pauseListChangedEvent = false;
        }
    }

    #endregion

    #region Keybindings integration

    private void OnPluginsListChanged()
    {
        if (_pauseListChangedEvent) return;
        transform.parent.parent.BroadcastMessage(nameof(IActionsInvoker.OnActionsProviderAvailable), this, SendMessageOptions.DontRequireReceiver);

        FileManagerSecure.CreateDirectory(@"Saves\PluginData");
        SaveJSON(GetJSON(), _configPath);
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
