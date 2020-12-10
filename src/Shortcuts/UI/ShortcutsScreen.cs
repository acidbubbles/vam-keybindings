using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ShortcutsScreen : MonoBehaviour
{
    // TODO: Provide helpful labels (how could we do that?)
    // TODO: Group by plugin/storable
    // TODO: Do we really need Invoke in there?
    // TODO: Search (just hide the rows)

    private class Row
    {
        public string action;
        public GameObject container;
        public UIDynamicButton bindingBtn;
    }

    public IPrefabManager prefabManager { get; set; }
    public IKeyMapManager keyMapManager { get; set; }
    public RemoteActionsManager remoteActionsManager { get; set; }
    public bool isRecording;
    private readonly List<Row> _rows = new List<Row>();
    private Coroutine _setKeybindingCoroutine;
    private readonly List<KeyChord> _setKeybindingList = new List<KeyChord>();
    private UIDynamicButton _setBindingBtn;
    private IAction _setBindingAction;
    private Color _setBindingRestoreColor;

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

    public void Awake()
    {
        var title = prefabManager.CreateText(transform, "<b>Shortcuts</b>");
        title.fontSize = 30;
        title.alignment = TextAnchor.MiddleCenter;

        var subtitle = prefabManager.CreateText(transform, "<i>You can configure custom shortcuts in CustomActionsPlugin</i>");
        subtitle.alignment = TextAnchor.UpperCenter;
    }

    public void OnEnable()
    {
        foreach (var actionName in remoteActionsManager.names)
        {
            IAction action;
            if (remoteActionsManager.TryGetAction(actionName, out action))
                AddEditRow(action);
        }

        // TODO: Shortcuts mapped to nothing?
    }

    private void ClearRows()
    {
        foreach (var row in _rows)
            Destroy(row.container);
        _rows.Clear();
    }

    private void AddEditRow(IAction action)
    {
        var go = new GameObject();
        go.transform.SetParent(transform, false);

        var row = new Row {container = go, action = action.name};
        _rows.Add(row);

        go.transform.SetSiblingIndex(transform.childCount - 2);

        var group = go.AddComponent<HorizontalLayoutGroup>();
        group.spacing = 10f;

        var displayNameText = prefabManager.CreateText(go.transform, action.label);
        var displayNameLayout = displayNameText.GetComponent<LayoutElement>();
        displayNameLayout.flexibleWidth = 1000f;

        var bindingBtn = prefabManager.CreateButton(go.transform, GetMappedBinding(action));
        bindingBtn.button.onClick.AddListener(() =>
        {
            StopRecording();
            _setBindingBtn = bindingBtn;
            _setBindingAction = action;
            _setBindingRestoreColor = bindingBtn.buttonColor;
            _setKeybindingCoroutine = StartCoroutine(SetKeybinding());
            bindingBtn.buttonColor = new Color(0.9f, 0.6f, 0.65f);
            bindingBtn.label = "Recording...";
        });
        row.bindingBtn = bindingBtn;
        var bindingLayout = bindingBtn.GetComponent<LayoutElement>();
        bindingLayout.minWidth = 400f;
        bindingLayout.preferredWidth = 400f;

        var editBtn = prefabManager.CreateButton(go.transform, "Invoke");
        editBtn.button.onClick.AddListener(action.Invoke);
        var editLayout = editBtn.GetComponent<LayoutElement>();
        editLayout.minWidth = 160f;
        editLayout.preferredWidth = 160f;

        var clearBtn = prefabManager.CreateButton(go.transform, "X");
        clearBtn.button.onClick.AddListener(() =>
        {
            var mapped = keyMapManager.GetMapByName(action.name);
            if (mapped != null)
                keyMapManager.maps.Remove(mapped);
            bindingBtn.label = "-";
        });
        var clearLayout = clearBtn.GetComponent<LayoutElement>();
        clearLayout.minWidth = 40f;
        clearLayout.preferredWidth = 40f;
    }

    private void StopRecording()
    {
        if (!isRecording) return;
        isRecording = false;
        _setKeybindingList.Clear();
        _setBindingBtn.buttonColor = _setBindingRestoreColor;
        _setBindingBtn.label = GetMappedBinding(_setBindingAction);
        if (_setKeybindingCoroutine != null) StopCoroutine(_setKeybindingCoroutine);
    }

    private IEnumerator SetKeybinding()
    {
        isRecording = true;
        // TODO: Multi-key shortcuts might confuse people. Make it optional?
        var expire = float.MaxValue;
        while (Time.unscaledTime < expire)
        {
            yield return 0;
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                // Apply
                ApplyRecordedKeybinding();
                yield break;
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // Cancel
                StopRecording();
                yield break;
            }

            var key = KeyCodes.bindableKeyCodes.GetCurrent();
            if (key == KeyCode.None) continue;

            var binding = new KeyChord(key, Input.GetKey(KeyCode.LeftControl) ? KeyCode.LeftControl : KeyCode.None);
            _setKeybindingList.Add(binding);
            _setBindingBtn.label = _setKeybindingList.GetKeyChordsAsString();
            expire = Time.unscaledTime + Settings.TimeoutLen;
        }
        ApplyRecordedKeybinding();
    }

    private void ApplyRecordedKeybinding()
    {
        if (_setKeybindingList.Count > 0)
        {
            var bindings = _setKeybindingList.ToArray();
            var previousMap = keyMapManager.maps.FirstOrDefault(m => m.action == _setBindingAction.name);
            if (previousMap != null)
                keyMapManager.maps.Remove(previousMap);
            var conflictMap = keyMapManager.maps.FirstOrDefault(m => m.chords.SameBinding(bindings));
            if (conflictMap != null)
            {
                keyMapManager.maps.Remove(conflictMap);
                var conflictRow = _rows.FirstOrDefault(r => r.action == conflictMap.action);
                if (conflictRow != null)
                {
                    conflictRow.bindingBtn.label = "-";
                }
                SuperController.LogError($"Reassigned binding from {conflictMap.action} to {_setBindingAction.name}");
            }
            // TODO: Detect when a key binding already exists and will be overwritten
            keyMapManager.maps.Add(new KeyMap(bindings, _setBindingAction.name));
            keyMapManager.RebuildTree();
        }
        StopRecording();
    }

    private string GetMappedBinding(IAction action)
    {
        var mapped = keyMapManager.GetMapByName(action.name);
        return mapped != null
            ? mapped.chords.GetKeyChordsAsString()
            : "-";
    }

    public void OnDisable()
    {
        ClearRows();
        StopRecording();
    }
}
