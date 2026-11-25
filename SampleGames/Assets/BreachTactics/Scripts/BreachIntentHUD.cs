using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BreachTactics
{
    /// <summary>월드 좌표의 공격 관계를 화면 위 2D 전술 HUD로 표시합니다.</summary>
    public sealed class BreachIntentHUD : MonoBehaviour
    {
        private sealed class Entry
        {
            public BreachUnitState Enemy;
            public BreachUnitState Target;
            public RectTransform Line;
            public RectTransform Arrow;
            public RectTransform Source;
            public RectTransform TargetMark;
            public Text Label;
        }

        private readonly List<Entry> entries = new();
        private RectTransform root;
        private Font font;

        public void Build()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            GameObject canvasObject = new("Runtime_IntentHUD");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            root = canvasObject.GetComponent<RectTransform>();
        }

        public void SetIntents(IReadOnlyList<EnemyIntent> intents,
            IReadOnlyList<BreachUnitState> units)
        {
            Clear();
            if (root == null || intents == null || units == null) return;

            Color[] colors =
            {
                new(1f, 0.22f, 0.08f, 0.95f), new(1f, 0.72f, 0.05f, 0.95f),
                new(1f, 0.12f, 0.65f, 0.95f), new(0.62f, 0.3f, 1f, 0.95f)
            };

            for (int i = 0; i < intents.Count; i++)
            {
                EnemyIntent intent = intents[i];
                BreachUnitState enemy = units.FirstOrDefault(unit => unit.Id == intent.EnemyId);
                BreachUnitState target = units.FirstOrDefault(unit => unit.Id == intent.TargetId);
                if (enemy == null || target == null || !enemy.Alive || !target.Alive) continue;

                Color color = colors[i % colors.Length];
                entries.Add(new Entry
                {
                    Enemy = enemy,
                    Target = target,
                    Line = CreateImage($"IntentLine_{enemy.Id}", color, new Vector2(100f, 8f)),
                    Arrow = CreateImage($"IntentArrow_{enemy.Id}", color, new Vector2(24f, 24f)),
                    Source = CreateImage($"IntentSource_{enemy.Id}", color, new Vector2(30f, 30f)),
                    TargetMark = CreateImage($"IntentTarget_{target.Id}", color, new Vector2(48f, 48f)),
                    Label = CreateLabel($"{enemy.Name}  →  {target.Name}", color)
                });
            }
        }

        private void LateUpdate()
        {
            Camera camera = Camera.main;
            if (camera == null || root == null) return;

            foreach (Entry entry in entries)
            {
                Vector3 enemyScreen = camera.WorldToScreenPoint(
                    GridToWorld(entry.Enemy.Position) + Vector3.up * 1.8f);
                Vector3 targetScreen = camera.WorldToScreenPoint(
                    GridToWorld(entry.Target.Position) + Vector3.up * 1.8f);
                bool visible = enemyScreen.z > 0f && targetScreen.z > 0f;
                entry.Line.gameObject.SetActive(visible);
                entry.Arrow.gameObject.SetActive(visible);
                entry.Source.gameObject.SetActive(visible);
                entry.TargetMark.gameObject.SetActive(visible);
                entry.Label.gameObject.SetActive(visible);
                if (!visible) continue;

                Vector2 start = ScreenToCanvas(enemyScreen);
                Vector2 end = ScreenToCanvas(targetScreen);
                Vector2 delta = end - start;
                float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;

                entry.Line.anchoredPosition = (start + end) * 0.5f;
                entry.Line.sizeDelta = new Vector2(delta.magnitude, 8f);
                entry.Line.localRotation = Quaternion.Euler(0f, 0f, angle);
                entry.Source.anchoredPosition = start;
                entry.Source.localRotation = Quaternion.Euler(0f, 0f, 45f);
                entry.TargetMark.anchoredPosition = end;
                entry.TargetMark.localRotation = Quaternion.Euler(0f, 0f, 45f);
                entry.Arrow.anchoredPosition = Vector2.Lerp(start, end, 0.78f);
                entry.Arrow.localRotation = Quaternion.Euler(0f, 0f, angle + 45f);
                entry.Label.rectTransform.anchoredPosition = Vector2.Lerp(start, end, 0.5f)
                    + Vector2.up * 22f;
            }
        }

        public void Clear()
        {
            foreach (Entry entry in entries)
            {
                if (entry.Line != null) Destroy(entry.Line.gameObject);
                if (entry.Arrow != null) Destroy(entry.Arrow.gameObject);
                if (entry.Source != null) Destroy(entry.Source.gameObject);
                if (entry.TargetMark != null) Destroy(entry.TargetMark.gameObject);
                if (entry.Label != null) Destroy(entry.Label.gameObject);
            }
            entries.Clear();
        }

        private RectTransform CreateImage(string name, Color color, Vector2 size)
        {
            GameObject value = new(name);
            value.transform.SetParent(root, false);
            RectTransform rect = value.AddComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            Image image = value.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return rect;
        }

        private Text CreateLabel(string value, Color color)
        {
            GameObject objectValue = new("IntentLabel");
            objectValue.transform.SetParent(root, false);
            RectTransform rect = objectValue.AddComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(260f, 34f);
            Text text = objectValue.AddComponent<Text>();
            text.font = font;
            text.fontSize = 20;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.text = value;
            text.raycastTarget = false;
            Outline outline = objectValue.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.9f);
            outline.effectDistance = new Vector2(2f, -2f);
            return text;
        }

        private Vector2 ScreenToCanvas(Vector3 point)
        {
            return new Vector2(
                point.x / Screen.width * root.rect.width - root.rect.width * 0.5f,
                point.y / Screen.height * root.rect.height - root.rect.height * 0.5f);
        }

        private static Vector3 GridToWorld(GridPos position)
        {
            return new Vector3(position.X - 3.5f, 0f, position.Y - 3.5f);
        }
    }
}
