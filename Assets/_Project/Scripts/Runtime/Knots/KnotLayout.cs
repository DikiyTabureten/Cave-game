using System.Collections.Generic;
using UnityEngine;

namespace CaveGame
{
    /// <summary>
    /// Вся верёвка одной ломаной: коренная часть, затем шаги подряд.
    /// Переводит «шаг + расстояние внутри шага» в расстояние вдоль всей верёвки и обратно.
    /// </summary>
    public sealed class KnotLayout
    {
        private readonly float[] _stepStarts;
        private readonly Polyline[] _steps;

        public KnotLayout(KnotDefinition knot) : this(knot.standingPart, knot.steps)
        {
        }

        public KnotLayout(IReadOnlyList<Vector2> standingPart, IReadOnlyList<KnotStep> steps)
            : this(standingPart, ToPointLists(steps))
        {
        }

        public KnotLayout(IReadOnlyList<Vector2> standingPart, IReadOnlyList<IReadOnlyList<Vector2>> steps)
        {
            var rope = new List<Vector2>(standingPart);
            _stepStarts = new float[steps.Count];
            _steps = new Polyline[steps.Count];

            StandingPart = new Polyline(standingPart);
            float offset = StandingPart.Length;
            for (int i = 0; i < steps.Count; i++)
            {
                _steps[i] = new Polyline(steps[i]);
                _stepStarts[i] = offset;
                offset += _steps[i].Length;

                // Первая точка шага совпадает с последней точкой предыдущего участка.
                for (int k = 1; k < steps[i].Count; k++)
                {
                    rope.Add(steps[i][k]);
                }
            }

            Rope = new Polyline(rope);
        }

        public Polyline Rope { get; }
        public Polyline StandingPart { get; }
        public int StepCount => _steps.Length;

        public Polyline Step(int index) => _steps[index];

        public float StepStart(int index) => _stepStarts[index];

        /// <param name="stepIndex">Индекс шага или <see cref="KnotCrossing.StandingPart"/>.</param>
        public float ToRopeDistance(int stepIndex, float distance)
        {
            return stepIndex == KnotCrossing.StandingPart ? distance : _stepStarts[stepIndex] + distance;
        }

        /// <summary>
        /// Обратное преобразование. Для коренной части возвращает <see cref="KnotCrossing.StandingPart"/>.
        /// </summary>
        public int FromRopeDistance(float ropeDistance, out float localDistance)
        {
            for (int i = _stepStarts.Length - 1; i >= 0; i--)
            {
                if (ropeDistance >= _stepStarts[i])
                {
                    localDistance = ropeDistance - _stepStarts[i];
                    return i;
                }
            }

            localDistance = ropeDistance;
            return KnotCrossing.StandingPart;
        }

        private static IReadOnlyList<IReadOnlyList<Vector2>> ToPointLists(IReadOnlyList<KnotStep> steps)
        {
            var result = new List<IReadOnlyList<Vector2>>(steps.Count);
            foreach (var step in steps)
            {
                result.Add(step.points);
            }
            return result;
        }
    }
}
