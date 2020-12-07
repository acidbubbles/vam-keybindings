using System.Linq;
using SimpleJSON;

public class KeyMap
{
    public KeyChord[] chords { get; set; }
    public string action { get; set; }

    public KeyMap()
    {
    }

    public KeyMap(KeyChord[] chords, string action)
    {
        this.chords = chords;
        this.action = action;
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
            {"action", action},
        };
    }

    public void RestoreFromJSON(JSONNode mapJSON)
    {
        chords = mapJSON["chords"].AsArray.Childs.Select(KeyChord.FromJSON).ToArray();
        action = mapJSON["action"].Value;
    }

    public override string ToString()
    {
        return $"map {chords.GetKeyChordsAsString()} {action}";
    }
}
