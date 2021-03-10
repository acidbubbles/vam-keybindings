using System.Text.RegularExpressions;
using MVR.FileManagementSecure;
using SimpleJSON;
using UnityEngine.Events;

public class PluginReference
{
    private static readonly Regex _varPattern = new Regex(@"^.+?\.(.+?)\.[0-9]+:", RegexOptions.Compiled);
    private static readonly Regex _pathPattern = new Regex(@"([^/]+).(cs|cslist|dll)$", RegexOptions.Compiled);
    private static readonly Regex _sanitizePattern = new Regex(@"[^a-zA-Z_-]+", RegexOptions.Compiled);

    public readonly UnityEvent onChange = new UnityEvent();
    public bool hasValue => !string.IsNullOrEmpty(_pluginJSON.val);

    private readonly JSONStorableUrl _pluginJSON = new JSONStorableUrl("PluginUrl", null, "cs|cslist|dll", "Custom/Scripts");
    private readonly JSONStorableString _labelJSON = new JSONStorableString("Label", null);

    public PluginReference(MVRScript plugin)
    {
        var button = plugin.CreateButton("Select");
        _labelJSON.setCallbackFunction = val => button.label = _labelJSON.val ?? "Select";
        _pluginJSON.beginBrowseWithObjectCallback = jsu =>
        {
            jsu.shortCuts = FileManagerSecure.GetShortCutsForDirectory("Custom/Scripts", true, true, true, true);
        };
        _pluginJSON.setCallbackFunction = val =>
        {
            if (string.IsNullOrEmpty(val)) return;
            // Potential values:
            // - Custom/Scripts/Dev/vam-timeline/VamTimeline.AtomAnimation.cslist
            // - AcidBubbles.Cornwall.2:/Custom/Scripts/AcidBubbles/Cornwall/Cornwall.cs
            _labelJSON.val = ComputeLabel(val);
            onChange.Invoke();
        };
        _pluginJSON.fileBrowseButton = button.button;
    }

    private string ComputeLabel(string val)
    {
        var varMatch = _varPattern.Match(val);
        if (varMatch.Success) return _sanitizePattern.Replace(varMatch.Groups[1].Value, "_");
        var pathMatch = _pathPattern.Match(val);
        if (pathMatch.Success) return _sanitizePattern.Replace(pathMatch.Groups[1].Value, "_");
        return $"?{val}";
    }

    public JSONStorableAction CreateBinding()
    {
        return new JSONStorableAction(_labelJSON.val, Invoke);
    }

    private void Invoke()
    {
        SuperController.LogMessage("Adding " + _pluginJSON.val);
    }

    public JSONNode GetJSON()
    {
        var jc = new JSONClass();
        _labelJSON.StoreJSON(jc);
        _pluginJSON.StoreJSON(jc);
        return jc;
    }

    public void RestoreFromJSON(JSONClass jc)
    {
        _labelJSON.RestoreFromJSON(jc);
        _pluginJSON.RestoreFromJSON(jc);
    }
}
