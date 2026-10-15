using System;
using System.Collections.Generic;
using UnityEngine;

namespace CaveGame
{
    /// <summary>
    /// Строит путь узла по контрольным точкам: один сплайн Catmull-Rom на всю верёвку,
    /// разрезанный на коренную часть и шаги, плюс перехлёсты с чередованием над/под.
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
        public static Result Build(
            IReadOnlyList<Vector2> standingControls,
            IReadOnlyList<IReadOnlyList<Vector2>> stepControls,
            float spacing)
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
            result.Crossings = FindAlternatingCrossings(layout);
            return result;
        }

        /// <summary>
        /// Находит перехлёсты и раздаёт слои так, чтобы вдоль верёвки проходы чередовались: над, под, над…
        /// </summary>
        public static List<KnotCrossing> FindAlternatingCrossings(KnotLayout layout)
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
                if (firstPassOver[i] == secondPassOver[i])
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
