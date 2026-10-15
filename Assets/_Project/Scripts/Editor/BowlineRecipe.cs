using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CaveGame.EditorTools
{
    /// <summary>
    /// Геометрия и тексты булиня (беседочного узла). Контрольные точки записаны «стоя»: коренная часть
    /// приходит сверху, как на большинстве схем. В игре узел повёрнут на 90°, коренная приходит слева,
    /// как у восьмёрки: так он лучше ложится в кадр 16:9.
    /// </summary>
    public static class BowlineRecipe
    {
        public const string AssetPath = "Assets/_Project/Data/Knots/Bowline.asset";

        private const float Spacing = 0.05f;
        private const float Tolerance = 0.35f;

        // Коренная часть сверху вниз до малой петельки.
        private static readonly Vector2[] StandingControls =
        {
            new Vector2(0f, 8f), new Vector2(0f, 6.8f), new Vector2(0f, 5.2f)
        };

        // Контрольные точки шагов (без начальной точки — она равна концу предыдущего участка).
        private static readonly Vector2[][] StepControls =
        {
            // 1. Малая петелька: вниз, вправо по кругу и обратно влево НАД коренной (перехлёст X ≈ (0, 4.67)).
            new[]
            {
                new Vector2(0f, 4.0f), new Vector2(0.3f, 3.1f), new Vector2(1.0f, 2.75f), new Vector2(1.6f, 3.2f),
                new Vector2(1.6f, 4.1f), new Vector2(1.0f, 4.65f), new Vector2(-0.8f, 4.45f), new Vector2(-1.3f, 3.8f)
            },
            // 2. Большая петля вниз и вокруг, затем снизу в петельку: ПОД её нижним краем, наружу НАД верхним.
            new[]
            {
                new Vector2(-1.8f, 2.2f), new Vector2(-1.4f, 0.4f), new Vector2(0f, -0.4f), new Vector2(1.3f, 0.2f),
                new Vector2(1.5f, 1.6f), new Vector2(1.1f, 3.4f), new Vector2(0.85f, 4.9f), new Vector2(0.8f, 5.4f)
            },
            // 3. Вокруг коренной части сзади — ПОД ней.
            new[]
            {
                new Vector2(0.4f, 5.95f), new Vector2(-0.4f, 5.95f), new Vector2(-0.75f, 5.4f)
            },
            // 4. Обратно в петельку сверху вниз: НАД выходящей веткой и НАД краем петельки,
            //    затем вниз из петельки ПОД её нижним краем, рядом с веткой шага 2.
            new[]
            {
                new Vector2(-0.6f, 4.35f), new Vector2(0.1f, 3.7f), new Vector2(0.6f, 3.1f), new Vector2(0.6f, 2.3f)
            },
            // 5. Затянуть: хвост остаётся внутри большой петли.
            new[]
            {
                new Vector2(0.55f, 1.2f)
            }
        };

        // Слои пересечений каждого шага по порядку вдоль шага: true — конец НАД.
        // У булиня строгого чередования нет: на шаге 4 конец проходит над двумя участками подряд.
        private static readonly bool[][] StepLayers =
        {
            new[] { true },               // над коренной
            new[] { false, true },        // под краем петельки, наружу над краем
            new[] { false },              // под коренной
            new[] { true, true, false },  // над веткой, над краем петельки, под нижним краем
            new bool[0]
        };

        private static readonly string[] Hints =
        {
            "Сделай малую петельку: ходовой конец ложится НАД коренной частью.",
            "Сделай большую петлю и проведи конец в петельку снизу: сначала ПОД краем, потом наружу.",
            "Обведи конец вокруг коренной части сзади — ПОД ней.",
            "Верни конец в петельку сверху вниз, рядом со своей же веткой.",
            "Затяни узел: конец остаётся внутри большой петли."
        };

        public static KnotDefinition CreateDefinition()
        {
            var built = KnotBuilder.Build(
                StandingControls.Select(Lay).ToArray(),
                StepControls.Select(step => (IReadOnlyList<Vector2>)step.Select(Lay).ToArray()).ToArray(),
                Spacing,
                StepLayers);

            var knot = ScriptableObject.CreateInstance<KnotDefinition>();
            knot.name = "Bowline";
            knot.displayName = "Булинь";
            knot.description = "Беседочный узел — петля на конце верёвки, которая не затягивается под нагрузкой " +
                               "и легко развязывается потом.";
            knot.standingPart = built.StandingPart;
            knot.crossings = built.Crossings;
            knot.steps = new List<KnotStep>();
            for (int i = 0; i < built.Steps.Count; i++)
            {
                knot.steps.Add(new KnotStep { hint = Hints[i], points = built.Steps[i], tolerance = Tolerance });
            }
            return knot;
        }

        [MenuItem("Tools/Cave Game/Create Bowline")]
        public static KnotDefinition CreateOrUpdateAsset()
        {
            return KnotAssetUtility.SaveOrUpdate(CreateDefinition(), AssetPath);
        }

        /// <summary>Кладёт «стоячую» схему на бок: коренная часть приходит слева.</summary>
        private static Vector2 Lay(Vector2 upright)
        {
            return new Vector2(-upright.y, upright.x);
        }
    }
}
