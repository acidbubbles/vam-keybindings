using SimpleJSON;
using UnityEngine;

public class AnalogMap :  IMap
{
    public KeyChord chord { get; set; }
    public string axisName { get; set; }
    public bool reversed { get; set; }
    public string commandName { get; set; }
    public bool isActive { get; set; }
    public int slot { get; set; }

    public AnalogMap()
    {
    }

    public AnalogMap(KeyChord chord, string axisName, bool reversed, string commandName, int slot)
    {
        this.chord = chord;
        this.axisName = axisName;
        this.commandName = commandName;
        this.reversed = reversed;
        this.slot = slot;
    }

    public string GetPrettyString()
    {
        return $"{chord}{(chord.key == KeyCode.None ? "" : "+")}{axisName}{(reversed ? " (reverse)" : "")}";
    }

    public JSONClass GetJSON()
    {
        return new JSONClass
        {
            {"chord", chord.GetJSON()},
            {"axis", axisName},
            {"reversed", reversed ? "true" : "false"},
            {"action", commandName},
            {"slot", slot.ToString()},
        };
    }

    public void RestoreFromJSON(JSONNode mapJSON)
    {
        chord = KeyChord.FromJSON(mapJSON["chord"]);
        axisName = mapJSON["axis"].Value;
        reversed = mapJSON["axis"].Value == "true";
        commandName = mapJSON["action"].Value;
        slot = mapJSON["slot"].AsInt;
    }
}
