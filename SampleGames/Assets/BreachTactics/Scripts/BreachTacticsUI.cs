using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace BreachTactics
{
    public sealed class BreachTacticsUI : MonoBehaviour
    {
        private Text statusText;
        private Text turnText;
        private Text objectiveText;
        private Font font;

        public event Action EndTurnRequested;
        public event Action RestartRequested;

        public void Build()
        {
            EnsureEventSystem();
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            GameObject canvasObject = new("Runtime_BreachCanvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 30;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();

            statusText = CreatePanelText(canvasObject.transform, "Status", new Vector2(24f, 24f),
                new Vector2(650f, 210f), TextAnchor.UpperLeft, 25);

            turnText = CreatePanelText(canvasObject.transform, "Turn", new Vector2(-24f, -24f),
                new Vector2(430f, 150f), TextAnchor.MiddleCenter, 32, true);

            objectiveText = CreateTopText(canvasObject.transform);

            CreateButton(canvasObject.transform, "EndTurn", "TURN END",
                new Vector2(-24f, -195f), new Color(0.82f, 0.18f, 0.12f), () => EndTurnRequested?.Invoke());

            CreateButton(canvasObject.transform, "Restart", "RESTART",
                new Vector2(-24f, -310f), new Color(0.18f, 0.36f, 0.58f), () => RestartRequested?.Invoke());
        }

        public void SetStatus(string value)
        {
            if (statusText != null)
            {
                statusText.text = value;
            }
        }

        public void SetTurn(string value)
        {
            if (turnText != null)
            {
                turnText.text = value;
            }
        }

        public void SetObjective(string value)
        {
            if (objectiveText != null)
            {
                objectiveText.text = value;
            }
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
            {
                return;
            }

            GameObject eventSystemObject = new("Runtime_EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<InputSystemUIInputModule>();
        }

        private Text CreatePanelText(Transform parent, string objectName, Vector2 position,
            Vector2 size, TextAnchor alignment, int fontSize, bool topRight = false)
        {
            GameObject panelObject = new(objectName + "Panel");
            panelObject.transform.SetParent(parent, false);
            RectTransform panelRect = panelObject.AddComponent<RectTransform>();
            panelRect.anchorMin = topRight ? Vector2.one : Vector2.zero;
            panelRect.anchorMax = topRight ? Vector2.one : Vector2.zero;
            panelRect.pivot = topRight ? Vector2.one : Vector2.zero;
            panelRect.anchoredPosition = position;
            panelRect.sizeDelta = size;

            Image image = panelObject.AddComponent<Image>();
            image.color = new Color(0.015f, 0.035f, 0.07f, 0.92f);

            GameObject textObject = new(objectName + "Text");
            textObject.transform.SetParent(panelObject.transform, false);
            RectTransform rect = textObject.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(18f, 12f);
            rect.offsetMax = new Vector2(-18f, -12f);

            Text text = textObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.color = Color.white;
            text.alignment = alignment;
            text.raycastTarget = false;
            return text;
        }

        private Text CreateTopText(Transform parent)
        {
            GameObject objectValue = new("ObjectiveText");
            objectValue.transform.SetParent(parent, false);
            RectTransform rect = objectValue.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -20f);
            rect.sizeDelta = new Vector2(1000f, 72f);

            Text text = objectValue.AddComponent<Text>();
            text.font = font;
            text.fontSize = 30;
            text.fontStyle = FontStyle.Bold;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            return text;
        }

        private void CreateButton(Transform parent, string objectName, string label,
            Vector2 position, Color color, Action action)
        {
            GameObject buttonObject = new(objectName + "Button");
            buttonObject.transform.SetParent(parent, false);
            RectTransform rect = buttonObject.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.one;
            rect.anchorMax = Vector2.one;
            rect.pivot = Vector2.one;
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(430f, 92f);

            Image image = buttonObject.AddComponent<Image>();
            image.color = color;

            Button button = buttonObject.AddComponent<Button>();
            button.onClick.AddListener(() => action());

            GameObject textObject = new("Text");
            textObject.transform.SetParent(buttonObject.transform, false);
            RectTransform textRect = textObject.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text text = textObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = 30;
            text.fontStyle = FontStyle.Bold;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            text.text = label;
        }
    }
}
