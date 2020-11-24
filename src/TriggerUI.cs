using System.Collections;
using AssetBundles;
using UnityEngine;

public interface ITriggerUI
{
    Transform triggerActionsParent { get; }
    RectTransform triggerActionsPrefab { get; }
    RectTransform triggerActionMiniPrefab { get; }
    RectTransform triggerActionDiscretePrefab { get; }
    RectTransform triggerActionTransitionPrefab { get; }
}

public class TriggerUI : ITriggerUI
{
    public Transform triggerActionsParent { get; set; }
    public RectTransform triggerActionsPrefab { get; private set; }
    public RectTransform triggerActionMiniPrefab { get; private set; }
    public RectTransform triggerActionDiscretePrefab { get; private set; }
    public RectTransform triggerActionTransitionPrefab { get; private set; }

    public IEnumerator LoadUIAssets()
    {
        AssetBundleLoadAssetOperation request =
            AssetBundleManager.LoadAssetAsync("z_ui2", "TriggerActionsPanel", typeof(GameObject));
        if (request == null)
        {
            SuperController.LogError("Request for TriggerActionsPanel in z_ui2 assetbundle failed");
            yield break;
        }

        yield return request;
        GameObject go = request.GetAsset<GameObject>();
        if (go == null)
        {
            SuperController.LogError("Failed to load TriggerActionsPanel asset");
        }

        triggerActionsPrefab = go.GetComponent<RectTransform>();
        if (triggerActionsPrefab == null)
        {
            SuperController.LogError("Failed to load TriggerActionsPanel asset");
        }

        request = AssetBundleManager.LoadAssetAsync("z_ui2", "TriggerActionMiniPanel", typeof(GameObject));
        if (request == null)
        {
            SuperController.LogError("Request for TriggerActionMiniPanel in z_ui2 assetbundle failed");
            yield break;
        }

        yield return request;
        go = request.GetAsset<GameObject>();
        if (go == null)
        {
            SuperController.LogError("Failed to load TriggerActionMiniPanel asset");
        }

        triggerActionMiniPrefab = go.GetComponent<RectTransform>();
        if (triggerActionMiniPrefab == null)
        {
            SuperController.LogError("Failed to load TriggerActionMiniPanel asset");
        }

        request = AssetBundleManager.LoadAssetAsync("z_ui2", "TriggerActionDiscretePanel", typeof(GameObject));
        if (request == null)
        {
            SuperController.LogError("Request for TriggerActionDiscretePanel in z_ui2 assetbundle failed");
            yield break;
        }

        yield return request;
        go = request.GetAsset<GameObject>();
        if (go == null)
        {
            SuperController.LogError("Failed to load TriggerActionDiscretePanel asset");
        }

        triggerActionDiscretePrefab = go.GetComponent<RectTransform>();
        if (triggerActionDiscretePrefab == null)
        {
            SuperController.LogError("Failed to load TriggerActionDiscretePanel asset");
        }

        request = AssetBundleManager.LoadAssetAsync("z_ui2", "TriggerActionTransitionPanel", typeof(GameObject));
        if (request == null)
        {
            SuperController.LogError("Request for TriggerActionTransitionPanel in z_ui2 assetbundle failed");
            yield break;
        }

        yield return request;
        go = request.GetAsset<GameObject>();
        if (go == null)
        {
            SuperController.LogError("Failed to load TriggerActionTransitionPanel asset");
        }

        triggerActionTransitionPrefab = go.GetComponent<RectTransform>();
        if (triggerActionTransitionPrefab == null)
        {
            SuperController.LogError("Failed to load TriggerActionTransitionPanel asset");
        }
    }
}
