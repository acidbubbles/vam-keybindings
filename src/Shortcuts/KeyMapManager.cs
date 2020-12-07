using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public interface IBindingsManager
{
    List<KeyMap> maps { get; }
    void RebuildTree();
}

public class KeyMapManager : IBindingsManager
{
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
            node.action = map.action;
        }
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
            // TODO: Do not save "default" mappings, and deal with session-level v.s. scene-level
            mapsJSON.Add(map.GetJSON());
        }
        return mapsJSON;
    }


    public void RestoreFromJSON(JSONNode mapsJSON)
    {
        maps.Clear();
        // TODO: Restore defaults and overwrite as needed
        foreach (JSONNode mapJSON in mapsJSON.AsArray)
        {
            var map = new KeyMap();
            map.RestoreFromJSON(mapJSON);
            maps.Add(map);
        }

        RebuildTree();
    }

    public void RestoreDefaults()
    {
        // TODO: Mark them as "default" so they are not saved and not overwritten
        maps.Add(new KeyMap(new[] {new KeyChord(KeyCode.S, KeyCode.LeftControl)}, ActionNames.SaveScene));
        maps.Add(new KeyMap(new[] {new KeyChord(KeyCode.O, KeyCode.LeftControl)}, ActionNames.LoadScene));

        // tests
        maps.Add(new KeyMap(new[] {new KeyChord(KeyCode.Alpha1)}, "print.1"));
        maps.Add(new KeyMap(new[] {new KeyChord(KeyCode.Alpha2)}, "print.2"));
        maps.Add(new KeyMap(new[] {new KeyChord(KeyCode.Alpha3)}, "print.3"));
        maps.Add(new KeyMap(new[] {new KeyChord(KeyCode.Alpha3), new KeyChord(KeyCode.Alpha4)}, "print.3.4"));
        maps.Add(new KeyMap(new[] {new KeyChord(KeyCode.Alpha3), new KeyChord(KeyCode.Alpha5)}, "print.3.5"));
        RebuildTree();
    }
}
