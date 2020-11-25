using System;
using SimpleJSON;

public class DebugBoundAction : IBoundAction
{
    public const string Type = "debug";
    public string type => Type;

    private string _message;

    public DebugBoundAction()
    {
    }

    public DebugBoundAction(string message)
    {
        _message = message;
    }

    public void Validate()
    {
    }

    public void SyncAtomNames()
    {
    }

    public void Invoke()
    {
        SuperController.LogMessage(_message);
    }

    public void Edit()
    {
    }

    public JSONClass GetJSON()
    {
        return new JSONClass
        {
            {"message", _message}
        };
    }

    public void RestoreFromJSON(JSONClass json)
    {
        _message = json["message"];
    }
}
