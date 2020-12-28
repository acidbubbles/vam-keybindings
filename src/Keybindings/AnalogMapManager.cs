using System;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;
using UnityEngine.Events;

public interface IAnalogMapManager : IDisposable
{
    UnityEvent onChanged { get; }
    List<AnalogMap> maps { get; }
    JSONArray GetJSON();
    void RestoreFromJSON(JSONNode mapsJSON);
    void Clear();
    AnalogMap GetMapByName(string commandName);
}

public class AnalogMapManager : IAnalogMapManager
{
    public UnityEvent onChanged { get; } = new UnityEvent();
    public List<AnalogMap> maps { get; } = new List<AnalogMap>();

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
            var map = new AnalogMap();
            map.RestoreFromJSON(mapJSON);
            maps.Add(map);
        }
        onChanged.Invoke();
    }

    public void Clear()
    {
        maps.Clear();
        onChanged.Invoke();
    }

    public AnalogMap GetMapByName(string commandName)
    {
        return maps.FirstOrDefault(m => m.commandName == commandName);
    }

    public void Dispose()
    {
        onChanged.RemoveAllListeners();
    }
}
