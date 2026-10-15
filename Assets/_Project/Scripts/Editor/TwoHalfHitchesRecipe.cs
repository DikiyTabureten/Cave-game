using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CaveGame.EditorTools
{
    /// <summary>
    /// Геометрия и тексты простого штыка: оборот вокруг перекладины и два полуштыка на коренной части.
    /// Вид сверху: коренная часть приходит слева, перекладина стоит вертикально справа (x = 0).
    /// </summary>
    public static class TwoHalfHitchesRecipe
    {
        public const string AssetPath = "Assets/_Project/Data/Knots/TwoHalfHitches.asset";

        private const float Spacing = 0.05f;
        private const float Tolerance = 0.35f;

        // Коренная часть слева до перекладины. Кончается левее перекладины: оборот — уже шаг игрока.
        private static readonly Vector2[] StandingControls =
        {
            new Vector2(-5.5f, 0f), new Vector2(-3.5f, 0f), new Vector2(-1.2f, 0f)
        };

        // Контрольные точки шагов (без начальной точки — она равна концу предыдущего участка).
        private static readonly Vector2[][] StepControls =
        {
            // 1. Оборот: вправо ПОД перекладиной, разворот за ней и обратно НАД ней.
            new[]
            {
                new Vector2(-0.4f, 0f), new Vector2(0.6f, 0.05f), new Vector2(1.1f, -0.4f),
                new Vector2(0.6f, -0.8f), new Vector2(-1.0f, -0.8f)
            },
            // 2. Первый полуштык: вверх НАД коренной, вокруг и вниз ПОД ней, затем НАД своей веткой — в петлю.
            new[]
            {
                new Vector2(-1.8f, -0.75f), new Vector2(-2.1f, 0f), new Vector2(-1.9f, 0.75f), new Vector2(-1.4f, 0.6f),
                new Vector2(-1.35f, -0.2f), new Vector2(-1.55f, -0.8f), new Vector2(-1.9f, -1.25f)
            },
            // 3. Второй полуштык — то же, левее, в ту же сторону.
            new[]
            {
                new Vector2(-3.25f, -1.35f), new Vector2(-3.75f, -0.4f), new Vector2(-3.55f, 0.75f), new Vector2(-3.05f, 0.6f),
                new Vector2(-2.95f, -0.3f), new Vector2(-3.1f, -1.2f), new Vector2(-3.45f, -1.7f)
            },
            // 4. Затянуть: конец уходит вниз-влево, полуштыки прижимаются к перекладине.
            new[]
            {
                new Vector2(-3.95f, -2.3f)
            }
        };

        // Слои пересечений каждого шага по порядку вдоль шага: true — конец НАД (верёвкой или опорой).
        // Полуштык — простой узел вокруг коренной: над, под, над — чередование его и запирает.
        private static readonly bool[][] StepLayers =
        {
            new[] { false, true },        // под перекладиной, обратно над ней
            new[] { true, false, true },  // над коренной, под коренной, над своей веткой
            new[] { true, false, true },  // то же для второго полуштыка
            new bool[0]
        };

        private static readonly string[] Hints =
        {
            "Обнеси конец вокруг перекладины: сначала ПОД ней, потом обратно НАД.",
            "Первый полуштык: обведи конец вокруг коренной части — НАД, потом ПОД — и продень в петлю.",
            "Второй полуштык: сделай то же самое ещё раз, в ту же сторону.",
            "Затяни: сдвинь оба полуштыка к перекладине."
        };

        public static KnotSupport CreateSupport()
        {
            return new KnotSupport
            {
                shape = KnotSupportShape.Bar,
                position = new Vector2(0f, -0.4f),
                length = 3.6f,
                thickness = 0.4f,
                angle = 90f
            };
        }

        public static KnotDefinition CreateDefinition()
        {
            var support = CreateSupport();
            var built = KnotBuilder.Build(StandingControls, StepControls, Spacing, StepLayers, support);

            var knot = ScriptableObject.CreateInstance<KnotDefinition>();
            knot.name = "TwoHalfHitches";
            knot.displayName = "Штык";
            knot.description = "Простой штык надёжно привязывает верёвку к опоре: перекладине, дереву или кольцу.";
            knot.standingPart = built.StandingPart;
            knot.crossings = built.Crossings;
            knot.support = support;
            knot.steps = new List<KnotStep>();
            for (int i = 0; i < built.Steps.Count; i++)
            {
                knot.steps.Add(new KnotStep { hint = Hints[i], points = built.Steps[i], tolerance = Tolerance });
            }
            return knot;
        }

        [MenuItem("Tools/Cave Game/Create Two Half Hitches")]
        public static KnotDefinition CreateOrUpdateAsset()
        {
            return KnotAssetUtility.SaveOrUpdate(CreateDefinition(), AssetPath);
        }
    }
}
