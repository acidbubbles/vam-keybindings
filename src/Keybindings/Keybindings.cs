using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IKeybindingsSettings
{
    JSONStorableBool showKeyPressesJSON { get; }
}

public class Keybindings : MVRScript, IActionsInvoker, IKeybindingsSettings
{
    private PrefabManager _prefabManager;
    private KeyMapManager _keyMapManager;
    private RemoteCommandsManager _remoteCommandsManager;
    private SelectionHistoryManager _selectionHistoryManager;
    private KeybindingsExporter _exporter;
    private KeybindingsScreen _ui;
    private KeybindingsOverlay _overlay;
    private Coroutine _timeoutCoroutine;
    private KeyMapTreeNode _current;
    private FuzzyFinder _fuzzyFinder;
    private bool _valid;
    private bool _commandMode;
    private bool _ctrlDown;
    private bool _altDown;
    private bool _shiftDown;
    public JSONStorableBool showKeyPressesJSON { get; private set; }

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
        _remoteCommandsManager = new RemoteCommandsManager();
        _selectionHistoryManager = gameObject.AddComponent<SelectionHistoryManager>();
        _exporter = new KeybindingsExporter(this, _keyMapManager);
        _fuzzyFinder = new FuzzyFinder();

        SuperController.singleton.StartCoroutine(_prefabManager.LoadUIAssets());

        AcquireAllAvailableBroadcastingPlugins();

        _exporter.ImportDefaults();

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
        _ui.remoteCommandsManager = _remoteCommandsManager;
        _ui.exporter = _exporter;
        _ui.settings = this;
        _ui.Configure();
        if (active) go.SetActive(true);

        _overlay = KeybindingsOverlay.CreateOverlayGameObject(_prefabManager);
        _overlay.autoClear = Settings.TimeoutLen;
        _overlay.Append("VimVam Ready!");
    }

    public void OnDestroy()
    {
        if (_overlay != null) Destroy(_overlay.gameObject);
        if (_selectionHistoryManager != null) Destroy(_selectionHistoryManager);
        _keyMapManager?.Dispose();
    }

    public void Update()
    {
        if (!_valid) return;

        try
        {
            // Don't waste resources
            if (!Input.anyKeyDown) return;

            if (_commandMode)
            {
                HandleControlMode();
                return;
            }

            // Do not listen while a keybinding is being recorded
            if (_ui.isRecording) return;

            // <C-*> shortcuts can work even in a text field, otherwise text fields have preference
            if (LookInputModule.singleton.inputFieldActive && !Input.GetKey(KeyCode.LeftControl)) return;

            HandleNormalMode();
        }
        catch (Exception e)
        {
            SuperController.LogError($"{nameof(Keybindings)}.{nameof(Update)}: {e}");
        }
    }

    #region Normal mode

    private void HandleNormalMode()
    {
        if (_timeoutCoroutine != null)
            StopCoroutine(_timeoutCoroutine);

        var current = _current;
        _current = null;
        var match = current != null ? DoMatch(current) : null;

        if (match == null)
        {
            match = DoMatch(_keyMapManager.root);
            if (match == null)
            {
                if (Input.GetKeyDown(KeyCode.Semicolon) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
                {
                    StartCommandMode();
                }

                return;
            }
        }

        if (showKeyPressesJSON.val)
            _overlay.Append(match.keyChord.ToString());

        if (match.next.Count == 0)
        {
            if (match.boundCommandName != null)
                Invoke(match.boundCommandName);
            return;
        }

        _current = match;
        _timeoutCoroutine = StartCoroutine(TimeoutCoroutine());
    }

    private KeyMapTreeNode DoMatch(KeyMapTreeNode node)
    {
        _ctrlDown = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        _altDown = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        _shiftDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        for (var i = 0; i < node.next.Count; i++)
        {
            var child = node.next[i];
            if (IsMatch(child.keyChord))
                return child;
        }

        return null;
    }

    private bool IsMatch(KeyChord keyChord)
    {
        if (!Input.GetKeyDown(keyChord.key)) return false;
        if (keyChord.ctrl != _ctrlDown) return false;
        if (keyChord.alt != _altDown) return false;
        if (keyChord.shift != _shiftDown) return false;
        return true;
    }

    private IEnumerator TimeoutCoroutine()
    {
        yield return new WaitForSecondsRealtime(Settings.TimeoutLen);
        if (_current == null) yield break;
        try
        {
            if (_current.boundCommandName != null)
            {
                Invoke(_current.boundCommandName);
                _current = _keyMapManager.root;
            }
            _timeoutCoroutine = null;
        }
        catch (Exception e)
        {
            SuperController.LogError($"{nameof(Keybindings)}.{nameof(TimeoutCoroutine)}: {e}");
        }
    }

    private void Invoke(string action)
    {
        if(!_remoteCommandsManager.Invoke(action))
            _overlay.Set($"Action '{action}' not found");
    }

    #endregion

    #region Control mode

    private void ToggleCommandMode()
    {
        if (_commandMode)
            LeaveCommandMode();
        else
            StartCommandMode();
    }

    private void StartCommandMode()
    {
        _commandMode = true;
        _fuzzyFinder.Init(_remoteCommandsManager.names);
        _overlay.autoClear = float.PositiveInfinity;
        _overlay.Set(":");
        EventSystem.current.SetSelectedGameObject(_overlay.input.gameObject);
        _overlay.input.text = "";
        _overlay.input.ActivateInputField();
        _overlay.input.Select();
    }

    private void LeaveCommandMode()
    {
        _commandMode = false;
        _fuzzyFinder.Clear();
        _overlay.input.text = "";
        _overlay.input.DeactivateInputField();
        EventSystem.current.SetSelectedGameObject(null);
        _overlay.autoClear = Settings.TimeoutLen;
        _overlay.Set("");
    }

    private void HandleControlMode()
    {
        var query = _overlay.input.text;

        if (Input.GetKeyDown(KeyCode.Return))
        {
            var selectedAction = _fuzzyFinder.current;
            LeaveCommandMode();
            if (selectedAction != null)
                Invoke(selectedAction);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LeaveCommandMode();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                _fuzzyFinder.tabIndex = _fuzzyFinder.tabIndex == 0 ? _fuzzyFinder.matches - 1 : (_fuzzyFinder.tabIndex - 1);
            else
                _fuzzyFinder.tabIndex = (_fuzzyFinder.tabIndex + 1) % _fuzzyFinder.matches;
        }

        _overlay.Set(!_fuzzyFinder.FuzzyFind(query) ? "" : $"{_fuzzyFinder.ColorizeMatch(_fuzzyFinder.current, query)} ({_fuzzyFinder.tabIndex + 1}/{_fuzzyFinder.matches})");
    }

    #endregion

    #region Settings

    private void OpenSettings()
    {
        SuperController.singleton.SetActiveUI("MainMenu");
        SuperController.singleton.SetMainMenuTab("TabSessionPlugins");
        UITransform.gameObject.SetActive(true);
    }

    #endregion

    #region Interop

    private void AcquireAllAvailableBroadcastingPlugins()
    {
        _remoteCommandsManager.Add(new ActionCommandInvoker(this, nameof(Keybindings), "FindCommand", ToggleCommandMode));
        _remoteCommandsManager.Add(new ActionCommandInvoker(this, nameof(Keybindings), "KeybindingsSettings", OpenSettings));

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
}
