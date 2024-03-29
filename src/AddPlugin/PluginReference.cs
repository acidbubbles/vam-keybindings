﻿using System.Text.RegularExpressions;
using MVR.FileManagementSecure;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Events;

public class PluginReference
{
    // AcidBubbles.Cornwall.2:/Custom/Scripts/AcidBubbles/Cornwall/Cornwall.cs
    private static readonly Regex _varPattern = new Regex(@"^(.+?)\.(.+?)\.([0-9]+):", RegexOptions.Compiled);
    // Custom/Scripts/Dev/vam-timeline/VamTimeline.AtomAnimation.cslist
    private static readonly Regex _pathPattern = new Regex(@"([^/]+).(cs|cslist|dll)$", RegexOptions.Compiled);
    private static readonly Regex _sanitizePattern = new Regex(@"[^a-zA-Z_-]+", RegexOptions.Compiled);

    public readonly UnityEvent onChange = new UnityEvent();
    public readonly UnityEvent onRemove = new UnityEvent();
    public bool hasValue => !string.IsNullOrEmpty(_pluginJSON.val);
    public string commandName => _commandNameJSON.val;

    private readonly JSONStorableUrl _pluginJSON = new JSONStorableUrl("PluginUrl", null, "cs|cslist|dll", "Custom/Scripts");
    private readonly JSONStorableString _labelJSON = new JSONStorableString("Label", null);
    private readonly JSONStorableString _commandNameJSON = new JSONStorableString("CommandName", null);
    private readonly MVRScript _plugin;
    private readonly UIDynamicButton _selectButton;
    private readonly UIDynamicButton _removeButton;

    public PluginReference(MVRScript plugin)
    {
        _plugin = plugin;
        _selectButton = plugin.CreateButton("Browse...");
        _labelJSON.setCallbackFunction = val => _selectButton.label = string.IsNullOrEmpty(_labelJSON.val) ? "Browse..." : _labelJSON.val;
        _pluginJSON.beginBrowseWithObjectCallback = jsu => { jsu.shortCuts = FileManagerSecure.GetShortCutsForDirectory("Custom/Scripts", true, true, true, true); };
        _pluginJSON.setCallbackFunction = val =>
        {
            if (string.IsNullOrEmpty(val)) return;
            SyncLabel(val);
            onChange.Invoke();
        };
        _pluginJSON.fileBrowseButton = _selectButton.button;

        _removeButton = plugin.CreateButton("Remove", true);
        _removeButton.button.onClick.AddListener(() => onRemove.Invoke());
        _removeButton.textColor = Color.white;
        _removeButton.buttonColor = Color.red;
    }

    public void Unregister()
    {
        _plugin.RemoveButton(_selectButton);
        _plugin.RemoveButton(_removeButton);
    }

    private void SyncLabel(string val)
    {
        var varMatch = _varPattern.Match(val);
        if (varMatch.Success)
        {
            _labelJSON.val = varMatch.Value.Substring(0, varMatch.Value.Length - 1);
            _commandNameJSON.val = _sanitizePattern.Replace(varMatch.Groups[2].Value, "_");
            return;
        }
        var pathMatch = _pathPattern.Match(val);
        if (pathMatch.Success)
        {
            _labelJSON.val = "[Script] " + pathMatch.Groups[1].Value;
            _commandNameJSON.val = "Script_" + _sanitizePattern.Replace(pathMatch.Groups[1].Value, "_");
            return;
        }
        _labelJSON.val = $"?{val}";
        _commandNameJSON.val = _sanitizePattern.Replace(val, "_");
    }

    public void Clear()
    {
        _pluginJSON.val = "";
        _labelJSON.val = "";
        _commandNameJSON.val = "";
    }

    public JSONStorableAction CreateBinding()
    {
        return new JSONStorableAction(_commandNameJSON.val, Invoke);
    }

    private void Invoke()
    {
        var atom = SuperController.singleton.GetSelectedAtom();
        if (atom == null)
        {
            SuperController.LogError($"Keybindings: Please select an atom on which you want to add {_labelJSON.val}");
            return;
        }
        var pluginManager = atom.GetStorableByID("PluginManager") as MVRPluginManager;
        if (pluginManager == null)
        {
            SuperController.LogError($"Keybindings: The atom '{atom.uid}' of type {atom.type} does not support plugins");
            return;
        }
        var plugin = pluginManager.CreatePlugin();
        plugin.pluginURLJSON.val = _pluginJSON.val;
        SuperController.singleton.OpenTab(atom, "Plugins");
        plugin.scriptControllers[0].script.UITransform.gameObject.SetActive(true);
    }

    public JSONNode GetJSON()
    {
        var jc = new JSONClass();
        _labelJSON.StoreJSON(jc);
        _commandNameJSON.StoreJSON(jc);
        _pluginJSON.StoreJSON(jc);
        return jc;
    }

    public void RestoreFromJSON(JSONClass jc)
    {
        _labelJSON.RestoreFromJSON(jc);
        _commandNameJSON.RestoreFromJSON(jc);
        _pluginJSON.RestoreFromJSON(jc);
    }
}
