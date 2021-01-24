using System.Linq;
using SimpleJSON;

public class KeyMap : IMap
{
    public KeyChord[] chords { get; set; }
    public string commandName { get; set; }
    public int slot { get; set; }

    public KeyMap()
    {
    }

    public KeyMap(KeyChord[] chords, string commandName, int slot = 0)
    {
        this.chords = chords;
        this.commandName = commandName;
        this.slot = slot;
    }

    public JSONClass GetJSON()
    {
        var chordsJSON = new JSONArray();
        foreach (var chord in chords)
        {
            chordsJSON.Add(chord.GetJSON());
        }

        return new JSONClass
        {
            {"chords", chordsJSON},
            {"action", commandName},
            {"slot", slot.ToString()}
        };
    }

    public void RestoreFromJSON(JSONNode mapJSON)
    {
        chords = mapJSON["chords"].AsArray.Childs.Select(KeyChord.FromJSON).ToArray();
        commandName = mapJSON["action"].Value;
        slot = mapJSON["slot"].AsInt;
    }

    public string GetPrettyString()
    {
        return chords.GetKeyChordsAsString();
    }

    public override string ToString()
    {
        return $"map {chords.GetKeyChordsAsString()} {commandName}";
    }
}
