using System;
using UnityEngine;
using UnityEngine.UI;
using static CaveGame.UiFactory;

namespace CaveGame
{
    /// <summary>
    /// Интерфейс сцены вязки, собирается кодом: название узла, подсказка шага, «Шаг N из M»,
    /// кнопка «Меню» и экран «Узел завязан!» с кнопками «Заново» и «Следующий узел» / «В меню».
    /// Раскладка — в координатах 1920×1080.
    /// </summary>
    public sealed class KnotHud
    {
        private const float Margin = 60f;

        private readonly Text _title;
        private readonly Text _stepCounter;
        private readonly Text _hint;
        private readonly Text _deviation;
        private readonly GameObject _completion;

        public event Action RestartRequested;
        public event Action NextRequested;
        public event Action MenuRequested;

        /// <param name="hasNextKnot">Есть ли в каталоге следующий узел: иначе на экране завершения кнопка «В меню».</param>
        public KnotHud(Transform parent, KnotDefinition knot, KnotVisualSettings settings, bool hasNextKnot)
        {
            var root = CreateCanvas(parent, "HUD");

            // Левый верхний угол: название, счётчик шагов, подсказка.
            _title = CreateText(root, "Title", knot.displayName, 60, FontStyle.Bold, TextColor, TextAnchor.UpperLeft);
            Place(_title.rectTransform, new Vector2(0, 1), new Vector2(Margin, -40), new Vector2(1200, 80));

            _stepCounter = CreateText(root, "StepCounter", "", 32, FontStyle.Bold, TextColor, TextAnchor.UpperLeft);
            Place(_stepCounter.rectTransform, new Vector2(0, 1), new Vector2(Margin, -120), new Vector2(1200, 50));

            _hint = CreateText(root, "Hint", "", 36, FontStyle.Normal, TextColor, TextAnchor.UpperLeft);
            Place(_hint.rectTransform, new Vector2(0, 1), new Vector2(Margin, -175), new Vector2(1250, 120));

            var menu = CreateButton(root, "MenuButton", "Меню", 34, () => MenuRequested?.Invoke());
            Place((RectTransform)menu.transform, new Vector2(1, 1), new Vector2(-Margin - 260, -40),
                new Vector2(260, MinButtonHeight));

            _deviation = CreateText(root, "Deviation", "Верни конец к пунктиру", 36, FontStyle.Bold,
                settings.errorColor, TextAnchor.LowerCenter);
            Place(_deviation.rectTransform, new Vector2(0.5f, 0), new Vector2(-600, 60 + Margin), new Vector2(1200, 60));
            _deviation.enabled = false;

            _completion = CreateCompletionScreen(root, knot, settings, hasNextKnot);
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

        private GameObject CreateCompletionScreen(RectTransform root, KnotDefinition knot, KnotVisualSettings settings,
            bool hasNextKnot)
        {
            // Панель сверху, в зоне интерфейса: завязанный узел под ней остаётся виден целиком.
            // Ширина 1200 оставляет справа место под кнопку «Меню».
            var panel = CreateImage(root, "CompletionScreen", PanelColor);
            Place(panel.rectTransform, new Vector2(0.5f, 1), new Vector2(-600, -24), new Vector2(1200, 310));

            var title = CreateText(panel.rectTransform, "Title", "Узел завязан!", 72, FontStyle.Bold,
                settings.ropeColor, TextAnchor.UpperLeft);
            Place(title.rectTransform, new Vector2(0, 1), new Vector2(50, -30), new Vector2(740, 100));

            var description = CreateText(panel.rectTransform, "Description", knot.description, 30, FontStyle.Normal,
                TextColor, TextAnchor.UpperLeft);
            Place(description.rectTransform, new Vector2(0, 1), new Vector2(50, -135), new Vector2(740, 120));

            // Справа столбиком: «Заново», под ней «Следующий узел» или «В меню».
            var again = CreateButton(panel.rectTransform, "AgainButton", "Заново", 34, () => RestartRequested?.Invoke());
            Place((RectTransform)again.transform, new Vector2(1, 1), new Vector2(-40 - 340, -30),
                new Vector2(340, MinButtonHeight));

            var forward = hasNextKnot
                ? CreateButton(panel.rectTransform, "NextButton", "Следующий узел", 34, AccentButton(settings),
                    () => NextRequested?.Invoke())
                : CreateButton(panel.rectTransform, "MenuButton", "В меню", 34, AccentButton(settings),
                    () => MenuRequested?.Invoke());
            Place((RectTransform)forward.transform, new Vector2(1, 1), new Vector2(-40 - 340, -30 - MinButtonHeight - 15),
                new Vector2(340, MinButtonHeight));

            return panel.gameObject;
        }

        private static Color AccentButton(KnotVisualSettings settings)
        {
            // Основное действие — чуть ярче остальных кнопок, в тон акцентного цвета.
            return Color.Lerp(ButtonColor, settings.ropeColor, 0.35f);
        }
    }
}
