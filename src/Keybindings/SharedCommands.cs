using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SharedCommands : MVRScript, ICommandsProvider
{
    private readonly List<JSONStorableAction> _commands = new List<JSONStorableAction>();
    private SelectionHistoryManager _selectionManager;
    // ReSharper disable once Unity.NoNullCoalescing
    private ISelectionHistoryManager selectionManager => _selectionManager ?? (_selectionManager = transform.parent.GetComponentInChildren<SelectionHistoryManager>() ?? gameObject.AddComponent<SelectionHistoryManager>());

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

        // Mode
        CreateAction("ChangeGameModePlayMode", () => SuperController.singleton.gameMode = SuperController.GameMode.Play);
        CreateAction("ChangeGameModeEditMode", () => SuperController.singleton.gameMode = SuperController.GameMode.Edit);

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
        // Common
        CreateAction("OpenAtomMenu", () => SuperController.singleton.SetActiveUI("SelectedOptions"));
        CreateAction("OpenAtomControlTab", () => OpenTab(type => type == "Person" ? "ControlAndPhysics1" : "Control"));
        CreateAction("OpenAtomPresetTab", () => OpenTab(_ => "Preset"));
        CreateAction("OpenAtomMoveTab", () => OpenTab(_ => "Move"));
        CreateAction("OpenAtomAnimationTab", () => OpenTab(_ => "Animation"));
        CreateAction("OpenAtomPhysicsControlTab", () => OpenTab(_ => "Physics Control"));
        CreateAction("OpenAtomPhysicsObjectTab", () => OpenTab(_ => "Physics Object"));
        CreateAction("OpenAtomCollisionTriggerTab", () => OpenTab(_ => "Collision Trigger"));
        CreateAction("OpenAtomMaterialTab", () => OpenTab(_ => "Material"));
        CreateAction("OpenAtomPluginsTab", () => OpenTab(_ => "Plugins"));
        // Animation Pattern
        CreateAction("OpenAnimationPatternAtomAnimationPatternTab", () => OpenTab(_ => "Animation Pattern", "AnimationPattern"));
        CreateAction("OpenAnimationPatternAtomAnimationTriggersTab", () => OpenTab(_ => "Animation Triggers", "AnimationPattern"));
        // Custom Unity Asset
        CreateAction("OpenCustomUnityAssetAtomAssetTab", () => OpenTab(_ => "Asset", "CustomUnityAsset"));
        // Audio Source
        CreateAction("OpenAudioSourceAtomAudioSourceTab", () => OpenTab(_ => "Audio Source", "AudioSource"));
        // Person
        CreateAction("OpenPersonAtomClothingTab", () => OpenTab(_ => "Clothing", "Person"));
        CreateAction("OpenPersonAtomClothingTab", () => OpenTab(_ => "Clothing", "Person"));
        CreateAction("OpenPersonAtomHairTab", () => OpenTab(_ => "Hair", "Person"));
        CreateAction("OpenPersonAtomAppearancePresets", () => OpenTab(_ => "Appearance Presets", "Person"));
        CreateAction("OpenPersonAtomGeneralPresets", () => OpenTab(_ => "General Presets", "Person"));
        CreateAction("OpenPersonAtomPosePresets", () => OpenTab(_ => "Pose Presets", "Person"));
        CreateAction("OpenPersonAtomSkinPresets", () => OpenTab(_ => "Skin Presets", "Person"));
        CreateAction("OpenPersonAtomPluginsPresets", () => OpenTab(_ => "Plugins Presets", "Person"));
        CreateAction("OpenPersonAtomMorphsPresets", () => OpenTab(_ => "Morphs Presets", "Person"));
        CreateAction("OpenPersonAtomHairPresets", () => OpenTab(_ => "Hair Presets", "Person"));
        CreateAction("OpenPersonAtomClothingPresets", () => OpenTab(_ => "Clothing Presets", "Person"));
        CreateAction("OpenPersonAtomMaleMorphs", () => OpenTab(_ => "Male Morphs", "Person"));
        CreateAction("OpenPersonAtomFemaleMorphs", () => OpenTab(_ => "Female Morphs", "Person"));
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
        // TODO: Next/Previous tab

        // Selection
        CreateAction("DeselectAtom", () => SuperController.singleton.SelectController(null));
        CreateAction("SelectPreviousAtom", SelectPrevious);
        // TODO: LastSelected, oSelectionHistoryBack and Forward

        // TODO: Special: Reload a specific plugin and re-open the tab?
        // Dev
        CreateAction("ReloadKeybindingsPlugin", () => Reload());

        // TODO: Add atom types (AddAtomPerson, AddAtomCube, etc.)

        // Broadcast
        SuperController.singleton.BroadcastMessage(nameof(IActionsInvoker.OnActionsProviderAvailable), this, SendMessageOptions.DontRequireReceiver);
    }

    public void OnDestroy()
    {
        if(_selectionManager != null && _selectionManager.gameObject == gameObject) Destroy(_selectionManager);
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

    private void Reload()
    {
        var pluginsList = SuperController.singleton.mainHUD
            .Find("MainUICanvas")
            .Find("Panel")
            .Find("Content")
            .Find("TabSessionPlugins")
            .Find("Scroll View")
            .Find("Viewport")
            .Find("Content");
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
                if (uid == storeId)
                {
                    var reloadButton = pluginPanel.Find("ReloadButton")
                        .GetComponent<Button>();
                    reloadButton.onClick.Invoke();
                    return;
                }
            }
        }
        SuperController.LogError($"Shortcuts: Could not find plugin {storeId} in the session plugin panel.");
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
        if (tabName != null)
            SuperController.singleton.SetMainMenuTab(tabName);
    }

    private void OpenTab(Func<string, string> getTabName, string type = null)
    {
        var selectedAtom = _selectionManager.GetLastSelectedAtomOfType(type);
        if (ReferenceEquals(selectedAtom, null)) return;

        var tabName = getTabName(selectedAtom.type);
        if (tabName == null) return;

        var selector = selectedAtom.gameObject.GetComponentInChildren<UITabSelector>(true);
        if (selector == null) return;

        SuperController.singleton.SelectController(selectedAtom.mainController);
        SuperController.singleton.SetActiveUI("SelectedOptions");
        /*
        foreach (Transform t in selector.toggleContainer)
            SuperController.LogMessage(t.name);
        */
        selector.SetActiveTab(getTabName(selectedAtom.type));
    }

    private static Atom GetAtomOfType(string type)
    {
        var selectedAtom = SuperController.singleton.GetSelectedAtom();
        // TODO: Remember last selected atom
        if (selectedAtom != null && (type == null || selectedAtom.type == type))
            return null;

        if (type == null)
            selectedAtom = SuperController.singleton.GetAtoms().FirstOrDefault(a => a.type == "Person") ?? SuperController.singleton.GetAtoms().FirstOrDefault();
        else
            selectedAtom = SuperController.singleton.GetAtoms().FirstOrDefault(a => a.type == type);

        return selectedAtom;
    }

    private void SelectPrevious()
    {
        var selectedController = SuperController.singleton.GetSelectedController();
        var mainController = SuperController.singleton.GetSelectedAtom()?.mainController;
        if (selectedController != mainController)
            SuperController.singleton.SelectController(mainController);
        else if (selectionManager.history.Count > 1)
            SuperController.singleton.SelectController(selectionManager.history[selectionManager.history.Count - 2].mainController);
    }
}
