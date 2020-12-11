using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Commands : MVRScript, ICommandsProvider
{
    private readonly List<object> _actions = new List<object>();

    public override void Init()
    {
        if (containingAtom.type != "SessionPluginManager")
        {
            SuperController.LogError("Shortcuts plugin can only be installed as a session plugin.");
            CreateTextField(new JSONStorableString("Error", "Shortcuts plugin can only be installed as a session plugin."));
            enabledJSON.val = false;
            return;
        }

        _actions.Clear();

        CreateTextField(new JSONStorableString(
            "Description",
            "This plugin is used for bindings. It offers additional shortcuts not otherwise available using Virt-A-Mate triggers."
        ));

        // Logging
        CreateActionWithParam("LogMessage", SuperController.LogMessage);
        CreateAction(CommandNames.ClearMessageLog, SuperController.singleton.ClearMessages);
        CreateActionWithParam("LogError", SuperController.LogError);
        CreateAction(CommandNames.ClearErrorLog, SuperController.singleton.ClearErrors);

        // Main menu
        CreateAction(CommandNames.SaveScene, SuperController.singleton.SaveSceneDialog);
        CreateAction(CommandNames.LoadScene, SuperController.singleton.LoadSceneDialog);
        CreateAction(CommandNames.MergeLoadScene, SuperController.singleton.LoadMergeSceneDialog);
        CreateAction(CommandNames.Exit, SuperController.singleton.Quit);
        CreateAction(CommandNames.ScreenshotMode, SuperController.singleton.SelectModeScreenshot);
        CreateAction(CommandNames.OnlineBrowser, () => SuperController.singleton.activeUI = SuperController.ActiveUI.OnlineBrowser);
        CreateAction(CommandNames.MainMenu, () => SuperController.singleton.activeUI = SuperController.ActiveUI.MainMenu);
        CreateAction(CommandNames.ToggleErrorLog, ToggleErrorLog);
        CreateAction(CommandNames.ToggleMessageLog, ToggleMessageLog);
        CreateAction(CommandNames.CloseCurrentPanel, () => SuperController.singleton.activeUI = SuperController.ActiveUI.None);
        CreateAction(CommandNames.AddAtom, () => SuperController.singleton.SetMainMenuTab("TabAddAtom"));

        // Tabs
        // TODO: Add all known tabs and filters for Person tabs
        CreateAction(CommandNames.OpenControlTab, () => OpenTab("Control"));
        CreateAction(CommandNames.OpenPluginsTab, () => OpenTab("Plugins"));
        // TODO: Automatically focus on the search fields for clothing tab
        // TODO: Add links to the plugins by #, e.g. OpenPluginsTabPlugin1CustomUI

        // Selection
        CreateActionWithChoice("SelectAtom",
            val => SuperController.singleton.SelectController(SuperController.singleton.GetAtomByUid(val)
                .freeControllers[0]),
            () => SuperController.singleton.GetAtomUIDs()
        );

        // Broadcast
        SuperController.singleton.BroadcastMessage(nameof(IActionsInvoker.OnActionsProviderAvailable), this, SendMessageOptions.DontRequireReceiver);
    }

    public void OnDestroy()
    {
        SuperController.singleton.BroadcastMessage(nameof(IActionsInvoker.OnActionsProviderDestroyed), this, SendMessageOptions.DontRequireReceiver);
    }

    private void CreateAction(string jsaName, JSONStorableAction.ActionCallback fn)
    {
        var jsa = new JSONStorableAction(jsaName, fn);
        RegisterAction(jsa);
        _actions.Add(jsa);
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

    public void OnBindingsListRequested(ICollection<object> bindings)
    {
        foreach (var action in _actions)
        {
            bindings.Add(action);
        }
    }

    private static void ToggleMessageLog()
    {
        if (SuperController.singleton.msgLogPanel.gameObject.activeSelf)
            SuperController.singleton.CloseMessageLogPanel();
        else
            SuperController.singleton.OpenMessageLogPanel();
    }

    private static void ToggleErrorLog()
    {
        if (SuperController.singleton.errorLogPanel.gameObject.activeSelf)
            SuperController.singleton.CloseErrorLogPanel();
        else
            SuperController.singleton.OpenErrorLogPanel();
    }

    private static void OpenTab(string name)
    {
        var selectedAtom = SuperController.singleton.GetSelectedAtom();
        // TODO: Remember last selected atom
        if (selectedAtom == null)
        {
            selectedAtom = SuperController.singleton.GetAtoms().FirstOrDefault();
            if (selectedAtom == null) return;
            SuperController.singleton.SelectController(selectedAtom?.mainController);
        }

        var selector = selectedAtom.gameObject.GetComponentInChildren<UITabSelector>();
        selector?.SetActiveTab(name);
    }
}
