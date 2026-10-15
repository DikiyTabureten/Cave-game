using System.Collections.Generic;
using UnityEngine;

namespace CaveGame
{
    /// <summary>
    /// Рисует уложенную часть верёвки. Порядок слоёв:
    /// обводка → вся верёвка → в каждом перехлёсте тень и верхний участок поверх.
    /// </summary>
    public sealed class KnotRopeView
    {
        private const int OutlineOrder = 0;
        private const int RopeOrder = 1;
        private const int CrossingBaseOrder = 10;

        private readonly KnotLayout _layout;
        private readonly KnotVisualSettings _settings;
        private readonly LineRenderer _outline;
        private readonly LineRenderer _rope;
        private readonly LineRenderer[] _crossingShadows;
        private readonly LineRenderer[] _crossingTops;
        private readonly float[] _upperDistances;
        private readonly float[] _movingDistances;
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
            _rope = LineFactory.Create(root, "Rope", material, width, settings.ropeColor, RopeOrder);

            int count = knot.crossings.Count;
            _crossingShadows = new LineRenderer[count];
            _crossingTops = new LineRenderer[count];
            _upperDistances = new float[count];
            _movingDistances = new float[count];
            for (int i = 0; i < count; i++)
            {
                // У каждого перехлёста своя пара слоёв: тень, над ней верхний участок.
                int order = CrossingBaseOrder + i * 2;
                _crossingShadows[i] = LineFactory.Create(root, $"Crossing{i + 1}_Shadow", material,
                    width * settings.shadowScale, settings.shadowColor, order);
                _crossingTops[i] = LineFactory.Create(root, $"Crossing{i + 1}_Top", material,
                    width, settings.ropeColor, order + 1);
                _upperDistances[i] = layout.UpperRopeDistance(knot.crossings[i]);
                _movingDistances[i] = layout.ToRopeDistance(knot.crossings[i].stepIndex, knot.crossings[i].distance);
            }
        }

        /// <param name="drawnDistance">Сколько верёвки уложено, расстояние вдоль всей верёвки.</param>
        public void Render(float drawnDistance)
        {
            SetRange(_outline, 0f, drawnDistance);
            SetRange(_rope, 0f, drawnDistance);

            float half = _settings.crossingHalfLength;
            for (int i = 0; i < _upperDistances.Length; i++)
            {
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
                float shadowHalf = _settings.ropeWidth * _settings.shadowLengthInWidths * 0.5f;
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
