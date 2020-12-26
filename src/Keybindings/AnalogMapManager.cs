using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public interface IAnalogMapManager
{
    List<AnalogMap> maps { get; }
    JSONArray GetJSON();
    void RestoreFromJSON(JSONNode mapsJSON);
    void Clear();
}

public class AnalogMapManager : IAnalogMapManager
{
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
        // TEMP
        // maps.Add(new AnalogMap
        // {
        //     axisName = "Mouse X",
        //     chord = new KeyChord(KeyCode.None, true, false, false),
        //     commandName = "test"
        // });
        //
        // maps.Add(new AnalogMap
        // {
        //     axisName = "LeftStickX",
        //     chord = new KeyChord(KeyCode.None, false, false, false),
        //     commandName = "test"
        // });

        foreach (JSONNode mapJSON in mapsJSON.AsArray)
        {
            var map = new AnalogMap();
            map.RestoreFromJSON(mapJSON);
            maps.Add(map);
        }
    }

    public void Clear()
    {
        maps.Clear();
    }
}
