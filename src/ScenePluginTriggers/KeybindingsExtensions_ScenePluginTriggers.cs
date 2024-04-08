using System;
using System.Collections.Generic;
using System.Linq;
using MVR.FileManagementSecure;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable once InconsistentNaming
public class KeybindingsExtensions_ScenePluginTriggers : MVRScript
{
    private static readonly string _configPath = SuperController.singleton.savesDir + @"\PluginData\keybindingextensions_sceneplugintriggers.config";

    private static readonly HashSet<string> _ignored = new HashSet<string>
    {
        "SaveToStore1",
        "SaveToStore2",
        "SaveToStore3",
        "RestoreAllFromDefaults",
        "RestorePhysicsFromDefaults",
        "RestoreAppearanceFromDefaults",
        "RestorePhysicalFromDefaults",
        "RestoreAllFromStore1",
        "RestorePhysicsFromStore1",
        "RestoreAppearanceFromStore1",
        "RestoreAllFromStore2",
        "RestorePhysicsFromStore2",
        "RestoreAppearanceFromStore2",
        "RestoreAllFromStore3",
        "RestorePhysicsFromStore3",
        "RestoreAppearanceFromStore3",
    };

    private readonly JSONStorableAction _refreshBinding;
    private readonly JSONStorableAction _settingsBinding;
    private readonly Dictionary<string, PluginTriggerActionBinding> _actions = new Dictionary<string, PluginTriggerActionBinding>();
    private readonly Dictionary<string, PluginTriggerBoolBinding> _booleans = new Dictionary<string, PluginTriggerBoolBinding>();
    private readonly Dictionary<string, PluginTriggerMissingBinding> _missing = new Dictionary<string, PluginTriggerMissingBinding>();
    private readonly JSONStorableString _enabledNamesJSON = new JSONStorableString("EnabledPluginTriggersList", "");
    private readonly List<string> _enabledNames = new List<string>();
    private readonly List<UIDynamic> _spacers = new List<UIDynamic>();

    public KeybindingsExtensions_ScenePluginTriggers()
    {
        _refreshBinding = new JSONStorableAction("RefreshList", UpdateList);
        _settingsBinding = new JSONStorableAction("Settings", OpenSettings);
    }

    public override void Init()
    {
        if (containingAtom.type != "SessionPluginManager")
        {
            SuperController.LogError($"{nameof(KeybindingsExtensions_ScenePluginTriggers)} plugin can only be installed as a session plugin.");
            CreateTextField(new JSONStorableString("Error", $"{nameof(KeybindingsExtensions_ScenePluginTriggers)} plugin can only be installed as a session plugin."));
            enabledJSON.val = false;
            return;
        }

        CreateTextField(new JSONStorableString("", "This screen lists all plugins in the current scene. Toggle the ones you want to add the Keybindings (requires the main Keybindings plugin). You can then map then to a shortcut, or call them using fuzzy finding."));

        CreateButton("Refresh").button.onClick.AddListener(() => Invoke(nameof(UpdateList), 0));

        _enabledNamesJSON.setCallbackFunction = val =>
        {
            _enabledNames.Clear();
            _enabledNames.AddRange(val.Split('\n').Where(x => x != ""));
        };

        if (FileManagerSecure.FileExists(_configPath))
            RestoreFromJSON(LoadJSON(_configPath).AsObject);

        Invoke(nameof(UpdateList), 0);

        SuperController.singleton.onAtomRemovedHandlers += OnAtomRemoved;
        SuperController.singleton.onSceneLoadedHandlers += OnSceneLoaded;
        SuperController.singleton.onSubSceneLoadedHandlers += OnSubsceneLoaded;
    }

    public override void InitUI()
    {
        base.InitUI();
        if (UITransform == null) return;
        leftUIContent.anchorMax = new Vector2(1, 1);
    }

    private void UpdateList()
    {
        foreach (var spacer in _spacers)
            RemoveSpacer(spacer);
        foreach (var action in _actions)
            RemoveToggle(action.Value.enabledJSON);
        _actions.Clear();
        foreach (var boolean in _booleans)
            RemoveToggle(boolean.Value.enabledJSON);
        _booleans.Clear();
        foreach (var m in _missing)
            RemoveToggle(m.Value.enabledJSON);
        _missing.Clear();

        foreach (var atom in SuperController.singleton.GetAtoms())
        {
            foreach (var storable in atom.GetStorableIDs().Select(id => atom.GetStorableByID(id)))
            {
                if (storable == null) continue;
                if (!storable.name.StartsWith("plugin#")) continue;
                var storableNameUnderscoreIndex = storable.name.IndexOf("_", StringComparison.Ordinal);
                if (storableNameUnderscoreIndex == -1) continue;
                var storableName = storable.name.Substring(storableNameUnderscoreIndex + 1);

                CreateTitle(storableName);

                foreach (var n in storable.GetActionNames())
                {
                    if (_ignored.Contains(n)) continue;
                    var ns = GetName(storableName, n);
                    PluginTriggerActionBinding a;
                    if (!_actions.TryGetValue(ns, out a))
                    {
                        a = new PluginTriggerActionBinding(ns)
                        {
                            enabledJSON = CreateEnabledToggle($"{n} (Action)", ns)
                        };
                        _actions.Add(ns, a);
                    }
                    a.Add(storable.GetAction(n));
                }
                foreach (var n in storable.GetBoolParamNames())
                {
                    var ns = GetName(storableName, n);
                    PluginTriggerBoolBinding a;
                    if (!_booleans.TryGetValue(ns, out a))
                    {
                        a = new PluginTriggerBoolBinding(ns)
                        {
                            enabledJSON = CreateEnabledToggle($"{n} (Toggle)", ns)
                        };
                        CreateToggle(a.enabledJSON);
                        _booleans.Add(ns, a);
                    }
                    a.Add(storable.GetBoolJSONParam(n));
                }
            }
        }

        var missing = false;
        foreach (var ns in _enabledNames)
        {
            if(_actions.ContainsKey(ns) || _booleans.ContainsKey(ns)) continue;
            if (!missing)
            {
                CreateTitle("Not present in the scene");
                missing = true;
            }

            CreateEnabledToggle(ns, ns);
            var a = new PluginTriggerMissingBinding(ns)
            {
                enabledJSON = CreateEnabledToggle($"{ns} (Missing)", ns)
            };
            _missing.Add(ns, a);
        }

        transform.parent.parent.BroadcastMessage(nameof(IActionsInvoker.OnActionsProviderAvailable), this, SendMessageOptions.DontRequireReceiver);
    }

