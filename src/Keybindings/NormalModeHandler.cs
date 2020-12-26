using System;
using System.Collections;
using UnityEngine;

public class NormalModeHandler : IModeHandler
{
    private readonly MonoBehaviour _owner;
    private readonly IKeybindingsSettings _settings;
    private readonly IKeyMapManager _keyMapManager;
    private readonly KeybindingsOverlayReference _overlay;
    private readonly RemoteCommandsManager _remoteCommandsManager;
    private Coroutine _timeoutCoroutine;
    private KeyMapTreeNode _current;

    public NormalModeHandler(
        MonoBehaviour owner,
        IKeybindingsSettings settings,
        IKeyMapManager keyMapManager,
        KeybindingsOverlayReference overlay,
        RemoteCommandsManager remoteCommandsManager
        )
    {
        _owner = owner;
        _settings = settings;
        _keyMapManager = keyMapManager;
        _overlay = overlay;
        _remoteCommandsManager = remoteCommandsManager;
    }

    public void Enter()
    {
    }

    public void Leave()
    {
        if (_timeoutCoroutine != null)
            _owner.StopCoroutine(_timeoutCoroutine);
    }

    public void OnKeyDown()
    {
        // <C-*> shortcuts can work even in a text field, otherwise text fields have preference
        if (LookInputModule.singleton.inputFieldActive && !Input.GetKey(KeyCode.LeftControl)) return;

        if (_timeoutCoroutine != null)
            _owner.StopCoroutine(_timeoutCoroutine);

        var current = _current;
        _current = null;
        var match = current != null ? DoMatch(current) : null;

        if (match == null)
        {
            match = DoMatch(_keyMapManager.root);
            if (match == null)
                return;
        }

        if (_settings.showKeyPressesJSON.val)
            _overlay.value.Append(match.keyChord.ToString());

        if (match.next.Count == 0)
        {
            if (match.boundCommandName != null)
                Invoke(match.boundCommandName);
            return;
        }

        _current = match;
        _timeoutCoroutine = _owner.StartCoroutine(TimeoutCoroutine());
    }

    private static KeyMapTreeNode DoMatch(KeyMapTreeNode node)
    {

        for (var i = 0; i < node.next.Count; i++)
        {
            var child = node.next[i];
            if (child.keyChord.IsActive())
                return child;
        }

        return null;
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
            _overlay.value.Set($"Action '{action}' not found");
    }
}
