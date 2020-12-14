using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

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

        // Plugins
        CreateActionWithParam("ReloadPluginsByName", ReloadPluginsByName);
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

    private void ReloadPluginsByName(string val)
    {
        var pluginsList = UITransform
            .GetChild(0)
            .Find("Canvas")
            .Find("Panel")
            .Find("Content")
            .Find("Plugins")
            .Find("Scroll View")
            .Find("Viewport")
            .Find("Content");
        var reloadButtons = new List<Button>();
        for (var i = 0; i < pluginsList.childCount; i++)
        {
            var pluginPanel = pluginsList.GetChild(i);
            var pluginPanelContent = pluginPanel.Find("Content");
            for (var j = 0; j < pluginPanelContent.childCount; j++)
            {
                var scriptPanel = pluginPanelContent.GetChild(j);
                var uid = scriptPanel
                    .Find("UID")
                    .GetComponent<Text>()
                    .text;
                if (uid.Contains(val))
                {
                    var reloadButton = pluginPanel.Find("ReloadButton")
                        .GetComponent<Button>();
                    reloadButtons.Add(reloadButton);
                }
            }
        }
        foreach (var reloadButton in reloadButtons)
        {
            reloadButton.onClick.Invoke();
        }
        foreach (var script in containingAtom
            .GetStorableIDs()
            .Select(id => containingAtom.GetStorableByID(id))
            .OfType<MVRScript>()
            .Where(s => s.storeId.Contains(val))
            .Where(s => !ReferenceEquals(s, this)))
        {
            containingAtom.RestoreFromLast(script);
        }
        if (reloadButtons.Count == 0)
            SuperController.LogError($"Shortcuts: Could not find any plugins containing {val} in atoms nor session plugins.");
    }
}
