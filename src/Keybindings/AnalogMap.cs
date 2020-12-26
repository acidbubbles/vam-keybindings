using SimpleJSON;

public class AnalogMap
{
    public KeyChord chord { get; set; }
    public string axisName { get; set; }
    public string commandName { get; set; }

    public JSONClass GetJSON()
    {
        return new JSONClass
        {
            {"chord", chord.GetJSON()},
            {"axis", axisName},
            {"action", commandName},
        };
    }

    public void RestoreFromJSON(JSONNode mapJSON)
    {
        chord = KeyChord.FromJSON(mapJSON["chord"]);
        axisName = mapJSON["axis"].Value;
        commandName = mapJSON["action"].Value;
    }
}
