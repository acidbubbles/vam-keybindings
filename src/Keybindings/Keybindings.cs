using System;
using System.Linq;
using DefaultNamespace;
using UnityEngine;

public interface IKeybindingsSettings
{
    JSONStorableBool showKeyPressesJSON { get; }
}

public interface IKeybindingsModeSelector
{
    void EnterNormalMode();
}

public class Keybindings : MVRScript, IActionsInvoker, IKeybindingsSettings, IKeybindingsModeSelector
{
    public JSONStorableBool showKeyPressesJSON { get; private set; }

    private PrefabManager _prefabManager;
    private KeyMapManager _keyMapManager;
    private AnalogMapManager _analogMapManager;
    private RemoteCommandsManager _remoteCommandsManager;
    private GlobalCommands _globalCommands;
    private SelectionHistoryManager _selectionHistoryManager;
    private KeybindingsStorage _storage;
    private KeybindingsScreen _ui;
    private FindModeHandler _findModeHandler;
    private NormalModeHandler _normalModeHandler;
    private IModeHandler _currentModeHandler;
    private KeybindingsOverlayReference _overlayReference;
    private bool _valid;
    private AnalogHandler _analogHandler;

    public override void Init()
    {
        if (containingAtom.type != "SessionPluginManager")
        {
            SuperController.LogError("Keybindings plugin can only be installed as a session plugin.");
            CreateTextField(new JSONStorableString("Error", "Keybindings plugin can only be installed as a session plugin."));
            enabledJSON.val = false;
            return;
        }

        showKeyPressesJSON = new JSONStorableBool("ShowKeypresses", false);

        _prefabManager = new PrefabManager();
        _keyMapManager = new KeyMapManager();
        _analogMapManager = new AnalogMapManager();
        _selectionHistoryManager = new SelectionHistoryManager();
        _remoteCommandsManager = new RemoteCommandsManager(_selectionHistoryManager);
        _globalCommands = new GlobalCommands(this, containingAtom, _selectionHistoryManager, _remoteCommandsManager);
        _storage = new KeybindingsStorage(this, _keyMapManager, _analogMapManager);
        _overlayReference = new KeybindingsOverlayReference();

        _analogHandler = new AnalogHandler(_remoteCommandsManager, _analogMapManager);
        _normalModeHandler = new NormalModeHandler(this, this, _keyMapManager, _overlayReference, _remoteCommandsManager);
        _findModeHandler = new FindModeHandler(this, _remoteCommandsManager, _overlayReference);

        SuperController.singleton.StartCoroutine(_prefabManager.LoadUIAssets());

        AcquireBuiltInCommands();
        _globalCommands.Init();
        AcquireAllAvailableBroadcastingPlugins();

        _storage.ImportDefaults();

        EnterNormalMode();
        // TODO: Map multiple bindings to the same action?

        _valid = true;
    }

    public override void InitUI()
    {
        base.InitUI();
        if (!_valid) return;
        if (UITransform == null) return;
        _prefabManager.triggerActionsParent = UITransform;

        var scriptUI = UITransform.GetComponentInChildren<MVRScriptUI>();

        var go = new GameObject();
        go.transform.SetParent(scriptUI.fullWidthUIContent, false);

        var active = go.activeInHierarchy;
        if (active) go.SetActive(false);
        _ui = go.AddComponent<KeybindingsScreen>();
        _ui.prefabManager = _prefabManager;
        _ui.keyMapManager = _keyMapManager;
        _ui.analogMapManager = _analogMapManager;
        _ui.remoteCommandsManager = _remoteCommandsManager;
        _ui.storage = _storage;
        _ui.settings = this;
        _ui.Configure();
        if (active) go.SetActive(true);

        var overlay = KeybindingsOverlay.CreateOverlayGameObject(_prefabManager);
        overlay.autoClear = Settings.TimeoutLen;
        _overlayReference.value = overlay;
        // overlay.Append("Keybindings ready!");
    }

    public void OnDisable()
    {
        _normalModeHandler.Leave();
        _analogHandler.Leave();
    }

    public void OnDestroy()
    {
        if (_overlayReference?.value != null) Destroy(_overlayReference?.value.gameObject);
        _keyMapManager?.Dispose();
    }

    public void Update()
    {
        if (!_valid) return;

        try
        {
            _selectionHistoryManager.Update();

            _analogHandler.Update();
            _globalCommands.Update();

            // Don't waste resources
            // Do not listen while a keybinding is being recorded
            if (_ui.isRecording) return;
            // Process based on the mode
                _currentModeHandler.OnKeyDown();
        }
        catch (Exception e)
        {
            SuperController.LogError($"{nameof(Keybindings)}.{nameof(Update)}: {e}");
        }
    }

    private void EnterFindMode()
    {
        _currentModeHandler?.Leave();
        _currentModeHandler = _findModeHandler;
        _currentModeHandler.Enter();
    }

    public void EnterNormalMode()
    {
        _currentModeHandler?.Leave();
        _currentModeHandler = _normalModeHandler;
        _currentModeHandler.Enter();
    }

    #region Interop

    private void AcquireBuiltInCommands()
    {
        _remoteCommandsManager.Add(new ActionCommandInvoker(this, nameof(Keybindings), "FindCommand", EnterFindMode));
        _remoteCommandsManager.Add(new ActionCommandInvoker(this, nameof(Keybindings), "Settings", OpenSettings));
        _remoteCommandsManager.Add(new ActionCommandInvoker(this, nameof(Keybindings), "ReloadPlugin", ReloadPlugin));
    }

    private void AcquireAllAvailableBroadcastingPlugins()
    {
        foreach (var storable in SuperController.singleton
            .GetAtoms()
            .SelectMany(atom => atom.GetStorableIDs()
            .Select(atom.GetStorableByID)
            .Where(s => s is MVRScript)))
        {
            _remoteCommandsManager.TryRegister(storable);
        }

        foreach (var storable in SuperController.singleton
            .GetComponentInChildren<MVRPluginManager>()
            .GetComponentsInChildren<MVRScript>()
            .Where(s => !ReferenceEquals(s, this)))
        {
            _remoteCommandsManager.TryRegister(storable);
        }
    }

    public void OnActionsProviderAvailable(JSONStorable storable)
    {
        if (!_valid) return;
        _remoteCommandsManager.TryRegister(storable);
    }

    public void OnActionsProviderDestroyed(JSONStorable storable)
    {
        if (!_valid) return;
        _remoteCommandsManager.Remove(storable);
    }

    #endregion

    #region Built-in commands

    private void OpenSettings()
    {
        SuperController.singleton.SetActiveUI("MainMenu");
        SuperController.singleton.SetMainMenuTab("TabSessionPlugins");
        UITransform.gameObject.SetActive(true);
    }

    private void ReloadPlugin()
    {
        if(SuperController.singleton.mainHUD.ReloadPlugins("MainUICanvas", "TabSessionPlugins", storeId)) return;
        SuperController.LogError($"Keybindings: Could not find plugin {storeId} in the session plugin panel.");
    }

    #endregion
}
