﻿using SimpleJSON;
using UnityEngine.Events;

public class DebugCommand : ICustomCommand
{
    public const string Type = "debug";
    public string type => Type;
    public string name { get; private set; }
    public object bindable => null;
    public string displayName => $"<b>DEBUG</b> <i>{name}</i> '{_message}'";

    private string _message;

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

    public UnityEvent Edit()
    {
        return null;
    }

    public JSONClass GetJSON()
    {
        return new JSONClass
        {
            {"message", _message},
            {"guid", name},
        };
    }

    public void RestoreFromJSON(JSONClass json)
    {
        name = json["guid"];
        _message = json["message"];
    }
}
