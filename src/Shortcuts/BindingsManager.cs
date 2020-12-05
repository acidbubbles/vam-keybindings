using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SimpleJSON;
using UnityEngine;

public interface IBindingsManager
{
}

public class BindingsManager : IBindingsManager
{
    private static readonly Regex _keysParserRegex = new Regex(@"(<.+?>)|.", RegexOptions.Compiled);
    public List<BindingMap> maps { get; } = new List<BindingMap>();
    public BindingTreeNode root { get; } = new BindingTreeNode();

    private void RebuildTree()
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
                    KeyCode keyCode;
                    if (!Enum.TryParse(keyString, out keyCode))
                    {
                        SuperController.LogError($"Could not parse key to keycode: {keyString} (in {map})");
                    }
                    if (root.Any(x => x.Key == keyCode))
                    {
                        SuperController.LogError($"Key conflict:{keyString} (in {map})");
                    }
                    root.Add(new BindingTreeNode {key = keyCode, action = map.action});
                    continue;
                }

                SuperController.LogError($"Not implemented: {keyString} (in {map}");
            }
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
