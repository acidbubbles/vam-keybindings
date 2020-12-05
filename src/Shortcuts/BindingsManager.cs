using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SimpleJSON;
using UnityEngine;

public interface IBindingsManager
{
    void RebuildTree();
}

public class BindingsManager : IBindingsManager
{
    private static readonly Regex _keysParserRegex = new Regex(@"(<.+?>)|.", RegexOptions.Compiled);
    public List<BindingMap> maps { get; } = new List<BindingMap>();
    public BindingTreeNode root { get; } = new BindingTreeNode();

    public void RebuildTree()
    {
        root.Clear();

        foreach (var map in maps)
        {
            if (string.IsNullOrEmpty(map.keys)) return;
            var matches = _keysParserRegex.Matches(map.keys);
            if (matches.Count == 0) return;
            foreach (Match match in matches)
            {
                var keyString = match.Value;
                if (keyString.Length == 1)
                {
                    RebuildKey(keyString, KeyCode.None, map);
                    continue;
                }

                if (keyString[0] == '<')
                {
                    if(!keyString.StartsWith("<C-") || keyString.Length != 5)
                        throw new NotImplementedException("Only <C-*> multi-key bindings are supported");
                    RebuildKey(keyString[3].ToString(), KeyCode.LeftControl, map);
                    continue;
                }

                SuperController.LogError($"Not implemented: {keyString} (in {map}");
            }
        }
    }

    private void RebuildKey(string keyString, KeyCode keyModifier, BindingMap map)
    {
        KeyCode keyCode;
        if (!TryGetKeyCode(keyString, out keyCode))
            SuperController.LogError($"Could not parse key to keycode: {keyString} (in {map})");
        if (!TryAdd(keyCode, keyModifier, map.action))
            SuperController.LogError($"Key conflict:{keyString} (in {map})");
    }

    private bool TryAdd(KeyCode keyCode, KeyCode keyModifier, string action)
    {
        if (root.All(x => x.Key != keyCode))
        {
            root.Add(new BindingTreeNode {key = keyCode, modifier = keyModifier, action = action});
            return false;
        }

        return true;
    }

    private static bool TryGetKeyCode(string keyString, out KeyCode keyCode)
    {
        try
        {
            keyCode = (KeyCode) Enum.Parse(typeof(KeyCode), keyString);
            return true;
        }
        catch (Exception exc)
        {
            throw new NotImplementedException($"Key '{keyString}'", exc);
        }
    }

    public JSONClass GetJSON()
    {
        var mapsJSON = new JSONClass();
        foreach (var map in maps)
        {
            mapsJSON.Add(map.GetJSON());
        }
        return mapsJSON;
    }


    public void RestoreFromJSON(JSONClass mapsJSON)
    {
        maps.Clear();
        foreach (JSONNode mapJSON in mapsJSON)
        {
            var map = new BindingMap();
            map.RestoreFromJSON(mapJSON);
            maps.Add(map);
        }

        RebuildTree();
    }
}
