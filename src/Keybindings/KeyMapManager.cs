using System;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;
using UnityEngine.Events;

public interface IKeyMapManager : IDisposable
{
    UnityEvent onChanged { get; }
    List<KeyMap> maps { get; }
    void RebuildTree();
    KeyMap GetMapByName(string name);
}

public class KeyMapManager : IKeyMapManager
{
    public UnityEvent onChanged { get; } = new UnityEvent();
    public List<KeyMap> maps { get; } = new List<KeyMap>();
    public KeyMapTreeNode root { get; } = new KeyMapTreeNode();

    public void RebuildTree()
    {
        root.next.Clear();

        foreach (var map in maps)
        {
            if (map.chords.Length == 0) continue;
            var node = root;
            foreach (var binding in map.chords)
            {
                KeyMapTreeNode next;
                if (node.TryGet(binding, out next))
                {
                    node = next;
                    continue;
                }

                next = new KeyMapTreeNode {keyChord = binding};
                node.next.Add(next);
                node = next;
            }
            node.boundCommandName = map.commandName;
        }
    }

    public KeyMap GetMapByName(string name)
    {
        return maps.FirstOrDefault(m => m.commandName == name);
    }

    // ReSharper disable once UnusedMember.Global
    public void Debug(KeyMapTreeNode node, int indent = 0)
    {
        var indentStr = new string(' ', indent);
        SuperController.LogMessage($"{indentStr}- {node}");
        foreach (var child in node.next)
        {
            Debug(child, indent + 2);
        }
    }


    public JSONArray GetJSON()
    {
        var mapsJSON = new JSONArray();
        foreach (var map in maps)
        {
            mapsJSON.Add(map.GetJSON());
        }
        return mapsJSON;
    }


    public void RestoreFromJSON(JSONNode mapsJSON)
    {
        foreach (JSONNode mapJSON in mapsJSON.AsArray)
        {
            var map = new KeyMap();
            map.RestoreFromJSON(mapJSON);
            maps.Add(map);
        }
        RebuildTree();
        onChanged.Invoke();
    }

    public void Clear()
    {
        maps.Clear();
        RebuildTree();
        onChanged.Invoke();
    }

    public void Dispose()
    {
        onChanged.RemoveAllListeners();
    }
}
