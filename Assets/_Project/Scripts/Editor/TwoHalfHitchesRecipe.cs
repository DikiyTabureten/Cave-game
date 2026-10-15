using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CaveGame.EditorTools
{
    /// <summary>
    /// Геометрия и тексты простого штыка: оборот вокруг перекладины, три полуштыка на коренной части
    /// и хвост вдоль коренной. Вид сверху: коренная часть приходит слева, перекладина стоит вертикально (x = 0).
    /// Длина хвоста — в ассете <see cref="TwoHalfHitchesSettings"/>.
    /// </summary>
    public static class TwoHalfHitchesRecipe
    {
        public const string AssetPath = "Assets/_Project/Data/Knots/TwoHalfHitches.asset";

        /// <summary>Длина прямого участка хвоста по умолчанию, в юнитах.</summary>
        public const float DefaultTailLength = 1.8f;

        private const float Spacing = 0.05f;
        private const float Tolerance = 0.35f;
        private const int HalfHitchCount = 3;

        // Левое пересечение первого полуштыка с коренной и шаг между соседними полуштыками.
        private const float FirstHitchX = -2.1f;
        private const float HitchStep = 1.4f;

        // Коренная часть слева до перекладины: тянется дальше хвоста, чтобы хвост лежал вдоль неё.
        // Кончается левее перекладины: оборот — уже шаг игрока.
        private static readonly Vector2[] StandingControls =
        {
            new Vector2(-8.5f, 0f), new Vector2(-5f, 0f), new Vector2(-1.2f, 0f)
        };

        // Оборот: вправо ПОД перекладиной, разворот за ней и обратно НАД ней.
        private static readonly Vector2[] TurnControls =
        {
            new Vector2(-0.4f, 0f), new Vector2(0.6f, 0.05f), new Vector2(1.1f, -0.4f),
            new Vector2(0.6f, -0.8f), new Vector2(-1.0f, -0.8f)
        };

        // Полуштык относительно x его левого пересечения с коренной: заход снизу справа, вверх НАД коренной,
        // вокруг и вниз ПОД ней, НАД своей веткой — в петлю, выход вниз. У всех полуштыков форма одна.
        private static readonly Vector2[] HalfHitchTemplate =
        {
            new Vector2(0.3f, -0.75f), new Vector2(0f, 0f), new Vector2(0.2f, 0.75f), new Vector2(0.7f, 0.6f),
            new Vector2(0.75f, -0.2f), new Vector2(0.55f, -0.8f), new Vector2(0.2f, -1.25f)
        };

        // Изгиб от выхода последнего полуштыка к хвосту, затем хвост — прямо вдоль коренной на высоте TailY.
        private const float TailBendX = -0.6f;
        private const float TailBendY = -0.95f;
        private const float TailY = -0.9f;

        // Слои: true — конец НАД (верёвкой или опорой).
        private static readonly bool[] TurnLayers = { false, true };            // под перекладиной, обратно над
        private static readonly bool[] HalfHitchLayers = { true, false, true }; // над коренной, под, над своей веткой

        private const string TurnHint = "Обнеси конец вокруг перекладины: сначала ПОД ней, потом обратно НАД.";
        private const string TailHint = "Затяни: сдвинь полуштыки к перекладине, хвост оставь вдоль коренной.";

        private static readonly string[] HalfHitchHints =
        {
            "Первый полуштык: обведи конец вокруг коренной части — НАД, потом ПОД — и продень в петлю.",
            "Второй полуштык: сделай то же самое ещё раз, в ту же сторону.",
            "Третий полуштык: ещё раз в ту же сторону, следом за вторым."
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

        /// <summary>Узел с длиной хвоста из ассета настроек (или по умолчанию, если ассета нет).</summary>
        public static KnotDefinition CreateDefinition()
        {
            return CreateDefinition(TwoHalfHitchesSettings.LoadOrDefault().tailLength);
        }

        public static KnotDefinition CreateDefinition(float tailLength)
        {
            var stepControls = new List<IReadOnlyList<Vector2>> { TurnControls };
            var layers = new List<IReadOnlyList<bool>> { TurnLayers };
            var hints = new List<string> { TurnHint };

            Vector2 exit = Vector2.zero;
            for (int i = 0; i < HalfHitchCount; i++)
            {
                float x = FirstHitchX - HitchStep * i;
                var hitch = new Vector2[HalfHitchTemplate.Length];
                for (int k = 0; k < hitch.Length; k++)
                {
                    hitch[k] = HalfHitchTemplate[k] + new Vector2(x, 0f);
                }

                stepControls.Add(hitch);
                layers.Add(HalfHitchLayers);
                hints.Add(HalfHitchHints[i]);
                exit = hitch[hitch.Length - 1];
            }

            // Хвост: короткий изгиб от выхода третьего полуштыка, затем tailLength прямо вдоль коренной.
            var bend = new Vector2(exit.x + TailBendX, TailBendY);
            stepControls.Add(new[] { bend, new Vector2(bend.x - tailLength, TailY) });
            layers.Add(new bool[0]);
            hints.Add(TailHint);

            var support = CreateSupport();
            var built = KnotBuilder.Build(StandingControls, stepControls, Spacing, layers, support);

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
                knot.steps.Add(new KnotStep { hint = hints[i], points = built.Steps[i], tolerance = Tolerance });
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