    private void CreateTitle(string storableName)
    {
        var title = CreateSpacer();
        title.height = 46;
        _spacers.Add(title);
        var text = title.gameObject.AddComponent<Text>();
        text.font = manager.configurableButtonPrefab.GetComponentInChildren<Text>().font;
        text.alignment = TextAnchor.MiddleCenter;
        text.raycastTarget = false;
        text.fontSize = 28;
        text.text = storableName;
    }

    private JSONStorableBool CreateEnabledToggle(string label, string ns)
    {
        var nameEnabledJSON = new JSONStorableBool(label, _enabledNames.Contains(ns), val => { ToggleName(val, ns); });
        CreateToggle(nameEnabledJSON);
        return nameEnabledJSON;
    }

    private void ToggleName(bool val, string ns)
    {
        if (val)
            _enabledNames.Add(ns);
        else
            _enabledNames.Remove(ns);
        _enabledNamesJSON.valNoCallback = string.Join("\n", _enabledNames.ToArray());
        FileManagerSecure.CreateDirectory(@"Saves\PluginData");
        SaveJSON(GetJSON(), _configPath);
        UpdateList();
    }

    private static string GetName(string storable, string name)
    {
        return $"{storable}_{name}";
    }

    private void OpenSettings()
    {
        SuperController.singleton.SetActiveUI("MainMenu");
        SuperController.singleton.SetMainMenuTab("TabSessionPlugins");

        var pluginsPanel = UITransform.parent;
        for (var i = 0; i < pluginsPanel.childCount; i++)
        {
            var pluginPanel = pluginsPanel.GetChild(i);
            UITransform.gameObject.SetActive(pluginPanel == UITransform);
        }
    }

    public void OnBindingsListRequested(ICollection<object> bindings)
    {
        bindings.Add(new Dictionary<string, string>
        {
            {"Namespace", "ScenePluginTriggers"}
        });
        bindings.Add(_refreshBinding);
        bindings.Add(_settingsBinding);
        foreach (var binding in _actions)
            if (_enabledNames.Contains(binding.Key))
                bindings.Add(binding.Value.action);
        foreach (var binding in _booleans)
            if (_enabledNames.Contains(binding.Key))
                bindings.Add(binding.Value.action);
        foreach (var binding in _missing)
            bindings.Add(binding.Value.action);
    }

    private void OnSubsceneLoaded(SubScene subscene)
    {
        Invoke(nameof(UpdateList), 0);
    }

    private void OnSceneLoaded()
    {
        Invoke(nameof(UpdateList), 0);
    }

    private void OnAtomRemoved(Atom atom)
    {
        Invoke(nameof(UpdateList), 0);
    }

    public void OnDestroy()
    {
        SuperController.singleton.onAtomRemovedHandlers -= OnAtomRemoved;
        SuperController.singleton.onSceneLoadedHandlers -= OnSceneLoaded;
        SuperController.singleton.onSubSceneLoadedHandlers -= OnSubsceneLoaded;
        transform.parent.parent.BroadcastMessage(nameof(IActionsInvoker.OnActionsProviderDestroyed), this, SendMessageOptions.DontRequireReceiver);
    }

    public override JSONClass GetJSON(bool includePhysical = true, bool includeAppearance = true, bool forceStore = false)
    {
        var json = base.GetJSON(includePhysical, includeAppearance, forceStore);
        _enabledNamesJSON.StoreJSON(json, includePhysical, includeAppearance, forceStore);
        return json;
    }

    public override void RestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, JSONArray presetAtoms = null, bool setMissingToDefault = true)
    {
        base.RestoreFromJSON(jc, restorePhysical, restoreAppearance, presetAtoms, setMissingToDefault);
        _enabledNamesJSON.RestoreFromJSON(jc, restorePhysical, restoreAppearance, setMissingToDefault);
    }
}
