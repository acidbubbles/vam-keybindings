using System;
using System.Collections;
using System.Collections.Generic;
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
    private readonly List<KeyValuePair<KeyMap, ICommandReleaser>> _releasers = new List<KeyValuePair<KeyMap, ICommandReleaser>>();

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

        foreach (var x in _releasers)
        {
            try
            {
                x.Value.Release();
            }
            catch (Exception exc)
            {
                SuperController.LogError($"Error releasing command '{x.Key.commandName}': {exc}");
            }
        }
        _releasers.Clear();
    }

    public void OnKeyDown()
    {
        CheckReleasers();

        if (!Input.anyKeyDown) return;

        if (_timeoutCoroutine != null)
            _owner.StopCoroutine(_timeoutCoroutine);

        if (LookInputModule.singleton.inputFieldActive)
            return;

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
            if (match.map != null)
                Invoke(match.map);
            return;
        }

        _current = match;
        _timeoutCoroutine = _owner.StartCoroutine(TimeoutCoroutine());
    }

    private void CheckReleasers()
    {
        for (var i = 0; i < _releasers.Count; i++)
        {
            var releaser = _releasers[i];
            var chord = releaser.Key.chords[releaser.Key.chords.Length - 1];
            if (chord.IsActive()) continue;

            try
            {
                releaser.Value.Release();
                _releasers.RemoveAt(i);
                i--;
            }
            catch (Exception exc)
            {
                SuperController.LogError($"Error releasing command '{releaser.Key.commandName}': {exc}");
            }
        }
    }

    private static KeyMapTreeNode DoMatch(KeyMapTreeNode node)
    {

        for (var i = 0; i < node.next.Count; i++)
        {
            var child = node.next[i];
            if (child.keyChord.IsDown())
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
            if (_current.map != null)
            {
                Invoke(_current.map);
                _current = _keyMapManager.root;
            }
            _timeoutCoroutine = null;
        }
        catch (Exception e)
        {
            SuperController.LogError($"{nameof(Keybindings)}.{nameof(TimeoutCoroutine)}: {e}");
        }
    }

    private void Invoke(KeyMap map)
    {
        var releaser = _remoteCommandsManager.Invoke(map.commandName);
        if (releaser != null)
            _releasers.Add(new KeyValuePair<KeyMap, ICommandReleaser>(map, releaser));
    }
}
