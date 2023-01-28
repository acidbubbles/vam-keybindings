using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class FindModeHandler : IModeHandler
{
    private const string _selectByNameCommandNamespace = "Select:";

    private readonly IKeybindingsModeSelector _modeSelector;
    private readonly FuzzyFinder _fuzzyFinder;
    private readonly RemoteCommandsManager _remoteCommandsManager;
    private readonly KeybindingsOverlayReference _overlay;
    private string _lastSelectedAction;

    public FindModeHandler(IKeybindingsModeSelector modeSelector, RemoteCommandsManager remoteCommandsManager, KeybindingsOverlayReference overlay)
    {
        _modeSelector = modeSelector;
        _remoteCommandsManager = remoteCommandsManager;
        _overlay = overlay;
        _fuzzyFinder = new FuzzyFinder();
    }

    public void Enter()
    {
        var commands = new List<string>(_remoteCommandsManager.names.Count + SuperController.singleton.GetAtomUIDs().Count);
        commands.AddRange(_remoteCommandsManager.names);
        commands.AddRange(SuperController.singleton.GetAtomUIDsWithFreeControllers().Select(x => $"{_selectByNameCommandNamespace}{x}"));
        _fuzzyFinder.Init(commands);
        var overlay = _overlay.value;
        overlay.autoClear = float.PositiveInfinity;
        overlay.Set(":");
        EventSystem.current.SetSelectedGameObject(overlay.input.gameObject);
        overlay.input.text = _lastSelectedAction;
        overlay.input.ActivateInputField();
        overlay.input.Select();
        if (_lastSelectedAction != null) _fuzzyFinder.FuzzyFind(_lastSelectedAction);
    }

    public void Leave()
    {
        _fuzzyFinder.Clear();
        var overlay = _overlay.value;
        overlay.input.text = "";
        overlay.input.DeactivateInputField();
        EventSystem.current.SetSelectedGameObject(null);
        overlay.autoClear = Constants.TimeoutLen;
        overlay.Set("");
    }

    public void OnKeyDown()
    {
        if (!Input.anyKeyDown) return;

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Mouse0))
        {
            _modeSelector.EnterNormalMode();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            var selectedAction =  _fuzzyFinder.current;
            if (selectedAction != null)
            {
                if (selectedAction.StartsWith(_selectByNameCommandNamespace))
                {
                    SuperController.singleton.SelectController(SuperController.singleton.GetAtomByUid(selectedAction.Substring(_selectByNameCommandNamespace.Length))?.mainController);
                }
                else
                {
                    Invoke(selectedAction);
                }
                _lastSelectedAction = selectedAction;
            }
            _modeSelector.EnterNormalMode();
            return;
        }

        var query = _overlay.value.input.text;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (query.Length == 0 || _fuzzyFinder.matches < 2) return;
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                _fuzzyFinder.tabIndex = _fuzzyFinder.tabIndex == 0 ? _fuzzyFinder.matches - 1 : (_fuzzyFinder.tabIndex - 1);
            else
                _fuzzyFinder.tabIndex = (_fuzzyFinder.tabIndex + 1) % _fuzzyFinder.matches;
        }

        var matched = _fuzzyFinder.FuzzyFind(query);
        if (!matched)
        {
            _overlay.value.Set(":");
            return;
        }

        var contextStr = "";
        IActionCommandInvoker invoker;
        if (_remoteCommandsManager.TryGetInvoker(_fuzzyFinder.current, out invoker))
        {
            var script = invoker.storable as MVRScript;
            if (script != null && script.containingAtom != null)
            {
                var atomName = script.containingAtom.name;
                if (atomName != "CoreControl")
                    contextStr = $" <color=grey>[{script.containingAtom.name}]</color>";
            }
        }
        _overlay.value.Set($"{_fuzzyFinder.ColorizeMatch(_fuzzyFinder.current, query)}{contextStr} ({_fuzzyFinder.tabIndex + 1}/{_fuzzyFinder.matches})");
    }

    private void Invoke(string action)
    {
        var releaser = _remoteCommandsManager.Invoke(action);
        // TODO: Instead, filter releasers from fuzzy find
        releaser?.Release();
    }
}
