using System.Linq;
using SimpleJSON;

public class KeyMap : IMap
{
    public KeyChord[] chords { get; set; }
    public string commandName { get; set; }

    public KeyMap()
    {
    }

    public KeyMap(KeyChord[] chords, string commandName)
    {
        this.chords = chords;
        this.commandName = commandName;
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
        };
    }

    public void RestoreFromJSON(JSONNode mapJSON)
    {
        chords = mapJSON["chords"].AsArray.Childs.Select(KeyChord.FromJSON).ToArray();
        commandName = mapJSON["action"].Value;
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
