using SimpleJSON;

public interface IBoundAction
{
    string type { get; }

    void Validate();
    void SyncAtomNames();
    void Invoke();
    void Edit();
    JSONClass GetJSON();
    void RestoreFromJSON(JSONClass json);
}
