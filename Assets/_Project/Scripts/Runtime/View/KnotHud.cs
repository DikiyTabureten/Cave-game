using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace CaveGame
{
    /// <summary>
    /// Интерфейс прототипа, собирается кодом: название узла, подсказка шага, «Шаг N из M»,
    /// кнопка «Заново» и экран «Узел завязан!».
    /// </summary>
    public sealed class KnotHud
    {
        private static readonly Color TextColor = new Color(0.93f, 0.95f, 0.97f);
        private static readonly Color ButtonColor = new Color(0.2f, 0.24f, 0.3f);
        private static readonly Color PanelColor = new Color(0.12f, 0.14f, 0.18f);

        private readonly Font _font;
        private readonly Text _title;
        private readonly Text _stepCounter;
        private readonly Text _hint;
        private readonly Text _deviation;
        private readonly GameObject _completion;

        public event Action RestartRequested;

        public KnotHud(Transform parent, KnotDefinition knot, KnotVisualSettings settings)
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            EnsureEventSystem();

            var canvasGo = new GameObject("HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(parent, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            var root = (RectTransform)canvasGo.transform;

            // Левый верхний угол: название, счётчик шагов, подсказка.
            _title = CreateText(root, "Title", knot.displayName, 60, FontStyle.Bold, TextColor, TextAnchor.UpperLeft);
            Place(_title.rectTransform, new Vector2(0, 1), new Vector2(60, -40), new Vector2(1200, 80));

            _stepCounter = CreateText(root, "StepCounter", "", 32, FontStyle.Bold, TextColor, TextAnchor.UpperLeft);
            Place(_stepCounter.rectTransform, new Vector2(0, 1), new Vector2(60, -120), new Vector2(1200, 50));

            _hint = CreateText(root, "Hint", "", 36, FontStyle.Normal, TextColor, TextAnchor.UpperLeft);
            Place(_hint.rectTransform, new Vector2(0, 1), new Vector2(60, -175), new Vector2(1250, 120));

            var restart = CreateButton(root, "RestartButton", "Заново", 34, OnRestartClicked);
            Place((RectTransform)restart.transform, new Vector2(1, 1), new Vector2(-60 - 260, -40), new Vector2(260, 84));

            _deviation = CreateText(root, "Deviation", "Верни конец к пунктиру", 36, FontStyle.Bold,
                settings.errorColor, TextAnchor.LowerCenter);
            Place(_deviation.rectTransform, new Vector2(0.5f, 0), new Vector2(-600, 60), new Vector2(1200, 60));
            _deviation.enabled = false;

            _completion = CreateCompletionScreen(root, knot, settings);
            _completion.SetActive(false);
        }

        /// <param name="stepColor">Цвет шага на верёвке — счётчик окрашивается так же.</param>
        public void ShowStep(int index, int count, string hint, Color stepColor)
        {
            _stepCounter.text = $"Шаг {index + 1} из {count}";
            _stepCounter.color = stepColor;
            _hint.text = hint;
        }

        public void ShowDeviation(bool visible)
        {
            _deviation.enabled = visible;
        }

        public void ShowCompletion(bool visible)
        {
            _completion.SetActive(visible);
            _title.enabled = _stepCounter.enabled = _hint.enabled = !visible;
            if (visible) _deviation.enabled = false;
        }

        private GameObject CreateCompletionScreen(RectTransform root, KnotDefinition knot, KnotVisualSettings settings)
        {
            // Панель сверху, в зоне интерфейса: завязанный узел под ней остаётся виден целиком.
            var panel = CreateImage(root, "CompletionScreen", PanelColor);
            Place(panel.rectTransform, new Vector2(0.5f, 1), new Vector2(-600, -24), new Vector2(1200, 270));

            var title = CreateText(panel.rectTransform, "Title", "Узел завязан!", 72, FontStyle.Bold,
                settings.ropeColor, TextAnchor.UpperLeft);
            Place(title.rectTransform, new Vector2(0, 1), new Vector2(50, -30), new Vector2(760, 100));

            var description = CreateText(panel.rectTransform, "Description", knot.description, 30, FontStyle.Normal,
                TextColor, TextAnchor.UpperLeft);
            Place(description.rectTransform, new Vector2(0, 1), new Vector2(50, -135), new Vector2(760, 120));

            var again = CreateButton(panel.rectTransform, "AgainButton", "Ещё раз", 38, OnRestartClicked);
            Place((RectTransform)again.transform, new Vector2(1, 0.5f), new Vector2(-50 - 300, 48), new Vector2(300, 96));

            return panel.gameObject;
        }

        private void OnRestartClicked()
        {
            RestartRequested?.Invoke();
        }

        private Text CreateText(Transform parent, string name, string value, int size, FontStyle style, Color color, TextAnchor anchor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<Text>();
            text.font = _font;
            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = anchor;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        private static Image CreateImage(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private Button CreateButton(Transform parent, string name, string label, int fontSize, Action onClick)
        {
            // Картинка белая, цвет задаёт сама кнопка — так работают подсветка и нажатие.
            var image = CreateImage(parent, name, Color.white);
            var button = image.gameObject.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = ButtonColor;
            colors.highlightedColor = Color.Lerp(ButtonColor, Color.white, 0.2f);
            colors.selectedColor = ButtonColor;
            colors.pressedColor = Color.Lerp(ButtonColor, Color.black, 0.3f);
            button.colors = colors;
            button.onClick.AddListener(() => onClick());

            var text = CreateText(image.transform, "Label", label, fontSize, FontStyle.Bold, TextColor, TextAnchor.MiddleCenter);
            var rect = text.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return button;
        }

        /// <summary>
        /// Ставит элемент относительно точки привязки <paramref name="anchor"/>:
        /// <paramref name="topLeft"/> — смещение левого верхнего угла, <paramref name="size"/> — размер.
        /// </summary>
        private static void Place(RectTransform rect, Vector2 anchor, Vector2 topLeft, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = topLeft;
            rect.sizeDelta = size;
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null || UnityEngine.Object.FindAnyObjectByType<EventSystem>() != null) return;

            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            UnityEngine.Object.DontDestroyOnLoad(go);
        }
    }
}
