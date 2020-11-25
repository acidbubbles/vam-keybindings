using System;
using System.Collections;
using AssetBundles;
using UnityEngine;

public interface IPrefabManager
{
    Transform triggerActionsParent { get; }
    RectTransform triggerActionsPrefab { get; }
    RectTransform triggerActionMiniPrefab { get; }
    RectTransform triggerActionDiscretePrefab { get; }
    RectTransform triggerActionTransitionPrefab { get; }
}

public class PrefabManager : IPrefabManager
{
    public Transform triggerActionsParent { get; set; }
    public RectTransform triggerActionsPrefab { get; private set; }
    public RectTransform triggerActionMiniPrefab { get; private set; }
    public RectTransform triggerActionDiscretePrefab { get; private set; }
    public RectTransform triggerActionTransitionPrefab { get; private set; }

    public IEnumerator LoadUIAssets()
    {
        foreach (var r in LoadUIAsset("TriggerActionsPanel", x => triggerActionsPrefab = x)) yield return r;
        foreach (var r in LoadUIAsset("TriggerActionMiniPanel", x => triggerActionMiniPrefab = x)) yield return r;
        foreach (var r in LoadUIAsset("TriggerActionDiscretePanel", x => triggerActionDiscretePrefab = x))
            yield return r;
        foreach (var r in LoadUIAsset("TriggerActionTransitionPanel", x => triggerActionTransitionPrefab = x))
            yield return r;
    }

    private static IEnumerable LoadUIAsset(string assetName, Action<RectTransform> assignPrefab)
    {
        var request = AssetBundleManager.LoadAssetAsync("z_ui2", assetName, typeof(GameObject));
        if (request == null)
        {
            SuperController.LogError($"Request for {assetName} in z_ui2 assetbundle failed");
            yield break;
        }

        yield return request;

        var go = request.GetAsset<GameObject>();
        if (go == null)
        {
            SuperController.LogError("Failed to load asset's GameObject");
            yield break;
        }

        var rectTransform = go.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            SuperController.LogError("Failed to get asset RectTransform");
            yield break;
        }

        assignPrefab(rectTransform);
    }
}
