using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR;

public class KeybindingsOverlay : MonoBehaviour
{
    public static KeybindingsOverlay CreateOverlayGameObject(PrefabManager prefabManager)
    {
        // Heavily inspired by hazmox's VAMOverlays https://hub.virtamate.com/resources/vamoverlays.2438/
        // Thanks a lot for allowing me to use the result of your hard work!

        var mainCamera = Camera.main;
        if (mainCamera == null) return null;
        // TODO: We could use something that also works for monitor mode
        var isVR = XRDevice.isPresent;
        var go = new GameObject(nameof(KeybindingsOverlay)) {layer = 5};
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
            rectTransform.pivot = new Vector2(1, 0);
            rectTransform.sizeDelta = new Vector2(mainCamera.scaledPixelWidth/1.33f, mainCamera.scaledPixelHeight/12f);
            // TODO: Move that point in screen space for Desktop, and world space in VR (the former needs to be updated as the window is resized)
            rectTransform.localPosition = new Vector3(0.4f, -0.2f, 0.8f);
            rectTransform.localScale = new Vector3(0.00050f, 0.00050f, 1.0f);

            // To make the area visible
            // var bg = go.AddComponent<Image>();
            // bg.raycastTarget = false;
            // bg.color = new Color(1f, 1f, 1f, 0.01f);

            var overlay = go.AddComponent<KeybindingsOverlay>();

            {
                var textContainerGo = new GameObject("text") {layer = 5};
                textContainerGo.transform.SetParent(go.transform, false);

                textContainerGo.AddComponent<CanvasRenderer>();

                var textRect = textContainerGo.AddComponent<RectTransform>();
                textRect.pivot = new Vector2(1, 0);
                textRect.anchorMin = new Vector2(0, 0);
                textRect.anchorMax = new Vector2(1, 0);

                var text = textContainerGo.AddComponent<Text>();
                text.raycastTarget = false;
                text.color = Color.white;
                text.font = prefabManager.font;
                text.fontSize = isVR ? 18 : 36;
                text.alignment = TextAnchor.LowerRight;
                overlay.text = text;

                var textShadow = textContainerGo.AddComponent<Shadow>();
                textShadow.effectColor = Color.black;
                textShadow.effectDistance = new Vector2(2f, -0.5f);
            }

            {
                var textInputGo = new GameObject();
                textInputGo.transform.SetParent(go.transform, false);

                var textRect = textInputGo.AddComponent<RectTransform>();
                textRect.anchorMin = new Vector2(0, 1);
                textRect.anchorMax = new Vector2(1, 1);
                textRect.pivot = new Vector2(1, 1);
                textRect.sizeDelta = new Vector2(400f, 40f);

                var text = textInputGo.AddComponent<Text>();
                text.raycastTarget = true;
                text.font = prefabManager.font;
                text.alignment = TextAnchor.UpperRight;
                text.fontSize = 12;

                var input = textInputGo.AddComponent<InputField>();
                input.textComponent = text;
                input.interactable = true;
                overlay.input = input;
            }

            X();
            return overlay;
        }
        catch (Exception e)
        {
            Destroy(go);
            SuperController.LogError("Keybindings: Failed creating overlays" + e);
            return null;
        }
    }

    public static object X()
    {
        return new object();
    }

    public Text text;
    public InputField input;
    public float autoClear { get; set; }
    private Coroutine _autoClearCoroutine;
    private float _clearTime;

    public void Append(string value)
    {
        if (text.text == "")
            Set(value);
        else
            Set(text.text + " " + value);
    }

    public void Set(string value)
    {
        text.text = value;
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
