using SimpleJSON;

public class BindingMap
{
    public string keys { get; set; }
    public string action { get; set; }

    public JSONClass GetJSON()
    {
        return new JSONClass
        {
            {"keys", keys},
            {"action", action},
        };
    }

    public void RestoreFromJSON(JSONNode mapJSON)
    {
        keys = mapJSON["keys"].Value;
        action = mapJSON["action"].Value;
    }

    public override string ToString()
    {
        return $"map {keys} {action}";
    }
}
