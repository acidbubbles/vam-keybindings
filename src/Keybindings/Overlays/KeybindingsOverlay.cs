using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class KeybindingsOverlay : MonoBehaviour
{
    public static KeybindingsOverlay CreateOverlayGameObject(PrefabManager prefabManager)
    {
        // Heavily inspired by hazmox's VAMOverlays https://hub.virtamate.com/resources/vamoverlays.2438/
        // Thanks a lot for allowing me to use the result of your hard work!

        var mainCamera = Camera.main;
        if (mainCamera == null) return null;
        var go = new GameObject(nameof(KeybindingsOverlay)) {layer = 5};
        try
        {
            go.transform.SetParent(mainCamera.transform, false);

            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.sortingOrder = 2;
            canvas.worldCamera = mainCamera;

            // To make the area visible
            // var bg = go.AddComponent<Image>();
            // bg.raycastTarget = false;
            // bg.color = new Color(1f, 1f, 1f, 0.01f);

            var overlay = go.AddComponent<KeybindingsOverlay>();

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

    public void Append(string value)
    {
        if (text.text == "")
            Set(value);
        else
            Set(text.text + " " + value);
    }

    public void Set(string value)
    {
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
