using System;
using System.Collections;
using System.Linq;
using System.Text;
using SimpleJSON;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShortcutsPlugin : MVRScript, IActionsInvoker
{
    private PrefabManager _prefabManager;
    private KeyMapManager _keyMapManager;
    private RemoteActionsManager _remoteActionsManager;
    private ShortcutsScreen _ui;
    private ShortcutsOverlay _overlay;
    private Coroutine _timeoutCoroutine;
    private KeyMapTreeNode _current;
    private bool _loaded;
    private bool _controlMode;
    private readonly StringBuilder _controlInput = new StringBuilder();

    public override void Init()
    {
        _prefabManager = new PrefabManager();
        _keyMapManager = new KeyMapManager();
        _remoteActionsManager = new RemoteActionsManager();
        SuperController.singleton.StartCoroutine(_prefabManager.LoadUIAssets());
        SuperController.singleton.StartCoroutine(DeferredInit());

        AcquireAllAvailableBroadcastingPlugins();
    }

    private IEnumerator DeferredInit()
    {
        yield return new WaitForEndOfFrame();
        if (this == null) yield break;
        if (!_loaded) containingAtom.RestoreFromLast(this);

        // TODO: Remove this later, replace by levels (defaults, session, scene, atom)
        _keyMapManager.RestoreDefaults();
        // _bindingsManager.Debug(_bindingsManager.root);
    }

    public override void InitUI()
    {
        base.InitUI();
        if (UITransform == null) return;
        _prefabManager.triggerActionsParent = UITransform;

        var scriptUI = UITransform.GetComponentInChildren<MVRScriptUI>();

        var go = new GameObject();
        go.transform.SetParent(scriptUI.fullWidthUIContent, false);

        var active = go.activeInHierarchy;
        if (active) go.SetActive(false);
        _ui = go.AddComponent<ShortcutsScreen>();
        _ui.prefabManager = _prefabManager;
        _ui.keyMapManager = _keyMapManager;
        _ui.remoteActionsManager = _remoteActionsManager;
        _ui.Configure();
        if (active) go.SetActive(true);

        _overlay = ShortcutsOverlay.CreateOverlayGameObject(_prefabManager);
        _overlay.autoClear = Settings.TimeoutLen;
        _overlay.Append("VimVam Ready!");
    }

    public void OnDestroy()
    {
        if (_overlay != null) Destroy(_overlay.gameObject);
    }

    public void Update()
    {
        try
        {
            // Don't waste resources
            if (!Input.anyKeyDown) return;

            if (_controlMode)
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
            SuperController.LogError($"{nameof(ShortcutsPlugin)}.{nameof(Update)}: {e}");
        }
    }

    #region Normal mode

    private void HandleNormalMode()
    {
        if (_timeoutCoroutine != null)
            StopCoroutine(_timeoutCoroutine);

        var current = _current;
        _current = null;
        var next = current?.DoMatch();

        if (next == null)
        {
            next = _keyMapManager.root.DoMatch();
            if (next == null)
            {
                if (Input.GetKeyDown(KeyCode.Semicolon) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
                {
                    StartControlMode();
                }

                return;
            }
        }

        _overlay.Append(next.keyChord.ToString());

        if (next.next.Count == 0)
        {
            if (next.action != null)
                Invoke(next.action);
            return;
        }

        _current = next;
        _timeoutCoroutine = StartCoroutine(TimeoutCoroutine());
    }

    private IEnumerator TimeoutCoroutine()
    {
        yield return new WaitForSecondsRealtime(Settings.TimeoutLen);
        if (_current == null) yield break;
        try
        {
            if (_current.action != null)
            {
                Invoke(_current.action);
                _current = _keyMapManager.root;
            }
            _timeoutCoroutine = null;
        }
        catch (Exception e)
        {
            SuperController.LogError($"{nameof(ShortcutsPlugin)}.{nameof(TimeoutCoroutine)}: {e}");
        }
    }

    private void Invoke(string action)
    {
        if(!_remoteActionsManager.Invoke(action))
            _overlay.Set($"Action '{action}' not found");
    }

    #endregion

    #region Control mode

    private void StartControlMode()
    {
        _controlMode = true;
        _overlay.autoClear = float.PositiveInfinity;
        _overlay.Set(":");
        EventSystem.current.SetSelectedGameObject(_overlay.input.gameObject);
        _overlay.input.ActivateInputField();
        _overlay.input.Select();
    }

    private void LeaveControlMode()
    {
        _controlMode = false;
        _controlInput.Length = 0;
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
            LeaveControlMode();
            // TODO: Do not re-run fuzzy find, instead keep track of it all in a separate stateful class
            var selectedAction = _remoteActionsManager.FuzzyFind(query);
            if (selectedAction != null)
                Invoke(selectedAction.name);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LeaveControlMode();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // TODO: Module into results (reset on new char)
        }

        // TODO: Keep track of the results subset so we can accelerate fuzzy finding
        var nearestAction = _remoteActionsManager.FuzzyFind(query);
        if (nearestAction != null)
        {
            var nearestActionHighlighted = ColorizeMatch(nearestAction.name, query);
            _overlay.Set(nearestActionHighlighted);
        }
        else
        {
            _overlay.Set("");
        }
    }

    private string ColorizeMatch(string action, string query)
    {
        // TODO: Reuse the stringbuilder
        var sb = new StringBuilder(action.Length);
        var queryIndex = 0;
        for (var actionIndex = 0; actionIndex < action.Length; actionIndex++)
        {
            if (queryIndex >= query.Length)
            {
                sb.Append(action.Substring(actionIndex));
                break;
            }

            // TODO: Same code in RemoteActionsManager, to extract and reuse
            var queryChar = query[queryIndex];
            var actionChar = action[actionIndex];
            var isMatch = char.IsLower(queryChar) ? queryChar == char.ToLowerInvariant(actionChar) : queryChar == actionChar;

            if (isMatch)
            {
                queryIndex++;
                sb.Append("<color=cyan>");
                sb.Append(action[actionIndex]);
                sb.Append("</color>");
                continue;
            }

            sb.Append(action[actionIndex]);
        }

        return sb.ToString();
    }

    #endregion

    #region Save

    public override JSONClass GetJSON(bool includePhysical = true, bool includeAppearance = true, bool forceStore = false)
    {
        var json = base.GetJSON(includePhysical, includeAppearance, forceStore);

        try
        {
            json["maps"] = _keyMapManager.GetJSON();
            needsStore = true;
        }
        catch (Exception exc)
        {
            SuperController.LogError($"{nameof(ShortcutsPlugin)}.{nameof(GetJSON)} (Serialize): {exc}");
        }

        return json;
    }

    public override void RestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true,
        JSONArray presetAtoms = null, bool setMissingToDefault = true)
    {
        base.RestoreFromJSON(jc, restorePhysical, restoreAppearance, presetAtoms, setMissingToDefault);

        try
        {
            _loaded = true;
            _keyMapManager.RestoreFromJSON(jc["maps"]);
        }
        catch (Exception exc)
        {
            SuperController.LogError($"{nameof(ShortcutsPlugin)}.{nameof(RestoreFromJSON)}: {exc}");
        }
    }

    #endregion

    #region Interop

    public void AcquireAllAvailableBroadcastingPlugins()
    {
        foreach (var storable in SuperController.singleton
            .GetAtoms()
            .SelectMany(atom => atom.GetStorableIDs()
            .Select(atom.GetStorableByID)
            .Where(s => s is MVRScript)))
        {
            _remoteActionsManager.TryRegister(storable);
        }
    }

    public void OnActionsProviderAvailable(JSONStorable storable)
    {
        _remoteActionsManager.TryRegister(storable);
    }

    public void OnActionsProviderDestroyed(JSONStorable storable)
    {
        _remoteActionsManager.Remove(storable);
    }

    #endregion
}
