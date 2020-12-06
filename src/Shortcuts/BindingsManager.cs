using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public interface IBindingsManager
{
}

public class BindingsManager : IBindingsManager
{
    public List<BindingMap> maps { get; } = new List<BindingMap>();
    public BindingTreeNode root { get; } = new BindingTreeNode();

    public void RebuildTree()
    {
        root.next.Clear();

        foreach (var map in maps)
        {
            if (map.bindings.Length == 0) continue;
            var node = root;
            foreach (var binding in map.bindings)
            {
                BindingTreeNode next;
                if (node.TryGet(binding, out next))
                {
                    node = next;
                    continue;
                }

                next = new BindingTreeNode {binding = binding};
                node.next.Add(next);
                node = next;
            }
            node.action = map.action;
        }
    }

    // ReSharper disable once UnusedMember.Global
    public void Debug(BindingTreeNode node, int indent = 0)
    {
        var indentStr = new string(' ', indent);
        SuperController.LogMessage($"{indentStr}- {node}");
        foreach (var child in node.next)
        {
            Debug(child, indent + 2);
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

    public void RestoreDefaults()
    {
        maps.Add(new BindingMap(new[] {new Binding(KeyCode.S, KeyCode.LeftControl)}, ActionNames.SaveScene));
        maps.Add(new BindingMap(new[] {new Binding(KeyCode.O, KeyCode.LeftControl)}, ActionNames.LoadScene));

        // tests
        maps.Add(new BindingMap(new[] {new Binding(KeyCode.Alpha1)}, "print.1"));
        maps.Add(new BindingMap(new[] {new Binding(KeyCode.Alpha2)}, "print.2"));
        maps.Add(new BindingMap(new[] {new Binding(KeyCode.Alpha3)}, "print.3"));
        maps.Add(new BindingMap(new[] {new Binding(KeyCode.Alpha3), new Binding(KeyCode.Alpha4)}, "print.3.4"));
        maps.Add(new BindingMap(new[] {new Binding(KeyCode.Alpha3), new Binding(KeyCode.Alpha5)}, "print.3.5"));
        RebuildTree();
    }
}
