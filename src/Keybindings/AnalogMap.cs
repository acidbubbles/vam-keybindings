﻿using SimpleJSON;
using UnityEngine;

public class AnalogMap :  IMap
{
    public KeyChord chord { get; set; }
    public string axisName { get; set; }
    public string commandName { get; set; }
    public bool isActive { get; set; }
    public int slot { get; set; }

    public AnalogMap()
    {
    }

    public AnalogMap(KeyChord chord, string axisName, string commandName, int slot)
    {
        this.chord = chord;
        this.axisName = axisName;
        this.commandName = commandName;
        this.slot = slot;
    }

    public string GetPrettyString()
    {
        return $"{chord}{(chord.key == KeyCode.None ? "" : "+")}{axisName}";
    }

    public JSONClass GetJSON()
    {
        return new JSONClass
        {
            {"chord", chord.GetJSON()},
            {"axis", axisName},
            {"action", commandName},
            {"slot", slot.ToString()},
        };
    }

    public void RestoreFromJSON(JSONNode mapJSON)
    {
        chord = KeyChord.FromJSON(mapJSON["chord"]);
        axisName = mapJSON["axis"].Value;
        commandName = mapJSON["action"].Value;
        slot = mapJSON["slot"].AsInt;
    }
}
