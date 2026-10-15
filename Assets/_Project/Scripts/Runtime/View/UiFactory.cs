using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace CaveGame
{
    /// <summary>
    /// Сборка uGUI кодом: холст под 1920×1080, тексты, кнопки, раскладка. Общая для меню и сцены вязки.
    /// </summary>
    public static class UiFactory
    {
        public static readonly Vector2 ReferenceResolution = new Vector2(1920, 1080);
        public const float MatchWidthOrHeight = 0.5f;

        public static readonly Color TextColor = new Color(0.93f, 0.95f, 0.97f);
        public static readonly Color MutedTextColor = new Color(0.6f, 0.68f, 0.78f);
        public static readonly Color ButtonColor = new Color(0.2f, 0.24f, 0.3f);
        public static readonly Color PanelColor = new Color(0.12f, 0.14f, 0.18f);

        private static Font _font;

        public static Font Font => _font != null ? _font : _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        /// <summary>Минимальная высота кнопки в координатах 1920×1080 — под палец.</summary>
        public const float MinButtonHeight = 120f;

        /// <summary>
        /// Холст Screen Space Overlay: Scale With Screen Size, 1920×1080, Match = 0.5.
        /// Возвращает контейнер внутри холста, вписанный в Screen.safeArea, — весь интерфейс кладётся в него.
        /// </summary>
        public static RectTransform CreateCanvas(Transform parent, string name)
        {
            EnsureEventSystem();

            var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(parent, false);
            go.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = MatchWidthOrHeight;

            // Вырезы камеры и закругления экрана: интерфейс живёт только в безопасной области.
            var safeArea = new GameObject("SafeArea", typeof(RectTransform), typeof(SafeArea));
            safeArea.transform.SetParent(go.transform, false);
            return (RectTransform)safeArea.transform;
        }

        public static Text CreateText(Transform parent, string name, string value, int size, FontStyle style,
            Color color, TextAnchor anchor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<Text>();
            text.font = Font;
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

        public static Image CreateImage(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = color;
            return image;
        }

        /// <summary>Кнопка с подписью по центру. Подпись — первый дочерний Text.</summary>
        public static Button CreateButton(Transform parent, string name, string label, int fontSize, Action onClick)
        {
            return CreateButton(parent, name, label, fontSize, ButtonColor, onClick);
        }

        public static Button CreateButton(Transform parent, string name, string label, int fontSize, Color color, Action onClick)
        {
            var button = CreatePlainButton(parent, name, color, onClick);
            var text = CreateText(button.transform, "Label", label, fontSize, FontStyle.Bold, TextColor, TextAnchor.MiddleCenter);
            Stretch(text.rectTransform);
            return button;
        }

        /// <summary>Кнопка без подписи — содержимое добавляет вызывающий код.</summary>
        public static Button CreatePlainButton(Transform parent, string name, Color color, Action onClick)
        {
            // Картинка белая, цвет задаёт сама кнопка — так работают подсветка и нажатие.
            var image = CreateImage(parent, name, Color.white);
            var button = image.gameObject.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = color;
            colors.highlightedColor = Color.Lerp(color, Color.white, 0.2f);
            colors.selectedColor = color;
            colors.pressedColor = Color.Lerp(color, Color.black, 0.3f);
            button.colors = colors;
            // Сразу в обычный цвет: иначе кнопка первые кадры плавно темнеет из белого.
            image.CrossFadeColor(color, 0f, true, true);
            button.onClick.AddListener(() => onClick());
            return button;
        }

        /// <summary>
        /// Ставит элемент относительно точки привязки <paramref name="anchor"/>:
        /// <paramref name="topLeft"/> — смещение левого верхнего угла от неё, <paramref name="size"/> — размер.
        /// </summary>
        public static void Place(RectTransform rect, Vector2 anchor, Vector2 topLeft, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = topLeft;
            rect.sizeDelta = size;
        }

        public static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        public static void EnsureEventSystem()
        {
            if (EventSystem.current != null || UnityEngine.Object.FindAnyObjectByType<EventSystem>() != null) return;

            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            UnityEngine.Object.DontDestroyOnLoad(go);
        }
    }
}
