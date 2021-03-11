using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class KeybindingsScreen : MonoBehaviour
{
    private const string _notBoundButtonLabel = "";

    private class CommandBindingRow
    {
        public GameObject container;
        public UIDynamicButton bindingBtn1;
        public UIDynamicButton bindingBtn2;
        public ICommandInvoker invoker;
        public string commandName => invoker?.commandName;
    }

    public IPrefabManager prefabManager { get; set; }
    public IKeyMapManager keyMapManager { get; set; }
    public IAnalogMapManager analogMapManager { get; set; }
    public RemoteCommandsManager remoteCommandsManager { get; set; }
    public KeybindingsStorage storage { get; set; }
    public IKeybindingsSettings settings { get; set; }

    public bool isRecording;

    private bool _initialized;
    private InputField _searchInput;
    private UIDynamicToggle _showBoundOnly;
    private readonly List<List<CommandBindingRow>> _rowGroups = new List<List<CommandBindingRow>>();
    private Coroutine _setKeybindingCoroutine;

    public void Configure()
    {
        var layoutElement = gameObject.AddComponent<LayoutElement>();
        layoutElement.minWidth = 100;
        layoutElement.minHeight = 100;

        var group = gameObject.AddComponent<VerticalLayoutGroup>();
        group.spacing = 10f;
        group.padding = new RectOffset(10, 10, 10, 10);

        var bg = gameObject.AddComponent<Image>();
        bg.raycastTarget = false;
        bg.color = new Color(0.721f, 0.682f, 0.741f);
    }

    public void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        var optionsTitle = prefabManager.CreateText(transform, "<b>Options</b>");
        optionsTitle.fontSize = 32;
        optionsTitle.alignment = TextAnchor.MiddleCenter;

        var showKeypresses = prefabManager.CreateToggle(transform, "Show key presses as you type them (desktop)");
        showKeypresses.backgroundColor = Color.clear;
        settings.showKeyPressesJSON.toggle = showKeypresses.toggle;

        prefabManager.CreateSpacer(transform, 10f);

        var importTitle = prefabManager.CreateText(transform, "<b>Import / Export</b>");
        importTitle.fontSize = 32;
        importTitle.alignment = TextAnchor.MiddleCenter;

        var importSubtitle = prefabManager.CreateText(transform, "<i>Import an existing scheme, and use 'Save' to keep your current shortcuts.</i>");
        importSubtitle.alignment = TextAnchor.UpperCenter;
        importSubtitle.GetComponent<LayoutElement>().preferredHeight = 70;

        var toolbarGo = new GameObject();
        toolbarGo.transform.SetParent(transform, false);

        var group = toolbarGo.AddComponent<HorizontalLayoutGroup>();
        group.spacing = 20;

        var importBtn = prefabManager.CreateButton(toolbarGo.transform, "Import (overwrite)");
        importBtn.button.onClick.AddListener(() => { if (!isRecording) { storage.OpenImportDialog(true); } });

        var addBtn = prefabManager.CreateButton(toolbarGo.transform, "Import (add)");
        addBtn.button.onClick.AddListener(() => { if (!isRecording) { storage.OpenImportDialog(false); } });

        var exportBtn = prefabManager.CreateButton(toolbarGo.transform, "Export");
        exportBtn.button.onClick.AddListener(() => { if (!isRecording) { storage.OpenExportDialog(); } });

        var saveBtn = prefabManager.CreateButton(toolbarGo.transform, "Save");
        saveBtn.button.onClick.AddListener(() => { if (!isRecording) { storage.ExportDefault(); } });

        prefabManager.CreateSpacer(transform, 10f);

        var keybindingsTitle = prefabManager.CreateText(transform, "<b>Keybindings</b>");
        keybindingsTitle.fontSize = 32;
        keybindingsTitle.alignment = TextAnchor.MiddleCenter;

        var keybindingsSubtitle = prefabManager.CreateText(transform, @"
Commands with an <color=blue>asterisk</color> can be bound to joysticks and mouse axis.
Mouse movements require a modifier key. Move in the other direction to reverse.
To clear a binding, click on one and then click outside.".Trim());
        keybindingsSubtitle.alignment = TextAnchor.UpperCenter;
        keybindingsSubtitle.GetComponent<LayoutElement>().preferredHeight = 100;

        {
            var searchGo = new GameObject();
            searchGo.transform.SetParent(transform, false);

            var layout = searchGo.AddComponent<LayoutElement>();
            layout.preferredHeight = 40f;

            var searchBackground = searchGo.AddComponent<Image>();
            searchBackground.raycastTarget = false;
            searchBackground.color = Color.white;

            var searchFieldGo = new GameObject();
            searchFieldGo.transform.SetParent(searchGo.transform, false);

            var searchFieldRect = searchFieldGo.AddComponent<RectTransform>();
            searchFieldRect.anchorMin = new Vector2(0f, 0f);
            searchFieldRect.anchorMax = new Vector2(1f, 1f);
            searchFieldRect.sizeDelta = new Vector2(1f, 1f);

            var text = searchFieldGo.AddComponent<Text>();
            text.font = prefabManager.font;
            text.color = Color.black;
            text.fontSize = 30;
            text.alignment = TextAnchor.MiddleLeft;

            var placeholderGo = new GameObject();
            placeholderGo.transform.SetParent(searchGo.transform, false);

            var placeholderRect = placeholderGo.AddComponent<RectTransform>();
            placeholderRect.anchorMin = new Vector2(0f, 0f);
            placeholderRect.anchorMax = new Vector2(1f, 1f);
            placeholderRect.sizeDelta = new Vector2(1f, 1f);

            var placeholderText = placeholderGo.AddComponent<Text>();
            placeholderText.raycastTarget = false;
            placeholderText.font = prefabManager.font;
            placeholderText.color = Color.gray;
            placeholderText.fontStyle = FontStyle.Italic;
            placeholderText.fontSize = 30;
            placeholderText.alignment = TextAnchor.MiddleLeft;
            placeholderText.text = "Search commands...";

            _searchInput = searchFieldGo.AddComponent<InputField>();
            _searchInput.textComponent = text;
            _searchInput.placeholder = placeholderText;
            _searchInput.onValueChanged.AddListener(_ => OnFilterChanged());
        }

        _showBoundOnly = prefabManager.CreateToggle(transform, "Only show bound commands");
        _showBoundOnly.backgroundColor = Color.clear;
        _showBoundOnly.toggle.onValueChanged.AddListener(_ => OnFilterChanged());
    }

    private void OnFilterChanged()
    {
        for (var i = 0; i < _rowGroups.Count; i++)
        {
            var rows = _rowGroups[i];
            var groupCounter = 0;
            for (var j = 1; j < rows.Count; j++)
            {
                var row = rows[j];
                if (row.commandName == null) continue;
                var active = FuzzyFinder.Match(row.commandName, _searchInput.text) && (!_showBoundOnly.toggle.isOn || !string.IsNullOrEmpty(row.bindingBtn1.label) || !string.IsNullOrEmpty(row.bindingBtn2.label));
                row.container.SetActive(active);
                if (active) groupCounter++;
            }
            rows[0].container.SetActive(groupCounter > 0);
        }
    }

    public void OnEnable()
    {
        Initialize();
        CreateRows();

        keyMapManager.onChanged.AddListener(OnKeybindingsChanged);
        analogMapManager.onChanged.AddListener(OnKeybindingsChanged);
    }

    private void CreateRows()
    {
        var unloadedCommands = keyMapManager.maps.Select(m => m.commandName).Concat(analogMapManager.maps.Select(m => m.commandName)).ToList();

        // ReSharper disable RedundantEnumerableCastCall
        var commands = remoteCommandsManager.actionCommands.Cast<ICommandInvoker>().Concat(remoteCommandsManager.analogCommands.Cast<ICommandInvoker>());
        // ReSharper restore RedundantEnumerableCastCall
        foreach (var ns in commands.GroupBy(c => c.ns))
        {
            var rowGroup = new List<CommandBindingRow>();
            _rowGroups.Add(rowGroup);
            AddGroupRow(rowGroup, ns.Key);

            foreach (var command in ns)
            {
                AddEditRow(rowGroup, command);
                unloadedCommands.Remove(command.commandName);
            }
        }

        if (unloadedCommands.Count <= 0) return;

        var disabledCommandInvokers = new List<ICommandInvoker>();
        foreach (var unloadedCommand in unloadedCommands)
        {
            var map = ((IMap) keyMapManager.maps.FirstOrDefault(m => m.commandName == unloadedCommand)) ?? analogMapManager.maps.FirstOrDefault(m => m.commandName == unloadedCommand);
            if (map == null) continue;
            disabledCommandInvokers.Add(new DisabledCommandInvoker(unloadedCommand, map.GetPrettyString(), map.slot));
        }

        foreach (var group in disabledCommandInvokers.GroupBy(c => c.ns))
        {
            var rowGroup = new List<CommandBindingRow>();
            _rowGroups.Add(rowGroup);
            AddGroupRow(rowGroup, $"[unloaded] {@group.Key}");

            foreach (var command in @group)
            {
                AddEditRow(rowGroup, command);
                unloadedCommands.Remove(command.commandName);
            }
        }
    }

    public void OnDisable()
    {
        keyMapManager.onChanged.RemoveListener(OnKeybindingsChanged);
        analogMapManager.onChanged.RemoveListener(OnKeybindingsChanged);

        if (_setKeybindingCoroutine != null) StopCoroutine(_setKeybindingCoroutine);
        _setKeybindingCoroutine = null;
        isRecording = false;
        ClearRows();
    }

    private void ClearRows()
    {
        foreach (var group in _rowGroups)
            foreach (var row in group)
                Destroy(row.container);
        _rowGroups.Clear();
    }

    private void OnKeybindingsChanged()
    {
        // TODO: Instead we could try to just update existing bindings and create/remove rows but this is easier
        ClearRows();
        CreateRows();
    }

    private void AddGroupRow(List<CommandBindingRow> rows, string groupName)
    {
        var go = new GameObject();
        go.transform.SetParent(transform, false);

        var row = new CommandBindingRow {container = go};
        rows.Add(row);

        go.transform.SetSiblingIndex(transform.childCount - 1);

        var layout = go.AddComponent<LayoutElement>();
        layout.preferredHeight = 70;

        var text = go.AddComponent<Text>();
        text.text = groupName;
        text.color = new Color(0.3f, 0.2f, 0.2f);
        text.raycastTarget = false;
        text.font = prefabManager.font;
        text.fontSize = 30;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleLeft;
    }

    private void AddEditRow(List<CommandBindingRow> rows, ICommandInvoker commandInvoker)
    {
        var go = new GameObject();
        go.transform.SetParent(transform, false);

        var row = new CommandBindingRow {container = go, invoker = commandInvoker};
        rows.Add(row);

        go.transform.SetSiblingIndex(transform.childCount - 1);

        var group = go.AddComponent<HorizontalLayoutGroup>();
        group.spacing = 10f;

        var isAnalog = commandInvoker is IAnalogCommandInvoker;

        var displayNameText = prefabManager.CreateText(go.transform, commandInvoker.localName + (isAnalog ? " *" : ""));
        if (isAnalog) displayNameText.color = new Color(0, 0.3f, 0.4f);
        var displayNameLayout = displayNameText.GetComponent<LayoutElement>();
        displayNameLayout.flexibleWidth = 1000f;

        row.bindingBtn1 = CreateBindingButton(commandInvoker, go, 0);
        row.bindingBtn2 = CreateBindingButton(commandInvoker, go, 1);
    }

    private UIDynamicButton CreateBindingButton(ICommandInvoker commandInvoker, GameObject go, int slot)
    {
        var bindingBtn = prefabManager.CreateButton(go.transform, GetMappedBinding(commandInvoker, slot));
        bindingBtn.button.onClick.AddListener(() =>
        {
            if (isRecording) return;
            if (commandInvoker is IActionCommandInvoker)
            {
                _setKeybindingCoroutine = StartCoroutine(RecordKeys(bindingBtn, commandInvoker, bindingBtn.buttonColor, slot));
                bindingBtn.label = "Type your shortcut...";
            }
            else if (commandInvoker is IAnalogCommandInvoker)
            {
                _setKeybindingCoroutine = StartCoroutine(RecordAnalog(bindingBtn, commandInvoker, bindingBtn.buttonColor, slot));
                bindingBtn.label = "Axis or 2 keys...";
            }
            bindingBtn.buttonColor = new Color(0.9f, 0.6f, 0.65f);
        });
        var bindingLayout = bindingBtn.GetComponent<LayoutElement>();
        bindingLayout.minWidth = 300f;
        bindingLayout.preferredWidth = 300f;
        return bindingBtn;
    }

    private void StopRecording(UIDynamicButton btn, Color btnColor, ICommandInvoker commandInvoker, int slot)
    {
        isRecording = false;
        btn.buttonColor = btnColor;
        btn.label = GetMappedBinding(commandInvoker, slot);
        if (_setKeybindingCoroutine != null) StopCoroutine(_setKeybindingCoroutine);
        _setKeybindingCoroutine = null;
    }

    private static readonly string[] _knownAxisNames = new[] {"Mouse X", "Mouse Y", "Mouse ScrollWheel", "LeftStickX", "LeftStickY", "RightStickX", "RightStickY", "Triggers", "DPadX", "DPadY", "Axis6", "Axis7", "Axis8"};

    private IEnumerator RecordAnalog(UIDynamicButton btn, ICommandInvoker commandInvoker, Color btnColor, int slot)
    {
        isRecording = true;
        var leftKeybinding = AnalogKeyChord.empty;
        while (true)
        {
            yield return 0;

            if(Input.GetKeyDown(KeyCode.Mouse0))
                continue;

            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                var mapped = analogMapManager.GetMapByName(commandInvoker.commandName, slot);
                if (mapped != null)
                    analogMapManager.maps.Remove(mapped);
                break;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
                break;

            var altDown = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);

            var keyUp = KeyCodes.bindableKeyCodes.GetCurrentUp();

            if (keyUp != KeyCode.None)
            {
                if (!KeyChordUtils.IsValid(keyUp))
                    continue;

                var binding = new AnalogKeyChord(keyUp, altDown);
                if (leftKeybinding.key == KeyCode.None)
                {
                    btn.label = "Type other direction...";
                    leftKeybinding = binding;
                    continue;
                }
                else if (keyUp == leftKeybinding.key)
                {
                    continue;
                }

                var map = new AnalogMap(leftKeybinding, binding, commandInvoker.commandName, slot);
                SaveAnalogMap(commandInvoker, binding, map);
                StopRecording(btn, btnColor, commandInvoker, slot);
                yield break;
            }

            foreach (var axisName in _knownAxisNames)
            {
                var axisValue = AnalogMap.GetAxis(axisName);
                if (Mathf.Abs(axisValue) < 0.75f) continue;
                var key = KeyCodes.bindableKeyCodes.GetCurrent();
                // We don't want to take over the mouse!
                if (axisName.StartsWith("Mouse") && key == KeyCode.None && !altDown) continue;
                var binding = new AnalogKeyChord(key, altDown);
                var map = new AnalogMap(binding, axisName, axisValue < 0, commandInvoker.commandName, slot);

                SaveAnalogMap(commandInvoker, binding, map);
                StopRecording(btn, btnColor, commandInvoker, slot);
                yield break;
            }
        }

        StopRecording(btn, btnColor, commandInvoker, slot);
    }

    private void SaveAnalogMap(ICommandInvoker commandInvoker, AnalogKeyChord binding, AnalogMap map)
    {
        var previousMap = analogMapManager.maps.FirstOrDefault(m => m.commandName == commandInvoker.commandName);
        if (previousMap != null)
        {
            remoteCommandsManager.UpdateValue(previousMap.commandName, 0);
            analogMapManager.maps.Remove(previousMap);
        }

        var conflictMap = analogMapManager.maps.FirstOrDefault(m => m.chord.Equals(binding) && m.axisName == map.axisName && m.leftChord.Equals(map.leftChord) && m.rightChord.Equals(map.rightChord));
        if (conflictMap != null)
        {
            remoteCommandsManager.UpdateValue(conflictMap.commandName, 0);
            analogMapManager.maps.Remove(conflictMap);
            var conflictRow = _rowGroups.SelectMany(g => g).FirstOrDefault(r => r.commandName == conflictMap.commandName);
            if (conflictRow != null)
            {
                if (conflictMap.slot == 0)
                    conflictRow.bindingBtn1.label = _notBoundButtonLabel;
                else if (conflictMap.slot == 0)
                    conflictRow.bindingBtn2.label = _notBoundButtonLabel;
                else
                    throw new InvalidOperationException("Unknown slot " + conflictMap.slot);
            }

            SuperController.LogMessage($"Keybindings: Reassigned binding from {conflictMap.commandName} to {commandInvoker.commandName}");
        }

        analogMapManager.maps.Add(map);
    }

    private IEnumerator RecordKeys(UIDynamicButton btn, ICommandInvoker commandInvoker, Color btnColor, int slot)
    {
        isRecording = true;
        var expire = float.MaxValue;
        var setKeybindingList = new List<KeyChord>();
        while (Time.unscaledTime < expire)
        {
            yield return 0;
            if (Input.GetKeyDown(KeyCode.Mouse0))
                continue;
            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                // Apply
                SaveKeyMap(setKeybindingList, commandInvoker, slot);
                StopRecording(btn, btnColor, commandInvoker, slot);
                yield break;
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // Cancel
                StopRecording(btn, btnColor, commandInvoker, slot);
                yield break;
            }

            var key = KeyCodes.bindableKeyCodes.GetCurrentDown();
            if (key == KeyCode.None) continue;

            if (!KeyChordUtils.IsValid(key)) continue;

            var ctrlDown = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            var altDown = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            var shiftDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            var binding = new KeyChord(key, ctrlDown, altDown, shiftDown);
            setKeybindingList.Add(binding);
            btn.label = setKeybindingList.GetKeyChordsAsString();
            expire = Time.unscaledTime + Settings.TimeoutLen;
        }
        SaveKeyMap(setKeybindingList, commandInvoker, slot);
        StopRecording(btn, btnColor, commandInvoker, slot);
    }

    private void SaveKeyMap(List<KeyChord> setKeybindingList, ICommandInvoker commandInvoker, int slot)
    {
        if (setKeybindingList.Count > 0)
        {
            var bindings = setKeybindingList.ToArray();
            var previousMap = keyMapManager.maps.FirstOrDefault(m => m.commandName == commandInvoker.commandName);
            if (previousMap != null)
                keyMapManager.maps.Remove(previousMap);
            var conflictMap = keyMapManager.maps.FirstOrDefault(m => m.chords.SameBinding(bindings));
            if (conflictMap != null)
            {
                keyMapManager.maps.Remove(conflictMap);
                var conflictRow = _rowGroups.SelectMany(g => g).FirstOrDefault(r => r.commandName == conflictMap.commandName);
                if (conflictRow != null)
                {
                    if (conflictMap.slot == 0)
                        conflictRow.bindingBtn1.label = _notBoundButtonLabel;
                    else if (conflictMap.slot == 0)
                        conflictRow.bindingBtn2.label = _notBoundButtonLabel;
                    else
                        throw new InvalidOperationException("Unknown slot " + conflictMap.slot);
                }
                SuperController.LogMessage($"Keybindings: Reassigned binding from {conflictMap.commandName} to {commandInvoker.commandName}");
            }
            keyMapManager.maps.Add(new KeyMap(bindings, commandInvoker.commandName, slot));
            keyMapManager.RebuildTree();
        }
        else
        {
            var mapped = keyMapManager.GetMapByName(commandInvoker.commandName, slot);
            if (mapped != null)
                keyMapManager.maps.Remove(mapped);
        }
    }

    private string GetMappedBinding(ICommandInvoker commandInvoker, int slot)
    {
        if (commandInvoker is IActionCommandInvoker)
        {
            var mapped = keyMapManager.GetMapByName(commandInvoker.commandName, slot);
            return mapped?.GetPrettyString() ?? _notBoundButtonLabel;
        }

        if (commandInvoker is IAnalogCommandInvoker)
        {
            var mapped = analogMapManager.GetMapByName(commandInvoker.commandName, slot);
            return mapped?.GetPrettyString() ?? _notBoundButtonLabel;
        }

        var disabledCommandInvoker = commandInvoker as DisabledCommandInvoker;
        if (disabledCommandInvoker != null && disabledCommandInvoker.slot == slot)
        {
            return disabledCommandInvoker.prettyString;
        }

        return _notBoundButtonLabel;
    }
}
