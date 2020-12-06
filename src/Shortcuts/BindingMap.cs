using System.Linq;
using SimpleJSON;

public class BindingMap
{
    public Binding[] bindings { get; set; }
    public string action { get; set; }

    public BindingMap()
    {
    }

    public BindingMap(Binding[] bindings, string action)
    {
        this.bindings = bindings;
        this.action = action;
    }

    public JSONClass GetJSON()
    {
        var bindingsJSON = new JSONArray();
        foreach (var binding in bindings)
        {
            bindingsJSON.Add(binding.GetJSON());
        }
        return new JSONClass
        {
            {"bindings", bindingsJSON},
            {"action", action},
        };
    }

    public void RestoreFromJSON(JSONNode mapJSON)
    {
        bindings = mapJSON["bindings"].AsArray.Childs.Select(Binding.FromJSON).ToArray();
        action = mapJSON["action"].Value;
    }

    public override string ToString()
    {
        return $"map {GetBindingsAsString()} {action}";
    }

    public string GetBindingsAsString()
    {
        return string.Join(", ", bindings.Select(b => b.ToString()).ToArray());
    }

    public bool SameBinding(Binding[] other)
    {
        if (other.Length != bindings.Length) return false;
        for (var i = 0; i < bindings.Length; i++)
        {
            if (!other[i].Equals(bindings[i])) return false;
        }
        return true;
    }
}
