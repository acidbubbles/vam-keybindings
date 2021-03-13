using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MVR.FileManagementSecure;
using SimpleJSON;
using UnityEngine;

public class KeybindingsStorage
{
    private const int _saveVersion = 2;
    private const string _saveExt = "keybindings";
    private const string _saveFolder = "Saves\\PluginData\\Keybindings";

    private readonly MVRScript _plugin;
    private readonly KeyMapManager _keyMapManager;
    private readonly IAnalogMapManager _analogMapManager;

    public KeybindingsStorage(MVRScript plugin, KeyMapManager keyMapManager, IAnalogMapManager analogMapManager)
    {
        _plugin = plugin;
        _keyMapManager = keyMapManager;
        _analogMapManager = analogMapManager;
    }

    public void OpenImportDialog(bool clear)
    {
        var shortcuts = FileManagerSecure.GetShortCutsForDirectory(_saveFolder);
        SuperController.singleton.GetMediaPathDialog(path =>
            {
                Import(clear, path);
            },
            _saveExt,
            _saveFolder,
            true,
            true,
            false,
            null,
            false,
            shortcuts);
    }

    private bool Import(bool clear, string path)
    {
        if (string.IsNullOrEmpty(path)) return false;
        if (clear)
        {
            _keyMapManager.Clear();
            _analogMapManager.Clear();
        }
        if (!FileManagerSecure.FileExists(path)) return false;
        var jc = (JSONClass) SuperController.singleton.LoadJSON(path);
        if (jc == null) return false;
        var version = jc["version"].AsInt;
        _keyMapManager.RestoreFromJSON(jc["keybindings"]);
        _analogMapManager.RestoreFromJSON(jc["analogMaps"]);
        if (version < 2)
        {
            if (_analogMapManager.maps.Any(m => m.commandName == "Camera.Pan_X" && m.slot == 0 && m.leftChord.Equals(new KeyChord(KeyCode.A, false, false, false))))
            {
                _analogMapManager.maps.Add(
                    new AnalogMap(new KeyChord(KeyCode.A, false, false, true), new KeyChord(KeyCode.D, false, false, true),
                        "Camera.Pan_X.Fast", 0));
            }
            if (_analogMapManager.maps.Any(m => m.commandName == "Camera.Pan_Y" && m.slot == 0 && m.leftChord.Equals(new KeyChord(KeyCode.Z, false, false, false))))
            {
                _analogMapManager.maps.Add(
                    new AnalogMap(new KeyChord(KeyCode.Z, false, false, true), new KeyChord(KeyCode.X, false, false, true),
                        "Camera.Pan_Y.Fast", 0));
            }
            if (_analogMapManager.maps.Any(m => m.commandName == "Camera.Pan_Z" && m.slot == 0 && m.leftChord.Equals(new KeyChord(KeyCode.W, false, false, false))))
            {
                _analogMapManager.maps.Add(
                    new AnalogMap(new KeyChord(KeyCode.W, false, false, true), new KeyChord(KeyCode.S, false, false, true),
                        "Camera.Pan_Z.Fast", 0));
            }
        }
        return true;
    }

    public void ImportDefaults()
    {
        if (Import(false, SuperController.singleton.savesDir + @"\PluginData\Keybindings\defaults.keybindings")) return;
        CreateDefaults();
    }

