using System;
using SimpleJSON;
using UnityEngine.Events;

public interface IBoundAction
{
    string type { get; }
    string name { get; }
    string displayName { get; }
    object bindable { get; }

    void Validate();
    void SyncAtomNames();
    void Invoke();
    UnityEvent Edit();
    JSONClass GetJSON();
    void RestoreFromJSON(JSONClass json);
}
