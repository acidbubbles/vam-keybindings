using System.Collections.Generic;
using MVR.FileManagementSecure;
using SimpleJSON;
using UnityEngine;

public class KeybindingsExporter
{
    private const string _saveExt = "keybindings";
    private const string _saveFolder = "Saves\\keybindings";

    private readonly MVRScript _plugin;
    private readonly KeyMapManager _keyMapManager;

    public KeybindingsExporter(MVRScript plugin, KeyMapManager keyMapManager)
    {
        _plugin = plugin;
        _keyMapManager = keyMapManager;
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
        if (clear) _keyMapManager.Clear();
        if (!FileManagerSecure.FileExists(path)) return false;
        var jc = (JSONClass) SuperController.singleton.LoadJSON(path);
        if (jc == null) return false;
        _keyMapManager.RestoreFromJSON(jc["keybindings"]);
        return true;
    }

    public void ImportDefaults()
    {
        // TODO: Check if the defaults doesn't exist in the user's folder, if it fallbacks to the plugin's version
        if (Import(false, SuperController.singleton.savesDir + @"\keybindings\defaults.keybindings")) return;

        // TODO: Replace this by an actual default
        _keyMapManager.maps.Add(new KeyMap(new KeyChord[] {new KeyChord(KeyCode.Semicolon, false, false, true)}, "Keybindings.FindCommand"));
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
        const string devKeybindingsPath = @"Custom\Scripts\Dev\vam-vimvam\keybindings";
        if (FileManagerSecure.DirectoryExists(devKeybindingsPath))
            shortCuts.Add(new ShortCut {path = devKeybindingsPath, displayName = "Built-in keybindings"});
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
            {"keybindings", _keyMapManager.GetJSON()}
        };
        _plugin.SaveJSON(jc, path);
    }

    public void ExportDefault()
    {
        FileManagerSecure.CreateDirectory(_saveFolder);
        Export(SuperController.singleton.savesDir + @"\keybindings\defaults.keybindings");
    }
}
