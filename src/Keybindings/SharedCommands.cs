using System;
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

        // Selected Tabs
        // TODO: Add all known tabs
        CreateAction("OpenControlTab", () => OpenTab(type => type == "Person" ? "ControlAndPhysics1" : "Control"));
        CreateAction("OpenPluginsTab", () => OpenTab(_ => "Plugins"));
        CreateAction("OpenClothingTab", () => OpenTab(_ => "Clothing", "Person"));
        CreateAction("OpenHairTab", () => OpenTab(_ => "Hair", "Person"));
        // TODO: Automatically focus on the search fields for clothing tab
        // TODO: Add links to the plugins by #, e.g. OpenPluginsTabPlugin1CustomUI

        // Main Tabs
        CreateAction("OpenMainMenu", () => OpenMainTab(null));
        CreateAction("OpenMainMenuFileTab", () => OpenMainTab("TabFile"));
        CreateAction("OpenMainMenuUserPrefsTab", () => OpenMainTab("TabUserPrefs"));
        CreateAction("OpenMainMenuNavigationTab", () => OpenMainTab("TabNavigation"));
        CreateAction("OpenMainMenuSelectTab", () => OpenMainTab("TabSelect"));
        CreateAction("OpenMainMenuSessionPluginPresetsTab", () => OpenMainTab("TabSessionPluginPresets"));
        CreateAction("OpenMainMenuSessionPluginsTab", () => OpenMainTab("TabSessionPlugins"));
        CreateAction("OpenMainMenuScenePluginsTab", () => OpenMainTab("TabScenePlugins"));
        CreateAction("OpenMainMenuScenePluginPresetsTab", () => OpenMainTab("TabScenePluginPresets"));
        CreateAction("OpenMainMenuSceneLightingTab", () => OpenMainTab("TabSceneLighting"));
        CreateAction("OpenMainMenuSceneMiscTab", () => OpenMainTab("TabSceneMisc"));
        CreateAction("OpenMainMenuSceneAnimationTab", () => OpenMainTab("TabSceneAnimation"));
        CreateAction("OpenMainMenuAddAtomTab", () => OpenMainTab("TabAddAtom"));
        CreateAction("OpenMainMenuAudioTab", () => OpenMainTab("TabAudio"));
        // CreateAction("OpenMainMenuDebugTab", () => OpenMainTab("TabDebug"));

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

    private static void OpenMainTab(string tabName)
    {
        SuperController.singleton.SetActiveUI("MainMenu");
        if(tabName != null)
        SuperController.singleton.SetMainMenuTab(tabName);
    }

    private static void OpenTab(Func<string, string> getTabName, string type = null)
    {
        var selectedAtom = SuperController.singleton.GetSelectedAtom();
        // TODO: Remember last selected atom
        if (selectedAtom == null || (type != null && selectedAtom.type != type))
        {
            if (type == null)
                selectedAtom = SuperController.singleton.GetAtoms().FirstOrDefault(a => a.type == "Person") ?? SuperController.singleton.GetAtoms().FirstOrDefault();
            else
                selectedAtom = SuperController.singleton.GetAtoms().FirstOrDefault(a => a.type == type);
            if (selectedAtom == null) return;
        }

        var tabName = getTabName(selectedAtom.type);
        if (tabName == null) return;

        var selector = selectedAtom.gameObject.GetComponentInChildren<UITabSelector>();
        if (selector == null) return;

        SuperController.singleton.SetActiveUI("SelectedOptions");
        SuperController.singleton.SelectController(selectedAtom.mainController);
        selector.SetActiveTab(getTabName(selectedAtom.type));
    }
}
