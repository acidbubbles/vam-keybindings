using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SharedCommands : MVRScript, ICommandsProvider
{
    private readonly List<JSONStorableAction> _commands = new List<JSONStorableAction>();

    public override void Init()
    {
        if (containingAtom.type != "SessionPluginManager")
        {
            SuperController.LogError("Shared commands plugin can only be installed as a session plugin.");
            CreateTextField(new JSONStorableString("Error", "Shared commands plugin can only be installed as a session plugin."));
            enabledJSON.val = false;
            return;
        }

        _commands.Clear();

        CreateTextField(new JSONStorableString(
            "Description",
            "This plugin implements the different commands you can bind shortcuts to. Customize keybindings in the Keybindings plugin."
        ));

        // Logging
        CreateAction("ClearMessageLog", SuperController.singleton.ClearMessages);
        CreateAction("ClearErrorLog", SuperController.singleton.ClearErrors);

        // Main menu
        CreateAction("SaveScene", SuperController.singleton.SaveSceneDialog);
        CreateAction("LoadScene", SuperController.singleton.LoadSceneDialog);
        CreateAction("MergeLoadScene", SuperController.singleton.LoadMergeSceneDialog);
        CreateAction("Exit", SuperController.singleton.Quit);
        CreateAction("ScreenshotMode", SuperController.singleton.SelectModeScreenshot);
        CreateAction("OnlineBrowser", () => SuperController.singleton.activeUI = SuperController.ActiveUI.OnlineBrowser);
        CreateAction("MainMenu", () => SuperController.singleton.activeUI = SuperController.ActiveUI.MainMenu);
        CreateAction("ToggleErrorLog", ToggleErrorLog);
        CreateAction("ToggleMessageLog", ToggleMessageLog);
        CreateAction("CloseAllPanels", CloseAllPanels);
        CreateAction("AddAtom", () => SuperController.singleton.SetMainMenuTab("TabAddAtom"));

        // Tabs
        // TODO: Add all known tabs and filters for Person tabs
        CreateAction("OpenControlTab", () => OpenTab("Control"));
        CreateAction("OpenPluginsTab", () => OpenTab("Plugins"));
        // TODO: Automatically focus on the search fields for clothing tab
        // TODO: Add links to the plugins by #, e.g. OpenPluginsTabPlugin1CustomUI

        // Selection
        CreateAction("Deselect", () => SuperController.singleton.SelectController(null));


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
        _commands.Add(jsa);
    }

    public void OnBindingsListRequested(ICollection<object> bindings)
    {
        foreach (var action in _commands)
        {
            bindings.Add(action);
        }
    }

    private static void CloseAllPanels()
    {
        SuperController.singleton.activeUI = SuperController.ActiveUI.None;
        SuperController.singleton.CloseMessageLogPanel();
        SuperController.singleton.CloseErrorLogPanel();
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
