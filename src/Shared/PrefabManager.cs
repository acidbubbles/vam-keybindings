﻿using System;
using System.Collections;
using AssetBundles;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public interface IPrefabManager
{
    Font font { get; }
    Transform triggerActionsParent { get; }
    RectTransform triggerActionsPrefab { get; }
    RectTransform triggerActionMiniPrefab { get; }
    RectTransform triggerActionDiscretePrefab { get; }
    RectTransform triggerActionTransitionPrefab { get; }

    Transform CreateSpacer(Transform transform, float height);
    Text CreateText(Transform transform, string label);
    UIDynamicButton CreateButton(Transform transform, string label);
    UIDynamicToggle CreateToggle(Transform transform, string label);
}

public class PrefabManager : IPrefabManager
{
    public Font font { get; private set; }
    public Transform triggerActionsParent { get; set; }
    public RectTransform triggerActionsPrefab { get; private set; }
    public RectTransform triggerActionMiniPrefab { get; private set; }
    public RectTransform triggerActionDiscretePrefab { get; private set; }
    public RectTransform triggerActionTransitionPrefab { get; private set; }
    public RectTransform buttonPrefab { get; private set; }
    public RectTransform togglePrefab { get; private set; }

    public IEnumerator LoadUIAssets()
    {
        font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        foreach (var r in LoadUIAsset("TriggerActionsPanel", x => triggerActionsPrefab = x)) yield return r;
        foreach (var r in LoadUIAsset("TriggerActionMiniPanel", x => triggerActionMiniPrefab = x)) yield return r;
        foreach (var r in LoadUIAsset("TriggerActionDiscretePanel", x => triggerActionDiscretePrefab = x)) yield return r;
        foreach (var r in LoadUIAsset("TriggerActionTransitionPanel", x => triggerActionTransitionPrefab = x)) yield return r;
        foreach (var r in LoadUIAsset("DynamicButton", x => buttonPrefab = x)) yield return r;
        foreach (var r in LoadUIAsset("DynamicToggle", x => togglePrefab = x)) yield return r;
    }

    public Transform CreateSpacer(Transform transform, float height)
    {
        var go = new GameObject();
        go.transform.SetParent(transform, false);

        go.AddComponent<LayoutElement>().minHeight = height;

        return go.transform;
    }

    public Text CreateText(Transform transform, string label)
    {
        var go = new GameObject();
        go.transform.SetParent(transform, false);

        var layout = go.AddComponent<LayoutElement>();
        layout.preferredHeight = 36;

        var text = go.AddComponent<Text>();
        text.font = font;
        text.raycastTarget = false;
        text.alignment = TextAnchor.MiddleLeft;
        text.color = Color.black;
        text.fontSize = 26;
        text.text = label;
        return text;
    }

    public UIDynamicButton CreateButton(Transform transform, string label)
    {
        var ui = Object.Instantiate(buttonPrefab).GetComponent<UIDynamicButton>();
        ui.gameObject.transform.SetParent(transform, false);
        ui.label = label;
        return ui;
    }

    public UIDynamicToggle CreateToggle(Transform transform, string label)
    {
        var ui = Object.Instantiate(togglePrefab).GetComponent<UIDynamicToggle>();
        ui.gameObject.transform.SetParent(transform, false);
        ui.label = label;
        return ui;
    }

    private static IEnumerable LoadUIAsset(string assetName, Action<RectTransform> assignPrefab)
    {
        var request = AssetBundleManager.LoadAssetAsync("z_ui2", assetName, typeof(GameObject));
        if (request == null)
        {
            SuperController.LogError($"Keybindings: Request for {assetName} in z_ui2 assetbundle failed");
            yield break;
        }

        yield return request;

        var go = request.GetAsset<GameObject>();
        if (go == null)
        {
            SuperController.LogError($"Keybindings: Failed to load asset {assetName} GameObject");
            yield break;
        }

        var rectTransform = go.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            SuperController.LogError($"Keybindings: Failed to get asset {assetName} RectTransform");
            yield break;
        }

        assignPrefab(rectTransform);
    }
}
