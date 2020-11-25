using System;

public class BindableActions : MVRScript
{
    public override void Init()
    {
        CreateTextField(new JSONStorableString("Description", "This plugin is used for bindings. It offers additional shortcuts not otherwise available using Virt-A-Mate triggers."));

        // Logging
        CreateActionWithParam("LogMessage", SuperController.LogMessage);
        CreateAction("ClearMessageLog", SuperController.singleton.ClearMessages);
        CreateActionWithParam("LogError", SuperController.LogError);
        CreateAction("ClearErrorLog", SuperController.singleton.ClearErrors);

        // Main menu
        CreateAction("SaveScene", SuperController.singleton.SaveSceneDialog);
        CreateAction("LoadScene", SuperController.singleton.LoadSceneDialog);
        CreateAction("Exit", SuperController.singleton.Quit);

        // Selection
        CreateActionWithParam("SelectAtom", val =>
        {
            var atom = SuperController.singleton.GetAtomByUid(val);
            if (atom == null) return;
            SuperController.singleton.SelectController(atom.freeControllers[0]);
        });
    }

    private void CreateAction(string jsaName, JSONStorableAction.ActionCallback fn)
    {
        var jsa = new JSONStorableAction(jsaName, fn);
        RegisterAction(jsa);
    }

    private void CreateActionWithParam(string jssName, Action<string> fn)
    {
        var jss = new JSONStorableString(jssName, null)
        {
            isStorable = false,
            isRestorable = false
        };
        RegisterString(jss);
        jss.setCallbackFunction = val =>
        {
            fn(val);
            jss.valNoCallback = null;
        };
    }
}
