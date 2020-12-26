using System.Collections.Generic;
using SimpleJSON;

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

public class AnalogMap
{
    public string axisName { get; set; }
    public string commandName { get; set; }

    public JSONClass GetJSON()
    {
        return new JSONClass
        {
            {"axis", axisName},
            {"action", commandName},
        };
    }

    public void RestoreFromJSON(JSONNode mapJSON)
    {
        axisName = mapJSON["axis"].Value;
        commandName = mapJSON["action"].Value;
    }
}
