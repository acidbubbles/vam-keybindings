using SimpleJSON;
using UnityEngine;

public class AnalogMap :  IMap
{
    public bool isAxis { get; set; }
    public KeyChord chord { get; set; }
    public string axisName { get; set; }
    public bool reversed { get; set; }
    public KeyChord leftChord { get; set; }
    public KeyChord rightChord { get; set; }
    public string commandName { get; set; }
    public bool isActive { get; set; }
    public int slot { get; set; }

    public AnalogMap()
    {
    }

    public AnalogMap(KeyChord chord, string axisName, bool reversed, string commandName, int slot = 0)
    {
        isAxis = true;
        leftChord = KeyChord.empty;
        rightChord = KeyChord.empty;
        this.chord = chord;
        this.axisName = axisName;
        this.commandName = commandName;
        this.reversed = reversed;
        this.slot = slot;
    }

    public AnalogMap(KeyChord leftChord, KeyChord rightChord, string commandName, int slot = 0)
    {
        isAxis = false;
        chord = KeyChord.empty;
        axisName = null;
        reversed = false;
        this.leftChord = leftChord;
        this.rightChord = rightChord;
        this.commandName = commandName;
        this.slot = slot;
    }

    public string GetPrettyString()
    {
        if(isAxis)
            return $"{chord}{(chord.key == KeyCode.None ? "" : "+")}{axisName}{(reversed ? " (reverse)" : "")}";
        else
            return $"{leftChord}/{rightChord}";
    }

    public JSONClass GetJSON()
    {
        var jc = new JSONClass
        {
            {"isAxis", isAxis ? "true": "false"},
            {"action", commandName},
            {"slot", slot.ToString()},
        };
        if (isAxis)
        {
            jc["chord"] = chord.GetJSON();
            jc["axis"] = axisName;
            jc["reversed"] = reversed ? "true" : "false";
        }
        else
        {
            jc["leftChord"] = leftChord.GetJSON();
            jc["rightChord"] = rightChord.GetJSON();
        }

        return jc;
    }

    public void RestoreFromJSON(JSONNode mapJSON)
    {
        try
        {
            commandName = mapJSON["action"].Value;
            slot = mapJSON["slot"].AsInt;
            isAxis = mapJSON["isAxis"].Value != "false";

            if (isAxis)
            {
                chord = KeyChord.FromJSON(mapJSON["chord"]);
                axisName = mapJSON["axis"].Value;
                reversed = mapJSON["reversed"].Value == "true";
            }
            else
            {
                leftChord = KeyChord.FromJSON(mapJSON["leftChord"]);
                rightChord = KeyChord.FromJSON(mapJSON["rightChord"]);
            }
        }
        catch (System.Exception e)
        {
            SuperController.LogError($"Keybindings: invalid keybinding for '{commandName}' slot {slot}: {e}");
        }
    }
}
