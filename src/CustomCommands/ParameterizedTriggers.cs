using System;
using System.Collections.Generic;

public class ParameterizedTriggers : MVRScript
{
    public override void Init()
    {
        CreateTextField(new JSONStorableString(
            "Description",
            $"Use the {nameof(CustomCommands)} plugin to create custom commands. This plugin is used to offer additional storables that can be invoked using the custom triggers."
        ));

        // Logging
        CreateActionWithParam("LogMessage", SuperController.LogMessage);
        CreateActionWithParam("LogError", SuperController.LogError);

        // Selection
        CreateAction("SelectThis",
            () => SuperController.singleton.SelectController(containingAtom.mainController)
        );
        CreateActionWithChoice("SelectAtom",
            val => SuperController.singleton.SelectController(SuperController.singleton.GetAtomByUid(val).mainController),
            () => SuperController.singleton.GetAtomUIDs()
        );
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

    private void CreateActionWithChoice(string jssName, Action<string> fn, Func<List<string>> genChoices)
    {
        var choices = genChoices();
        var jss = new JSONStorableStringChooser(jssName, choices, null, jssName)
        {
            isStorable = false,
            isRestorable = false
        };
        RegisterStringChooser(jss);
        jss.setCallbackFunction = val =>
        {
            fn(val);
            jss.valNoCallback = null;
        };
        jss.popupOpenCallback += () => jss.choices = genChoices();
    }
}
