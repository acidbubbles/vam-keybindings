using System;
using SimpleJSON;

public interface IKeybindingsSettings
{
    JSONStorableBool showKeyPressesJSON { get; }
    JSONStorableFloat mouseSensitivityJSON { get; }
    void RestoreFromJSON(JSONClass jc);
    JSONClass GetJSON();
}

public class KeybindingsSettings : IKeybindingsSettings
{
    public JSONStorableBool showKeyPressesJSON { get; } = new JSONStorableBool("ShowKeypresses", false);
    public JSONStorableFloat mouseSensitivityJSON { get; } = new JSONStorableFloat("MouseSensitivity", 0.7f, 0f, 1f);
    public void RestoreFromJSON(JSONClass jc)
    {
        showKeyPressesJSON.RestoreFromJSON(jc);
        mouseSensitivityJSON.RestoreFromJSON(jc);
    }

    public JSONClass GetJSON()
    {
        var jc = new JSONClass();
        showKeyPressesJSON.StoreJSON(jc);
        mouseSensitivityJSON.StoreJSON(jc);
        return jc;
    }
}
