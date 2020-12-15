using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using UnityEngine;

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
        CreateAction("Clear_MessageLog", SuperController.singleton.ClearMessages);
        CreateAction("Clear_ErrorLog", SuperController.singleton.ClearErrors);

        // Mode
        CreateAction("Change_GameMode_PlayMode", () => SuperController.singleton.gameMode = SuperController.GameMode.Play);
        CreateAction("Change_GameMode_EditMode", () => SuperController.singleton.gameMode = SuperController.GameMode.Edit);

        // Main menu
        CreateAction("SaveScene", SuperController.singleton.SaveSceneDialog);
        CreateAction("LoadScene", SuperController.singleton.LoadSceneDialog);
        CreateAction("MergeLoadScene", SuperController.singleton.LoadMergeSceneDialog);
        CreateAction("Exit", SuperController.singleton.Quit);
        CreateAction("ScreenshotMode", SuperController.singleton.SelectModeScreenshot);
        CreateAction("Open_OnlineBrowser", () => SuperController.singleton.activeUI = SuperController.ActiveUI.OnlineBrowser);
        CreateAction("Open_MainMenu", () => SuperController.singleton.activeUI = SuperController.ActiveUI.MainMenu);
        CreateAction("Toggle_ErrorLog", ToggleErrorLog);
        CreateAction("Toggle_MessageLog", ToggleMessageLog);
        CreateAction("Close_AllPanels", CloseAllPanels);
        CreateAction("Toggle_ShowHiddenAtoms", SuperController.singleton.ToggleShowHiddenAtoms);

        // Selected Tabs
        // Common
        CreateAction("Open_Atom_Menu", () => SuperController.singleton.SetActiveUI("SelectedOptions"));
        CreateAction("Open_Atom_ControlTab", () => OpenTab(type => type == "Person" ? "ControlAndPhysics1" : "Control"));
        CreateAction("Open_Atom_PresetTab", () => OpenTab(_ => "Preset"));
        CreateAction("Open_Atom_MoveTab", () => OpenTab(_ => "Move"));
        CreateAction("Open_Atom_AnimationTab", () => OpenTab(_ => "Animation"));
        CreateAction("Open_Atom_PhysicsControlTab", () => OpenTab(_ => "Physics Control"));
        CreateAction("Open_Atom_PhysicsObjectTab", () => OpenTab(_ => "Physics Object"));
        CreateAction("Open_Atom_CollisionTriggerTab", () => OpenTab(_ => "Collision Trigger"));
        CreateAction("Open_Atom_MaterialTab", () => OpenTab(_ => "Material"));
        CreateAction("Open_Atom_PluginsTab", () => OpenTab(_ => "Plugins"));
        CreateAction("Open_Atom_PluginsTab_Plugin#0", () => OpenPlugin(0));
        // Animation Pattern
        CreateAction("Open_AnimationPatternAtom_AnimationPatternTab", () => OpenTab(_ => "Animation Pattern", "AnimationPattern"));
        CreateAction("Open_AnimationPatternAtom_AnimationTriggersTab", () => OpenTab(_ => "Animation Triggers", "AnimationPattern"));
        // Custom Unity Asset
        CreateAction("Open_CustomUnityAssetAtom_AssetTab", () => OpenTab(_ => "Asset", "CustomUnityAsset"));
        // Audio Source
        CreateAction("Open_AudioSourceAtom_AudioSourceTab", () => OpenTab(_ => "Audio Source", "AudioSource"));
        // Person
        CreateAction("Open_PersonAtom_ClothingTab", () => OpenTab(_ => "Clothing", "Person"));
        CreateAction("Open_PersonAtom_ClothingTab", () => OpenTab(_ => "Clothing", "Person"));
        CreateAction("Open_PersonAtom_HairTab", () => OpenTab(_ => "Hair", "Person"));
        CreateAction("Open_PersonAtom_AppearancePresets", () => OpenTab(_ => "Appearance Presets", "Person"));
        CreateAction("Open_PersonAtom_GeneralPresets", () => OpenTab(_ => "General Presets", "Person"));
        CreateAction("Open_PersonAtom_PosePresets", () => OpenTab(_ => "Pose Presets", "Person"));
        CreateAction("Open_PersonAtom_SkinPresets", () => OpenTab(_ => "Skin Presets", "Person"));
        CreateAction("Open_PersonAtom_PluginsPresets", () => OpenTab(_ => "Plugins Presets", "Person"));
        CreateAction("Open_PersonAtom_MorphsPresets", () => OpenTab(_ => "Morphs Presets", "Person"));
        CreateAction("Open_PersonAtom_HairPresets", () => OpenTab(_ => "Hair Presets", "Person"));
        CreateAction("Open_PersonAtom_ClothingPresets", () => OpenTab(_ => "Clothing Presets", "Person"));
        CreateAction("Open_PersonAtom_MaleMorphs", () => OpenTab(_ => "Male Morphs", "Person"));
        CreateAction("Open_PersonAtom_FemaleMorphs", () => OpenTab(_ => "Female Morphs", "Person"));

        // Main Tabs
        CreateAction("Open_MainMenu", () => OpenMainTab(null));
        CreateAction("Open_MainMenu_FileTab", () => OpenMainTab("TabFile"));
        CreateAction("Open_MainMenu_UserPrefsTab", () => OpenMainTab("TabUserPrefs"));
        CreateAction("Open_MainMenu_NavigationTab", () => OpenMainTab("TabNavigation"));
        CreateAction("Open_MainMenu_SelectTab", () => OpenMainTab("TabSelect"));
        CreateAction("Open_MainMenu_SessionPluginPresetsTab", () => OpenMainTab("TabSessionPluginPresets"));
        CreateAction("Open_MainMenu_SessionPluginsTab", () => OpenMainTab("TabSessionPlugins"));
        CreateAction("Open_MainMenu_ScenePluginsTab", () => OpenMainTab("TabScenePlugins"));
        CreateAction("Open_MainMenu_ScenePluginPresetsTab", () => OpenMainTab("TabScenePluginPresets"));
        CreateAction("Open_MainMenu_SceneLightingTab", () => OpenMainTab("TabSceneLighting"));
        CreateAction("Open_MainMenu_SceneMiscTab", () => OpenMainTab("TabSceneMisc"));
        CreateAction("Open_MainMenu_SceneAnimationTab", () => OpenMainTab("TabSceneAnimation"));
        CreateAction("Open_MainMenu_AddAtomTab", () => OpenMainTab("TabAddAtom"));
        CreateAction("Open_MainMenu_AudioTab", () => OpenMainTab("TabAudio"));
        // CreateAction("OpenMainMenuDebugTab", () => OpenMainTab("TabDebug"));
        // TODO: Next/Previous tab

        // Selection
        CreateAction("Deselect_Atom", () => SuperController.singleton.SelectController(null));
        CreateAction("Select_HistoryBack", SelectHistoryBack);
        CreateAction("Select_PreviousAtom", () => SelectPreviousAtom());
        CreateAction("Select_NextAtom", () => SelectNextAtom());
        CreateAction("Select_PreviousPersonAtom", () => SelectPreviousAtom("Person"));
        CreateAction("Select_NextPersonAtom", () => SelectNextAtom("Person"));

        // Dev
        CreateAction("Reload_AllScenePlugins", ReloadAllScenePlugins);

        // Add atom
        CreateAction("AddAtom_AnimationPattern", () => SuperController.singleton.AddAtomByType("AnimationPattern", true, true, true));
        CreateAction("AddAtom_FloorsAndWalls_AtomSlate", () => SuperController.singleton.AddAtomByType("Slate", true, true, true));
        CreateAction("AddAtom_FloorsAndWalls_AtomWall", () => SuperController.singleton.AddAtomByType("Wall", true, true, true));
        CreateAction("AddAtom_FloorsAndWalls_AtomWoodPanel", () => SuperController.singleton.AddAtomByType("WoodPanel", true, true, true));
        CreateAction("AddAtom_Force_CycleForce", () => SuperController.singleton.AddAtomByType("CycleForce", true, true, true));
        CreateAction("AddAtom_Force_GrabPoint", () => SuperController.singleton.AddAtomByType("GrabPoint", true, true, true));
        CreateAction("AddAtom_Force_RhythmForce", () => SuperController.singleton.AddAtomByType("RhythmForce", true, true, true));
        CreateAction("AddAtom_Force_SyncForce", () => SuperController.singleton.AddAtomByType("SyncForce", true, true, true));
        CreateAction("AddAtom_Light_InvisibleLight", () => SuperController.singleton.AddAtomByType("InvisibleLight", true, true, true));
        CreateAction("AddAtom_Misc_ClothGrabSphere", () => SuperController.singleton.AddAtomByType("ClothGrabSphere", true, true, true));
        CreateAction("AddAtom_Misc_CustomUnityAsset", () => SuperController.singleton.AddAtomByType("CustomUnityAsset", true, true, true));
        CreateAction("AddAtom_Misc_Empty", () => SuperController.singleton.AddAtomByType("Empty", true, true, true));
        CreateAction("AddAtom_Misc_ImagePanel", () => SuperController.singleton.AddAtomByType("ImagePanel", true, true, true));
        CreateAction("AddAtom_Misc_SimpleSign", () => SuperController.singleton.AddAtomByType("SimpleSign", true, true, true));
        CreateAction("AddAtom_Misc_SubScene", () => SuperController.singleton.AddAtomByType("SubScene", true, true, true));
        CreateAction("AddAtom_Misc_UIText", () => SuperController.singleton.AddAtomByType("UIText", true, true, true));
        CreateAction("AddAtom_Misc_VaMLogo", () => SuperController.singleton.AddAtomByType("VaMLogo", true, true, true));
        CreateAction("AddAtom_Misc_WebBrowser", () => SuperController.singleton.AddAtomByType("WebBrowser", true, true, true));
        CreateAction("AddAtom_Misc_WebPanel", () => SuperController.singleton.AddAtomByType("WebPanel", true, true, true));
        CreateAction("AddAtom_People_Person", () => SuperController.singleton.AddAtomByType("Person", true, true, true));
        CreateAction("AddAtom_Reflective_Glass", () => SuperController.singleton.AddAtomByType("Glass", true, true, true));
        CreateAction("AddAtom_Reflective_ReflectiveSlate", () => SuperController.singleton.AddAtomByType("ReflectiveSlate", true, true, true));
        CreateAction("AddAtom_Reflective_ReflectiveWoodPanel", () => SuperController.singleton.AddAtomByType("ReflectiveWoodPanel", true, true, true));
        CreateAction("AddAtom_Shapes_Cube", () => SuperController.singleton.AddAtomByType("Cube", true, true, true));
        CreateAction("AddAtom_Shapes_Sphere", () => SuperController.singleton.AddAtomByType("Sphere", true, true, true));
        CreateAction("AddAtom_Shapes_Capsule", () => SuperController.singleton.AddAtomByType("Capsule", true, true, true));
        CreateAction("AddAtom_Sound_AudioSource", () => SuperController.singleton.AddAtomByType("AudioSource", true, true, true));
        CreateAction("AddAtom_Toys_Dildo", () => SuperController.singleton.AddAtomByType("Dildo", true, true, true));
        CreateAction("AddAtom_Triggers_CollisionTrigger", () => SuperController.singleton.AddAtomByType("CollisionTrigger", true, true, true));
        CreateAction("AddAtom_Triggers_LookAtTrigger", () => SuperController.singleton.AddAtomByType("LookAtTrigger", true, true, true));
        CreateAction("AddAtom_Triggers_UIButton", () => SuperController.singleton.AddAtomByType("UIButton", true, true, true));
        CreateAction("AddAtom_Triggers_UISlider", () => SuperController.singleton.AddAtomByType("UISlider", true, true, true));
        CreateAction("AddAtom_Triggers_UIToggle", () => SuperController.singleton.AddAtomByType("UIToggle", true, true, true));
        CreateAction("AddAtom_Triggers_VariableTrigger", () => SuperController.singleton.AddAtomByType("VariableTrigger", true, true, true));

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

    private void ReloadAllScenePlugins()
    {
        foreach (var atom in SuperController.singleton.GetAtoms().Where(a => !ReferenceEquals(a, containingAtom)))
        {
            if (atom.UITransform == null) continue;
            if (atom.UITransform
                .GetChild(0)
                .ReloadPlugins("Canvas", "Plugins", storeId))
                continue;
            foreach (var script in atom
                .GetStorableIDs()
                .Select(id => atom.GetStorableByID(id))
                .OfType<MVRScript>())
            {
                atom.RestoreFromLast(script);
            }
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
        if (tabName != null)
            SuperController.singleton.SetMainMenuTab(tabName);
    }

    private Atom OpenTab(Func<string, string> getTabName, string type = null)
    {
        var selectedAtom = selectionManager.GetLastSelectedAtomOfType(type);
        if (ReferenceEquals(selectedAtom, null)) return null;

        var tabName = getTabName(selectedAtom.type);
        if (tabName == null) return null;

        var selector = selectedAtom.gameObject.GetComponentInChildren<UITabSelector>(true);
        if (selector == null) return null;

        SuperController.singleton.SelectController(selectedAtom.mainController);
        SuperController.singleton.SetActiveUI("SelectedOptions");
        /*
        foreach (Transform t in selector.toggleContainer)
            SuperController.LogMessage(t.name);
        */
        selector.SetActiveTab(tabName);
        return selectedAtom;
    }

    private void OpenPlugin(int i)
    {
        var atom = OpenTab(_ => "Plugins");
        if (atom == null) return;
        var uid = atom.GetStorableIDs().FirstOrDefault(s => s.StartsWith($"plugin#{i}_"));
        if (uid == null) return;
        var plugin = atom.GetStorableByID(uid) as MVRScript;
        if (plugin == null) return;
        plugin.UITransform.gameObject.SetActive(true);
    }

    private void SelectHistoryBack()
    {
        var selectedController = SuperController.singleton.GetSelectedController();
        var mainController = SuperController.singleton.GetSelectedAtom()?.mainController;
        if (selectedController != mainController)
            SuperController.singleton.SelectController(mainController);
        else if (selectionManager.history.Count > 1)
            SuperController.singleton.SelectController(selectionManager.history[selectionManager.history.Count - 2].mainController);
    }

    private static void SelectPreviousAtom(string type = null)
    {
        var atoms = SuperController.singleton.GetAtoms().Where(a => type == null || a.type == type).ToList();
        if (atoms.Count == 0) return;
        var index = atoms.IndexOf(SuperController.singleton.GetSelectedAtom());
        if (index == -1)
        {
            SuperController.singleton.SelectController(atoms[atoms.Count - 1].mainController);
            return;
        }
        if (index == 0)
        {
            SuperController.singleton.SelectController(atoms[atoms.Count - 1].mainController);
            return;
        }
        SuperController.singleton.SelectController(atoms[index - 1].mainController);
    }

    private void SelectNextAtom(string type = null)
    {
        var atoms = SuperController.singleton.GetAtoms().Where(a => type == null || a.type == type).ToList();
        if (atoms.Count == 0) return;
        var index = atoms.IndexOf(SuperController.singleton.GetSelectedAtom());
        if (index == -1)
        {
            SuperController.singleton.SelectController(atoms[0].mainController);
            return;
        }
        if (index == atoms.Count - 1)
        {
            SuperController.singleton.SelectController(atoms[0].mainController);
            return;
        }
        SuperController.singleton.SelectController(atoms[index + 1].mainController);
    }
}
