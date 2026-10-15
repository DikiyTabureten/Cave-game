using System;
using System.Collections.Generic;
using UnityEngine;

namespace CaveGame
{
    /// <summary>
    /// Строит путь узла по контрольным точкам: один сплайн Catmull-Rom на всю верёвку,
    /// разрезанный на коренную часть и шаги, плюс перехлёсты. Слои над/под — либо строгое чередование
    /// вдоль верёвки, либо явно заданные для каждого шага (у некоторых узлов, например булиня,
    /// конец проходит над двумя участками подряд).
    /// </summary>
    public static class KnotBuilder
    {
        public sealed class Result
        {
            public List<Vector2> StandingPart;
            public List<List<Vector2>> Steps;
            public List<KnotCrossing> Crossings;
        }

        /// <param name="standingControls">Контрольные точки коренной части.</param>
        /// <param name="stepControls">Контрольные точки каждого шага без начальной (она равна концу предыдущего участка).</param>
        /// <param name="spacing">Максимальное расстояние между точками итогового пути.</param>
        /// <param name="stepLayers">
        /// Необязательно: для каждого шага — слои его пересечений по порядку вдоль шага
        /// (true — ведущийся конец НАД). Если не задано, слои чередуются автоматически.
        /// </param>
        /// <param name="support">
        /// Необязательно: опора узла. Её пересечения с верёвкой входят в слои шагов наравне с перехлёстами,
        /// поэтому с опорой слои нужно задать явно.
        /// </param>
        public static Result Build(
            IReadOnlyList<Vector2> standingControls,
            IReadOnlyList<IReadOnlyList<Vector2>> stepControls,
            float spacing,
            IReadOnlyList<IReadOnlyList<bool>> stepLayers = null,
            KnotSupport support = null)
        {
            var controls = new List<Vector2>(standingControls);
            var boundaries = new List<int> { standingControls.Count - 1 };
            foreach (var step in stepControls)
            {
                controls.AddRange(step);
                boundaries.Add(controls.Count - 1);
            }

            var samples = CatmullRom.Sample(controls, spacing, out var controlIndices);

            var result = new Result
            {
                StandingPart = samples.GetRange(0, controlIndices[boundaries[0]] + 1),
                Steps = new List<List<Vector2>>()
            };

            for (int i = 0; i < stepControls.Count; i++)
            {
                int from = controlIndices[boundaries[i]];
                int to = controlIndices[boundaries[i + 1]];
                result.Steps.Add(samples.GetRange(from, to - from + 1));
            }

            var layout = new KnotLayout(result.StandingPart, result.Steps.ConvertAll(s => (IReadOnlyList<Vector2>)s));
            if (stepLayers == null)
            {
                result.Crossings = FindAlternatingCrossings(layout);
                if (FindSupportCrossings(layout, support).Count > 0)
                {
                    throw new InvalidOperationException("Верёвка пересекает опору: слои над/под нужно задать явно.");
                }
                return result;
            }

            var crossings = FindAlternatingCrossings(layout, requireAlternation: false);
            crossings.AddRange(FindSupportCrossings(layout, support));
            result.Crossings = ApplyLayers(crossings, stepLayers);
            result.Crossings.Sort((a, b) => layout.ToRopeDistance(a.stepIndex, a.distance)
                .CompareTo(layout.ToRopeDistance(b.stepIndex, b.distance)));
            return result;
        }

        /// <summary>
        /// Пересечения верёвки с осью опоры. Слой (isOver) задаётся потом, вместе со слоями шагов.
        /// </summary>
        public static List<KnotCrossing> FindSupportCrossings(KnotLayout layout, KnotSupport support)
        {
            var crossings = new List<KnotCrossing>();
            if (support == null || support.shape == KnotSupportShape.None) return crossings;

            support.GetSegment(out Vector2 start, out Vector2 end);
            foreach (var (ropeDistance, position) in layout.Rope.FindIntersections(start, end))
            {
                int step = layout.FromRopeDistance(ropeDistance, out float distance);
                if (step == KnotCrossing.StandingPart)
                {
                    throw new InvalidOperationException($"Коренная часть пересекает опору в {position}: её нужно отодвинуть.");
                }

                crossings.Add(new KnotCrossing
                {
                    stepIndex = step,
                    distance = distance,
                    otherStepIndex = KnotCrossing.Support,
                    position = position
                });
            }
            return crossings;
        }

