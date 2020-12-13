using MVR.FileManagementSecure;
using SimpleJSON;

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
                false,
                true,
                false,
                null,
                false,
                shortcuts);
    }

    private void Import(bool clear, string path)
    {
        if (string.IsNullOrEmpty(path)) return;
        if (clear) _keyMapManager.Clear();
        var jc = (JSONClass) _plugin.LoadJSON(path);
        _keyMapManager.RestoreFromJSON(jc["keybindings"]);
    }

    public void ImportDefaults()
    {
        // TODO: Check if the defaults doesn't exist in the user's folder, if it fallbacks to the plugin's version
        Import(false, SuperController.singleton.savesDir + @"\keybindings\defaults.keybindings");
    }

    public void OpenExportDialog()
    {
            FileManagerSecure.CreateDirectory(_saveFolder);
            var fileBrowserUI = SuperController.singleton.fileBrowserUI;
            fileBrowserUI.SetTitle("Save colliders preset");
            fileBrowserUI.fileRemovePrefix = null;
            fileBrowserUI.hideExtension = false;
            fileBrowserUI.keepOpen = false;
            fileBrowserUI.fileFormat = _saveExt;
            fileBrowserUI.defaultPath = _saveFolder;
            fileBrowserUI.showDirs = true;
            fileBrowserUI.shortCuts = null;
            fileBrowserUI.browseVarFilesAsDirectories = false;
            fileBrowserUI.SetTextEntry(true);
            fileBrowserUI.Show(path => {
                fileBrowserUI.fileFormat = null;
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
