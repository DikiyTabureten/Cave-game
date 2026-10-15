using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CaveGame.EditorTools
{
    /// <summary>
    /// Геометрия и тексты простой стопорной восьмёрки. Вид сверху, коренная часть приходит слева.
    /// Путь — сплайн Catmull-Rom по контрольным точкам ниже; перехлёсты находятся автоматически.
    /// </summary>
    public static class FigureEightRecipe
    {
        public const string AssetPath = "Assets/_Project/Data/Knots/FigureEight.asset";

        private const float Spacing = 0.05f;
        private const float Tolerance = 0.35f;

        private static readonly Vector2[] StandingControls =
        {
            new Vector2(-6f, 0f), new Vector2(-4f, 0f), new Vector2(-2f, 0f)
        };

        // Контрольные точки шагов (без начальной точки — она равна концу предыдущего участка).
        private static readonly Vector2[][] StepControls =
        {
            // 1. Вдоль коренной части вправо, вверх и назад: петля, конец падает вниз НАД верёвкой.
            new[]
            {
                new Vector2(0.4f, 0f), new Vector2(1.8f, 0.2f), new Vector2(2.8f, 1.4f), new Vector2(2.1f, 2.6f),
                new Vector2(0.4f, 2.7f), new Vector2(-0.7f, 1.7f), new Vector2(-1.1f, 0f), new Vector2(-1.4f, -1.4f)
            },
            // 2. Влево и вверх вокруг коренной части, ПОД ней — вторая половина «восьмёрки».
            new[]
            {
                new Vector2(-2.4f, -2.0f), new Vector2(-3.5f, -1.1f), new Vector2(-3.4f, 0.7f),
                new Vector2(-2.8f, 1.8f), new Vector2(-1.8f, 1.9f)
            },
            // 3. В первую петлю спереди: НАД её левым краем, затем вниз ПОД нижним.
            new[]
            {
                new Vector2(-0.4f, 1.3f), new Vector2(0.9f, 0.9f), new Vector2(1.1f, -0.2f), new Vector2(1.4f, -1.0f)
            },
            // 4. Затягиваем: конец уходит вправо, прочь от коренной части.
            new[]
            {
                new Vector2(2.7f, -1.25f), new Vector2(4.2f, -1.0f), new Vector2(5.6f, -0.9f)
            }
        };

        private static readonly string[] Hints =
        {
            "Веди ходовой конец вдоль коренной части и заверни петлю. Конец ложится НАД коренной частью.",
            "Обведи конец вокруг коренной части сзади: он проходит ПОД ней. Получается «восьмёрка».",
            "Продень конец в первую петлю спереди: сначала НАД верёвкой, потом ПОД.",
            "Затяни узел: выведи конец в сторону, противоположную коренной части."
        };

        public static KnotDefinition CreateDefinition()
        {
            var built = KnotBuilder.Build(StandingControls, StepControls, Spacing);

            var knot = ScriptableObject.CreateInstance<KnotDefinition>();
            knot.name = "FigureEight";
            knot.displayName = "Восьмёрка";
            knot.description = "Простой стопорный узел. Завязывается на конце верёвки, " +
                               "чтобы она не выскользнула из спускового устройства или карабина.";
            knot.standingPart = built.StandingPart;
            knot.crossings = built.Crossings;
            knot.steps = new List<KnotStep>();
            for (int i = 0; i < built.Steps.Count; i++)
            {
                knot.steps.Add(new KnotStep { hint = Hints[i], points = built.Steps[i], tolerance = Tolerance });
            }
            return knot;
        }

        [MenuItem("Tools/Cave Game/Create Figure Eight")]
        public static KnotDefinition CreateOrUpdateAsset()
        {
            return KnotAssetUtility.SaveOrUpdate(CreateDefinition(), AssetPath);
        }
    }
}