    public void CreateDefaults()
    {
        _keyMapManager.maps.Add(
            new KeyMap(new[] {new KeyChord(KeyCode.Semicolon, false, false, true)},
                "Keybindings.FindCommand"));

#if VAM_GT_1_20_77_0
        _keyMapManager.maps.Add(
            new KeyMap(new[] {new KeyChord(KeyCode.M, false, false, false)},
                "Monitor.Toggle_MainMonitor"));
        _keyMapManager.maps.Add(
            new KeyMap(new[] {new KeyChord(KeyCode.F1, false, false, false)},
                "Monitor.Toggle_MonitorUI"));
        _keyMapManager.maps.Add(
            new KeyMap(new[] {new KeyChord(KeyCode.Tab, false, false, false)},
                "Camera.Toggle_FreeMoveMouse"));
        _keyMapManager.maps.Add(
            new KeyMap(new[] {new KeyChord(KeyCode.F, false, false, false)},
                "Camera.FocusOnSelectedController"));
        _keyMapManager.maps.Add(
            new KeyMap(new[] {new KeyChord(KeyCode.F, false, false, true)},
                "Camera.FocusMoveOnSelectedController"));
        _keyMapManager.maps.Add(
            new KeyMap(new[] {new KeyChord(KeyCode.R, false, false, false)},
                "Camera.ResetFocusPoint"));
        _keyMapManager.maps.Add(
            new KeyMap(new[] {new KeyChord(KeyCode.E, false, false, false)},
                "GameMode.EditMode"));
        _keyMapManager.maps.Add(
            new KeyMap(new[] {new KeyChord(KeyCode.P, false, false, false)},
                "GameMode.PlayMode"));
        _keyMapManager.maps.Add(
            new KeyMap(new[] {new KeyChord(KeyCode.N, false, false, false)},
                "Select.NextPersonAtom"));
        _keyMapManager.maps.Add(
            new KeyMap(new[] {new KeyChord(KeyCode.U, false, false, false)},
                "Monitor.Toggle_MonitorHudMonitor"));
        _keyMapManager.maps.Add(
            new KeyMap(new[] {new KeyChord(KeyCode.T, false, false, false)},
                "Global.Toggle_Targets"));
        _keyMapManager.maps.Add(
            new KeyMap(new[] {new KeyChord(KeyCode.H, false, false, false)},
                "Global.Toggle_ShowHiddenAtoms"));
        _keyMapManager.maps.Add(
            new KeyMap(new[] {new KeyChord(KeyCode.C, false, false, false)},
                "Global.CycleStack"));

        _analogMapManager.maps.Add(
            new AnalogMap(new KeyChord(KeyCode.A, false, false, false), new KeyChord(KeyCode.D, false, false, false),
                "Camera.Pan_X", 0));
        _analogMapManager.maps.Add(
            new AnalogMap(new KeyChord(KeyCode.A, false, false, true), new KeyChord(KeyCode.D, false, false, true),
                "Camera.Pan_X.Fast", 0));
        _analogMapManager.maps.Add(
            new AnalogMap(new KeyChord(KeyCode.Z, false, false, false), new KeyChord(KeyCode.X, false, false, false),
                "Camera.Pan_Y", 0));
        _analogMapManager.maps.Add(
            new AnalogMap(new KeyChord(KeyCode.Z, false, false, true), new KeyChord(KeyCode.X, false, false, true),
                "Camera.Pan_Y.Fast", 0));
        _analogMapManager.maps.Add(
            new AnalogMap(new KeyChord(KeyCode.W, false, false, false), new KeyChord(KeyCode.S, false, false, false),
                "Camera.Pan_Z", 0));
        _analogMapManager.maps.Add(
            new AnalogMap(new KeyChord(KeyCode.W, false, false, true), new KeyChord(KeyCode.S, false, false, true),
                "Camera.Pan_Z.Fast", 0));
#endif
    }

    public void OpenExportDialog()
    {
        FileManagerSecure.CreateDirectory(_saveFolder);
        var fileBrowserUI = SuperController.singleton.fileBrowserUI;
        fileBrowserUI.SetTitle("Save keybindings");
        fileBrowserUI.fileRemovePrefix = null;
        fileBrowserUI.hideExtension = false;
        fileBrowserUI.keepOpen = false;
        fileBrowserUI.fileFormat = _saveExt;
        fileBrowserUI.defaultPath = _saveFolder;
        fileBrowserUI.showDirs = true;
        fileBrowserUI.shortCuts = null;
        fileBrowserUI.browseVarFilesAsDirectories = false;
        var shortCuts = new List<ShortCut>
        {
            new ShortCut {path = @"Saves\keybindings", displayName = "Custom keybindings"},
        };
        const string devKeybindingsPath = @"Custom\Scripts\Dev\vam-keybindings\keybindings";
        if (FileManagerSecure.DirectoryExists(devKeybindingsPath))
            shortCuts.Add(new ShortCut {path = devKeybindingsPath, displayName = "Dev keybindings"});
        fileBrowserUI.shortCuts = shortCuts;
        fileBrowserUI.SetTextEntry(true);
        fileBrowserUI.Show(path =>
        {
            fileBrowserUI.fileFormat = "json";
            Export(path);
        });
        fileBrowserUI.ActivateFileNameField();
    }

    public void Export(string path)
    {
        if (string.IsNullOrEmpty(path)) return;
        if (!path.ToLower().EndsWith($".{_saveExt}")) path += $".{_saveExt}";
        var jc = new JSONClass
        {
            {"version", _saveVersion.ToString(NumberFormatInfo.InvariantInfo)},
            {"keybindings", _keyMapManager.GetJSON()},
            {"analogMaps", _analogMapManager.GetJSON()},
        };
        _plugin.SaveJSON(jc, path);
    }

    public void ExportDefault()
    {
        FileManagerSecure.CreateDirectory(_saveFolder);
        Export(SuperController.singleton.savesDir + @"\PluginData\Keybindings\defaults.keybindings");
    }
}
