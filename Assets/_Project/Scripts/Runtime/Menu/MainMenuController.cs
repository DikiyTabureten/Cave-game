using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static CaveGame.UiFactory;

namespace CaveGame
{
    /// <summary>
    /// Главное меню: заголовок и карточки узлов из каталога. Интерфейс собирается кодом на старте.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        private const float CardWidth = 720f;
        private const float CardHeight = 130f; // не меньше UiFactory.MinButtonHeight — под палец
        private const float CardSpacing = 24f;
        private const float ListTop = -360f;

        [SerializeField] private KnotCatalog catalog;
        [SerializeField] private Camera targetCamera;
        [SerializeField] private KnotVisualSettings visuals = new KnotVisualSettings();

        private void Awake()
        {
            if (catalog == null)
            {
                Debug.LogError("MainMenuController: не назначен каталог узлов. " +
                               "Пересоберите сцены через Tools/Cave Game/Build All Scenes.", this);
                enabled = false;
                return;
            }

            if (targetCamera == null) targetCamera = Camera.main;
            if (targetCamera != null)
            {
                targetCamera.clearFlags = CameraClearFlags.SolidColor;
                targetCamera.backgroundColor = visuals.backgroundColor;
            }

            BuildUi(catalog.AvailableKnots);
        }

        private void Update()
        {
            // «Назад» в главном меню — выход из приложения (в редакторе Application.Quit ничего не делает).
            if (BackButton.WasPressed()) Application.Quit();
        }

        private void BuildUi(List<KnotDefinition> knots)
        {
            var root = CreateCanvas(transform, "MenuUI");

            var title = CreateText(root, "Title", "Cave Game", 120, FontStyle.Bold, visuals.ropeColor, TextAnchor.MiddleCenter);
            Place(title.rectTransform, new Vector2(0.5f, 1), new Vector2(-700, -110), new Vector2(1400, 150));

            var subtitle = CreateText(root, "Subtitle", "Выбери узел", 40, FontStyle.Normal, MutedTextColor,
                TextAnchor.MiddleCenter);
            Place(subtitle.rectTransform, new Vector2(0.5f, 1), new Vector2(-700, -260), new Vector2(1400, 60));

            for (int i = 0; i < knots.Count; i++)
            {
                CreateCard(root, knots, i);
            }
        }

        private void CreateCard(RectTransform root, List<KnotDefinition> knots, int index)
        {
            var knot = knots[index];
            var card = CreatePlainButton(root, $"Card_{knot.name}", PanelColor, () => OpenKnot(knots, index));
            float top = ListTop - index * (CardHeight + CardSpacing);
            Place((RectTransform)card.transform, new Vector2(0.5f, 1), new Vector2(-CardWidth / 2f, top),
                new Vector2(CardWidth, CardHeight));

            // Содержимое карточки: цветная полоска, номер, название и число шагов.
            var stripe = CreateImage(card.transform, "Stripe", visuals.SegmentColor(index));
            Place(stripe.rectTransform, new Vector2(0, 1), Vector2.zero, new Vector2(12, CardHeight));
            stripe.raycastTarget = false;

            var number = CreateText(card.transform, "Number", (index + 1).ToString(), 56, FontStyle.Bold,
                visuals.SegmentColor(index), TextAnchor.MiddleCenter);
            Place(number.rectTransform, new Vector2(0, 1), new Vector2(30, 0), new Vector2(90, CardHeight));

            var title = CreateText(card.transform, "Name", knot.displayName, 48, FontStyle.Bold, TextColor,
                TextAnchor.LowerLeft);
            Place(title.rectTransform, new Vector2(0, 1), new Vector2(140, -12), new Vector2(CardWidth - 170, 62));

            var steps = CreateText(card.transform, "Steps", RussianText.Steps(knot.steps.Count), 28, FontStyle.Normal,
                MutedTextColor, TextAnchor.UpperLeft);
            Place(steps.rectTransform, new Vector2(0, 1), new Vector2(140, -78), new Vector2(CardWidth - 170, 40));
        }

        private static void OpenKnot(List<KnotDefinition> knots, int index)
        {
            KnotSelection.Select(knots, index);
            SceneManager.LoadScene(SceneNames.KnotPrototype);
        }
    }
}
