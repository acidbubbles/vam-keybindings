using System;
using System.Collections;
using System.Linq;
using SimpleJSON;
using UnityEngine;
using UnityEngine.EventSystems;

public class Keybindings : MVRScript, IActionsInvoker
{
    private PrefabManager _prefabManager;
    private KeyMapManager _keyMapManager;
    private RemoteCommandsManager _remoteCommandsManager;
    private KeybindingsExporter _exporter;
    private KeybindingsScreen _ui;
    private KeybindingsOverlay _overlay;
    private Coroutine _timeoutCoroutine;
    private KeyMapTreeNode _current;
    private FuzzyFinder _fuzzyFinder;
    private bool _valid;
    private bool _loaded;
    private bool _commandMode;
    private bool _ctrlDown;
    private bool _altDown;
    private bool _shiftDown;

    public override void Init()
    {
        if (containingAtom.type != "SessionPluginManager")
        {
            SuperController.LogError("Keybindings plugin can only be installed as a session plugin.");
            CreateTextField(new JSONStorableString("Error", "Keybindings plugin can only be installed as a session plugin."));
            enabledJSON.val = false;
            return;
        }

        _prefabManager = new PrefabManager();
        _keyMapManager = new KeyMapManager();
        _remoteCommandsManager = new RemoteCommandsManager();
        _exporter = new KeybindingsExporter(this);
        _fuzzyFinder = new FuzzyFinder();
        SuperController.singleton.StartCoroutine(_prefabManager.LoadUIAssets());
        SuperController.singleton.StartCoroutine(DeferredInit());

        _valid = true;
        AcquireAllAvailableBroadcastingPlugins();
    }

    private IEnumerator DeferredInit()
    {
        yield return new WaitForEndOfFrame();
        if (this == null) yield break;
        if (!_loaded) containingAtom.RestoreFromLast(this);
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
        _ui.Configure();
        if (active) go.SetActive(true);

        _overlay = KeybindingsOverlay.CreateOverlayGameObject(_prefabManager);
        _overlay.autoClear = Settings.TimeoutLen;
        _overlay.Append("VimVam Ready!");
    }

    public void OnDestroy()
    {
        if (_overlay != null) Destroy(_overlay.gameObject);
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

    public KeyMapTreeNode DoMatch(KeyMapTreeNode node)
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

    public bool IsMatch(KeyChord keyChord)
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
            var selectedAction = _fuzzyFinder.FuzzyFind(query);
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
            // TODO: Module into results (reset on new char)
        }

        var result = _fuzzyFinder.FuzzyFind(query);
        _overlay.Set(result != null
            ? _fuzzyFinder.ColorizeMatch(result, query)
            : ":");
    }

    #endregion

    #region Save

    public override JSONClass GetJSON(bool includePhysical = true, bool includeAppearance = true, bool forceStore = false)
    {
        var json = base.GetJSON(includePhysical, includeAppearance, forceStore);

        if (!_valid) return json;

        try
        {
            json["keybindings"] = _keyMapManager.GetJSON();
            needsStore = true;
        }
        catch (Exception exc)
        {
            SuperController.LogError($"{nameof(Keybindings)}.{nameof(GetJSON)} (Serialize): {exc}");
        }

        return json;
    }

    public override void RestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true,
        JSONArray presetAtoms = null, bool setMissingToDefault = true)
    {
        base.RestoreFromJSON(jc, restorePhysical, restoreAppearance, presetAtoms, setMissingToDefault);

        if (!_valid) return;

        try
        {
            _loaded = true;
            _keyMapManager.RestoreFromJSON(jc["keybindings"]);
        }
        catch (Exception exc)
        {
            SuperController.LogError($"{nameof(Keybindings)}.{nameof(RestoreFromJSON)}: {exc}");
        }
    }

    #endregion

    #region Interop

    private void AcquireAllAvailableBroadcastingPlugins()
    {
        _remoteCommandsManager.Add(new ActionCommandInvoker(this, nameof(Keybindings), "FindCommand", ToggleCommandMode));

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
