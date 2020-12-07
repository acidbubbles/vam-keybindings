using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class ShortcutsOverlay : MonoBehaviour
{
    public static ShortcutsOverlay CreateOverlayGameObject(PrefabManager prefabManager)
    {
        // Heavily inspired by hazmox's VAMOverlays https://hub.virtamate.com/resources/vamoverlays.2438/
        // Thanks a lot for allowing me to use the result of your hard work!

        var mainCamera = Camera.main;
        if (mainCamera == null) return null;
        // TODO: We could use something that also works for monitor mode
        var isVR = XRDevice.isPresent;
        var go = new GameObject("ShortcutsPluginOverlay") {layer = 5};
        try
        {
            // TODO: Will this allow switching from/to VR?
            go.transform.SetParent(mainCamera.transform, false);

            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 2;
            canvas.worldCamera = mainCamera;
            canvas.planeDistance = 10.0f;

            go.AddComponent<CanvasScaler>();

            var rectTransform = go.GetComponent<RectTransform>();
            // TODO: Handle screen resize
            rectTransform.sizeDelta = new Vector2(mainCamera.scaledPixelWidth, mainCamera.scaledPixelHeight);
            rectTransform.localPosition = new Vector3(0, 0, 0.8f);
            rectTransform.localScale = new Vector3(0.00050f, 0.00050f, 1.0f);

            // var bg = go.AddComponent<Image>();
            // bg.raycastTarget = false;
            // bg.color = Color.blue;

            var textContainerGo = new GameObject("text") {layer = 5};
            textContainerGo.transform.SetParent(go.transform, false);

            textContainerGo.AddComponent<CanvasRenderer>();

            var textRect = textContainerGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;

            var text = textContainerGo.AddComponent<Text>();
            text.raycastTarget = false;
            // text.canvasRenderer.SetAlpha(0.0f);
            text.color = Color.white;
            text.text = "";
            text.font = prefabManager.font;
            text.fontSize = isVR ? 18 : 48;
            text.alignment = TextAnchor.LowerRight;

            var textShadow = textContainerGo.AddComponent<Shadow>();
            textShadow.effectColor = Color.black;
            textShadow.effectDistance = new Vector2(2f, -0.5f);

            var overlay = go.AddComponent<ShortcutsOverlay>();
            overlay.text = text;

            return overlay;
        }
        catch (Exception e)
        {
            Destroy(go);
            SuperController.LogError("Shortcuts: Failed creating overlays" + e);
            return null;
        }
    }

    public Text text;
    public float autoClear { get; set; }
    private Coroutine _autoClearCoroutine;
    private float _clearTime;

    public void Draw(string value)
    {
        if (text.text == "")
            text.text = value;
        else
            text.text += " " + value;
        _clearTime = Time.unscaledTime + autoClear;
        if (_autoClearCoroutine == null) _autoClearCoroutine = StartCoroutine(AutoClearCoroutine());
    }

    private IEnumerator AutoClearCoroutine()
    {
        while (Time.unscaledTime < _clearTime)
        {
            yield return 0;
        }
        text.text = "";
        _autoClearCoroutine = null;
        // TODO: We could completely disable the canvas here
    }
}
