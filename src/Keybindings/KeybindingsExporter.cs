using MVR.FileManagementSecure;
using SimpleJSON;

public class KeybindingsExporter
{
    private const string _saveExt = "keybindings";
    private const string _saveFolder = "Saves\\keybindings";

    private readonly MVRScript _plugin;

    public KeybindingsExporter(MVRScript plugin)
    {
        _plugin = plugin;
    }

    public void Import()
    {
            var shortcuts = FileManagerSecure.GetShortCutsForDirectory(_saveFolder);
            SuperController.singleton.GetMediaPathDialog((string path) => {
                if (string.IsNullOrEmpty(path)) return;
                var jc = (JSONClass)_plugin.LoadJSON(path);
                _plugin.RestoreFromJSON(jc);
            }, _saveExt, _saveFolder, false, true, false, null, false, shortcuts);
    }

    public void Export()
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
            fileBrowserUI.Show((string path) => {
                fileBrowserUI.fileFormat = null;
                if (string.IsNullOrEmpty(path)) return;
                if (!path.ToLower().EndsWith($".{_saveExt}")) path += $".{_saveExt}";
                var jc = _plugin.GetJSON();
                jc.Remove("id");
                _plugin.SaveJSON(jc, path);
            });
            fileBrowserUI.ActivateFileNameField();
    }
}
