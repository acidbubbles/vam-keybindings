using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class KeybindingsOverlayReference
{
    public KeybindingsOverlay value { get; set; }
}

public class KeybindingsOverlay : MonoBehaviour
{
    public static KeybindingsOverlay CreateOverlayGameObject(PrefabManager prefabManager)
    {
        // Heavily inspired by hazmox's VAMOverlays https://hub.virtamate.com/resources/vamoverlays.2438/
        // Thanks a lot for allowing me to use the result of your hard work!

        var go = new GameObject(nameof(KeybindingsOverlay)) {layer = 5};
        go.SetActive(false);
        try
        {
            go.transform.SetParent(SuperController.singleton.MonitorCenterCamera.transform, false);

            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.sortingOrder = LayerMask.NameToLayer("ScreenUI");
            canvas.worldCamera = CameraTarget.centerTarget.targetCamera;

            // To make the area visible
            // var bg = go.AddComponent<Image>();
            // bg.raycastTarget = false;
            // bg.color = new Color(1f, 1f, 1f, 0.01f);

            var overlay = go.AddComponent<KeybindingsOverlay>();
            overlay._canvas = canvas;

            {
                var textContainerGo = new GameObject("text") {layer = 5};
                textContainerGo.transform.SetParent(go.transform, false);

                var textRect = textContainerGo.AddComponent<RectTransform>();
                textRect.pivot = new Vector2(1, 0);
                textRect.anchorMin = new Vector2(0, 0);
                textRect.anchorMax = new Vector2(1, 0);
                textRect.anchoredPosition = new Vector2(-15, 10f);

                var text = textContainerGo.AddComponent<Text>();
                text.raycastTarget = false;
                text.color = Color.white;
                text.font = prefabManager.font;
                text.fontSize = 28;
                text.alignment = TextAnchor.LowerRight;
                overlay.text = text;

                var textShadow = textContainerGo.AddComponent<Shadow>();
                textShadow.effectColor = Color.black;
                textShadow.effectDistance = new Vector2(2f, -3f);
            }

            {
                var textInputGo = new GameObject();
                textInputGo.transform.SetParent(go.transform, false);

                var textRect = textInputGo.AddComponent<RectTransform>();
                textRect.pivot = new Vector2(1, 0);
                textRect.anchorMin = new Vector2(0, 0);
                textRect.anchorMax = new Vector2(1, 0);
                textRect.anchoredPosition = new Vector2(-18, -20f);

                var text = textInputGo.AddComponent<Text>();
                text.raycastTarget = true;
                text.font = prefabManager.font;
                text.alignment = TextAnchor.UpperRight;
                text.fontSize = 16;

                var textShadow = textInputGo.AddComponent<Shadow>();
                textShadow.effectColor = Color.black;
                textShadow.effectDistance = new Vector2(1f, -1.5f);

                var input = textInputGo.AddComponent<InputField>();
                input.textComponent = text;
                input.interactable = true;
                input.selectionColor = new Color(0f, 0.3f, 0f, 0.1f);
                overlay.input = input;
            }

            return overlay;
        }
        catch (Exception e)
        {
            Destroy(go);
            SuperController.LogError("Keybindings: Failed creating overlays" + e);
            return null;
        }
    }

    public Text text;
    public InputField input;
    public float autoClear { get; set; }
    private Coroutine _autoClearCoroutine;
    private float _clearTime;
    private Canvas _canvas;

    public void Append(string value)
    {
        if (text.text == "")
            Set(value);
        else
            Set(text.text + " " + value);
    }

    public void Set(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            text.text = "";
            gameObject.SetActive(false);
            if (_autoClearCoroutine != null) StopCoroutine(_autoClearCoroutine);
            _autoClearCoroutine = null;
            return;
        }

        if (!gameObject.activeSelf)
        {
            var camera = CameraTarget.centerTarget.targetCamera;
            var hook = CameraTarget.centerTarget.targetCamera.transform.Find("CameraHook");
            if (hook != null)
            {
                var hookedCamera = hook.GetComponent<Camera>();
                if (hookedCamera != null && hookedCamera.enabled)
                    camera = hookedCamera;
            }
            _canvas.worldCamera = camera;
        }
        gameObject.SetActive(true);
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
        gameObject.SetActive(false);
    }
}