        /// <summary>
        /// Переписывает слои пересечений по явному списку: для шага k — по порядку вдоль шага.
        /// </summary>
        public static List<KnotCrossing> ApplyLayers(List<KnotCrossing> crossings, IReadOnlyList<IReadOnlyList<bool>> stepLayers)
        {
            for (int step = 0; step < stepLayers.Count; step++)
            {
                var indices = new List<int>();
                for (int i = 0; i < crossings.Count; i++)
                {
                    if (crossings[i].stepIndex == step) indices.Add(i);
                }
                indices.Sort((a, b) => crossings[a].distance.CompareTo(crossings[b].distance));

                if (indices.Count != stepLayers[step].Count)
                {
                    throw new InvalidOperationException(
                        $"Шаг {step + 1}: в геометрии {indices.Count} пересечений, а слоёв задано {stepLayers[step].Count}.");
                }

                for (int k = 0; k < indices.Count; k++)
                {
                    var crossing = crossings[indices[k]];
                    crossing.isOver = stepLayers[step][k];
                    crossings[indices[k]] = crossing;
                }
            }

            foreach (var crossing in crossings)
            {
                if (crossing.stepIndex >= stepLayers.Count)
                {
                    throw new InvalidOperationException($"Шаг {crossing.stepIndex + 1}: пересечение есть, а слои не заданы.");
                }
            }

            return crossings;
        }

        /// <summary>
        /// Находит перехлёсты и раздаёт слои так, чтобы вдоль верёвки проходы чередовались: над, под, над…
        /// </summary>
        /// <param name="requireAlternation">Бросать исключение, если строгое чередование невозможно.</param>
        public static List<KnotCrossing> FindAlternatingCrossings(KnotLayout layout, bool requireAlternation = true)
        {
            var intersections = layout.Rope.FindSelfIntersections();

            // Все проходы через перехлёсты по порядку вдоль верёвки.
            var passes = new List<(float distance, int crossing, bool isFirst)>();
            for (int i = 0; i < intersections.Count; i++)
            {
                passes.Add((intersections[i].FirstDistance, i, true));
                passes.Add((intersections[i].SecondDistance, i, false));
            }
            passes.Sort((a, b) => a.distance.CompareTo(b.distance));

            var secondPassOver = new bool[intersections.Count];
            var firstPassOver = new bool[intersections.Count];
            for (int p = 0; p < passes.Count; p++)
            {
                bool over = p % 2 == 0;
                if (passes[p].isFirst) firstPassOver[passes[p].crossing] = over;
                else secondPassOver[passes[p].crossing] = over;
            }

            var crossings = new List<KnotCrossing>(intersections.Count);
            for (int i = 0; i < intersections.Count; i++)
            {
                if (requireAlternation && firstPassOver[i] == secondPassOver[i])
                {
                    throw new InvalidOperationException(
                        $"Пересечение в {intersections[i].Position} не удаётся сделать чередующимся над/под.");
                }

                int step = layout.FromRopeDistance(intersections[i].SecondDistance, out float distance);
                int otherStep = layout.FromRopeDistance(intersections[i].FirstDistance, out float otherDistance);
                crossings.Add(new KnotCrossing
                {
                    stepIndex = step,
                    distance = distance,
                    isOver = secondPassOver[i],
                    otherStepIndex = otherStep,
                    otherDistance = otherDistance,
                    position = intersections[i].Position
                });
            }

            crossings.Sort((a, b) => layout.ToRopeDistance(a.stepIndex, a.distance)
                .CompareTo(layout.ToRopeDistance(b.stepIndex, b.distance)));
            return crossings;
        }
    }
}
