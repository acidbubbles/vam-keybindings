using System.Collections.Generic;
using CustomActions;
using UnityEngine;
using UnityEngine.UI;

public class CustomActionsScreen : MonoBehaviour
{
    private class Row
    {
        public IBoundAction action;
        public GameObject container;
    }

    public IPrefabManager prefabManager { get; set; }
    public IActionsRepository actions { get; set; }

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
        var title = prefabManager.CreateText(transform, "<b>Custom actions</b>");
        title.fontSize = 30;
        title.alignment = TextAnchor.MiddleCenter;

        var subtitle = prefabManager.CreateText(transform, "<i>The <b>[name]</b> can be invoked by a shortcut.</i>");
        subtitle.alignment = TextAnchor.UpperCenter;

        var addBtn = prefabManager.CreateButton(transform, "+ Add custom action");
        addBtn.button.onClick.AddListener(() =>
        {
            var action = actions.AddDiscreteTrigger();
            AddEditRow(action);
        });
    }

    public void OnEnable()
    {
        foreach (IBoundAction action in actions)
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

    private void AddEditRow(IBoundAction action)
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

        var btnEdit = prefabManager.CreateButton(go.transform, "Edit");
        btnEdit.button.onClick.AddListener(() =>
        {
            var e = action.Edit();
            e?.AddListener(() =>
            {
                actions.onChange.Invoke();
                text.text = action.displayName;
            });
        });
        var btnLayout = btnEdit.GetComponent<LayoutElement>();
        btnLayout.minWidth = 160f;
        btnLayout.preferredWidth = 160f;

        var btnRemove = prefabManager.CreateButton(go.transform, "X");
        btnRemove.button.onClick.AddListener(() =>
        {
            actions.Remove(action);
            _rows.Remove(row);
            Destroy(go);
        });
        btnRemove.buttonColor = Color.red;
        btnRemove.textColor = Color.white;
        var btnRemoveLayout = btnRemove.GetComponent<LayoutElement>();
        btnRemoveLayout.minWidth = 46f;
        btnRemoveLayout.preferredWidth = 46f;
    }

    public void OnDisable()
    {
        ClearRows();
    }
}
