using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShortcutsScreen : MonoBehaviour
{
    private class Row
    {
        public IAction action;
        public GameObject container;
    }

    public IPrefabManager prefabManager { get; set; }
    public IBindingsManager bindingsManager { get; set; }
    public RemoteActionsManager remoteActionsManager { get; set; }
    private readonly List<Row> _rows = new List<Row>();

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
        foreach (var action in remoteActionsManager.ToList())
        {
            AddEditRow(action);
        }
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

        var row = new Row {container = go, action = action};
        _rows.Add(row);

        go.transform.SetSiblingIndex(transform.childCount - 2);

        var group = go.AddComponent<HorizontalLayoutGroup>();
        group.spacing = 10f;

        var text = prefabManager.CreateText(go.transform, action.displayName);
        var textLayout = text.GetComponent<LayoutElement>();
        textLayout.flexibleWidth = 1000f;

        var btnEdit = prefabManager.CreateButton(go.transform, "Invoke");
        btnEdit.button.onClick.AddListener(action.Invoke);
        var btnLayout = btnEdit.GetComponent<LayoutElement>();
        btnLayout.minWidth = 160f;
        btnLayout.preferredWidth = 160f;
    }

    public void OnDisable()
    {
        ClearRows();
    }
}
