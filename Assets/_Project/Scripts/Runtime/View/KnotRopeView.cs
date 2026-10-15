using System.Collections.Generic;
using UnityEngine;

namespace CaveGame
{
    /// <summary>
    /// Рисует уложенную часть верёвки. Порядок слоёв:
    /// обводка → участки верёвки (коренная часть и каждый шаг своим цветом) → опора (если есть) →
    /// в каждом перехлёсте (и там, где верёвка идёт над опорой) тень и верхний участок.
    /// </summary>
    public sealed class KnotRopeView
    {
        public const int SupportOrder = 5;

        private const int OutlineOrder = 0;
        private const int RopeOrder = 1;
        private const int CrossingBaseOrder = 10;

        private readonly KnotLayout _layout;
        private readonly KnotVisualSettings _settings;
        private readonly LineRenderer _outline;
        // [0] — коренная часть, [k + 1] — шаг k.
        private readonly LineRenderer[] _segments;
        private readonly float[] _segmentStarts;
        private readonly float[] _segmentEnds;
        private readonly LineRenderer[] _crossingShadows;
        private readonly LineRenderer[] _crossingTops;
        private readonly float[] _upperDistances;
        private readonly float[] _movingDistances;
        private readonly float[] _halfLengths;
        private readonly List<Vector2> _buffer = new List<Vector2>();
        private Vector3[] _positions = new Vector3[256];

        public KnotRopeView(Transform parent, KnotDefinition knot, KnotLayout layout, Material material, KnotVisualSettings settings)
        {
            _layout = layout;
            _settings = settings;

            var root = new GameObject("Rope").transform;
            root.SetParent(parent, false);

            float width = settings.ropeWidth;
            _outline = LineFactory.Create(root, "Outline", material, width * settings.outlineScale, settings.outlineColor, OutlineOrder);

            int segmentCount = layout.StepCount + 1;
            _segments = new LineRenderer[segmentCount];
            _segmentStarts = new float[segmentCount];
            _segmentEnds = new float[segmentCount];
            _segments[0] = LineFactory.Create(root, "StandingPart", material, width,
                settings.SegmentColor(KnotCrossing.StandingPart), RopeOrder);
            _segmentEnds[0] = layout.StandingPart.Length;
            for (int k = 0; k < layout.StepCount; k++)
            {
                _segments[k + 1] = LineFactory.Create(root, $"Step{k + 1}", material, width, settings.SegmentColor(k), RopeOrder);
                _segmentStarts[k + 1] = layout.StepStart(k);
                _segmentEnds[k + 1] = layout.StepStart(k) + layout.Step(k).Length;
            }

            // Где верёвка проходит ПОД опорой, рисовать поверх нечего: опора и так лежит выше всей верёвки.
            var drawable = knot.crossings.FindAll(c => !(c.IsSupport && !c.isOver));

            // Все проходы верёвки через пересечения: по ним укорачиваем верхние участки в тесных местах.
            var passes = new List<float>();
            foreach (var crossing in knot.crossings)
            {
                passes.Add(layout.ToRopeDistance(crossing.stepIndex, crossing.distance));
                if (!crossing.IsSupport) passes.Add(layout.ToRopeDistance(crossing.otherStepIndex, crossing.otherDistance));
            }

            int count = drawable.Count;
            _crossingShadows = new LineRenderer[count];
            _crossingTops = new LineRenderer[count];
            _upperDistances = new float[count];
            _movingDistances = new float[count];
            _halfLengths = new float[count];
            for (int i = 0; i < count; i++)
            {
                // У каждого перехлёста своя пара слоёв: тень, над ней верхний участок.
                int order = CrossingBaseOrder + i * 2;
                _crossingShadows[i] = LineFactory.Create(root, $"Crossing{i + 1}_Shadow", material,
                    width * settings.shadowScale, settings.shadowColor, order);
                _upperDistances[i] = layout.UpperRopeDistance(drawable[i]);
                // Верхний участок того же цвета, что и шаг, к которому он относится.
                int upperStep = layout.FromRopeDistance(_upperDistances[i], out _);
                _crossingTops[i] = LineFactory.Create(root, $"Crossing{i + 1}_Top", material,
                    width, settings.SegmentColor(upperStep), order + 1);
                _movingDistances[i] = layout.ToRopeDistance(drawable[i].stepIndex, drawable[i].distance);
                _halfLengths[i] = CrossingBridge.HalfLength(_upperDistances[i], passes, settings.crossingHalfLength, width);
            }
        }

        /// <param name="drawnDistance">Сколько верёвки уложено, расстояние вдоль всей верёвки.</param>
        public void Render(float drawnDistance)
        {
            SetRange(_outline, 0f, drawnDistance);
            for (int s = 0; s < _segments.Length; s++)
            {
                SetRange(_segments[s], _segmentStarts[s], Mathf.Min(_segmentEnds[s], drawnDistance));
            }

            float maxShadowHalf = _settings.ropeWidth * _settings.shadowLengthInWidths * 0.5f;
            for (int i = 0; i < _upperDistances.Length; i++)
            {
                float half = _halfLengths[i];
                // Пока ведущийся конец не дошёл до пересечения, перехлёста ещё нет.
                if (drawnDistance < _movingDistances[i] - half)
                {
                    _crossingTops[i].enabled = false;
                    _crossingShadows[i].enabled = false;
                    continue;
                }

                float center = _upperDistances[i];
                SetRange(_crossingTops[i], center - half, Mathf.Min(center + half, drawnDistance));
                // Тень только в зоне самого пересечения, иначе она темнит фон вокруг верхнего участка.
                float shadowHalf = Mathf.Min(maxShadowHalf, half);
                SetRange(_crossingShadows[i], center - shadowHalf, Mathf.Min(center + shadowHalf, drawnDistance));
            }
        }

        private void SetRange(LineRenderer line, float from, float to)
        {
            if (to - from <= 0.01f)
            {
                line.enabled = false;
                return;
            }

            _layout.Rope.CopyRange(from, to, _buffer);
            LineFactory.SetPoints(line, _buffer, ref _positions);
            line.enabled = true;
        }
    }

    /// <summary>
    /// Общие настройки LineRenderer для верёвки, направляющей и прочих линий.
    /// </summary>
    public static class LineFactory
    {
        public static LineRenderer Create(Transform parent, string name, Material material, float width, Color color, int sortingOrder)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var line = go.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.alignment = LineAlignment.TransformZ;
            line.sharedMaterial = material;
            line.widthMultiplier = width;
            line.startColor = line.endColor = color;
            line.numCornerVertices = 4;
            line.numCapVertices = 4;
            line.sortingOrder = sortingOrder;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.positionCount = 0;
            return line;
        }

        public static void SetColor(LineRenderer line, Color color)
        {
            line.startColor = line.endColor = color;
        }

        /// <param name="buffer">Переиспользуемый массив, при нехватке места увеличивается.</param>
        public static void SetPoints(LineRenderer line, List<Vector2> points, ref Vector3[] buffer)
        {
            if (buffer.Length < points.Count)
            {
                buffer = new Vector3[Mathf.NextPowerOfTwo(points.Count)];
            }

            for (int i = 0; i < points.Count; i++)
            {
                buffer[i] = points[i];
            }

            line.positionCount = points.Count;
            line.SetPositions(buffer);
        }
    }
}
