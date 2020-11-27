using SimpleJSON;
using UnityEngine.Events;

public interface IBoundAction
{
    string type { get; }
    string name { get; }
    string displayName { get; }

    void Validate();
    void SyncAtomNames();
    void Invoke();
    UnityEvent Edit();
    JSONClass GetJSON();
    void RestoreFromJSON(JSONClass json);
}
