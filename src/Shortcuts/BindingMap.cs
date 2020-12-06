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
        return $"map {bindings.GetBindingsAsString()} {action}";
    }
}
