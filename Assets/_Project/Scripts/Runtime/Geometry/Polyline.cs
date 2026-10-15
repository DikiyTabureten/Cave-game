using System;
using System.Collections.Generic;
using UnityEngine;

namespace CaveGame
{
    /// <summary>
    /// Пересечение ломаной с самой собой. Расстояния — вдоль ломаной, FirstDistance &lt; SecondDistance.
    /// </summary>
    public readonly struct PolylineIntersection
    {
        public readonly float FirstDistance;
        public readonly float SecondDistance;
        public readonly Vector2 Position;

        public PolylineIntersection(float firstDistance, float secondDistance, Vector2 position)
        {
            FirstDistance = firstDistance;
            SecondDistance = secondDistance;
            Position = position;
        }
    }

    /// <summary>
    /// Неизменяемая ломаная с параметризацией по длине дуги.
    /// </summary>
    public sealed class Polyline
    {
        private readonly Vector2[] _points;
        private readonly float[] _cumulative;

        public Polyline(IReadOnlyList<Vector2> points)
        {
            if (points == null || points.Count < 2)
            {
                throw new ArgumentException("Ломаной нужно минимум две точки.", nameof(points));
            }

            _points = new Vector2[points.Count];
            _cumulative = new float[points.Count];
            for (int i = 0; i < points.Count; i++)
            {
                _points[i] = points[i];
                _cumulative[i] = i == 0 ? 0f : _cumulative[i - 1] + Vector2.Distance(points[i - 1], points[i]);
            }
        }

        public int Count => _points.Length;
        public Vector2 this[int index] => _points[index];
        public float Length => _cumulative[_cumulative.Length - 1];
        public Vector2 Start => _points[0];
        public Vector2 End => _points[_points.Length - 1];

        public float DistanceAt(int index) => _cumulative[index];

        public Vector2 PointAt(float distance)
        {
            if (distance <= 0f) return _points[0];
            if (distance >= Length) return End;

            int i = SegmentAt(distance);
            float segmentLength = _cumulative[i + 1] - _cumulative[i];
            float t = segmentLength > 0f ? (distance - _cumulative[i]) / segmentLength : 0f;
            return Vector2.LerpUnclamped(_points[i], _points[i + 1], t);
        }

        /// <summary>
        /// Ближайшая к <paramref name="point"/> точка ломаной на участке [from, to].
        /// Возвращает расстояние вдоль ломаной, <paramref name="gap"/> — расстояние от точки до пути.
        /// </summary>
        public float ClosestDistance(Vector2 point, float from, float to, out float gap)
        {
            from = Mathf.Clamp(from, 0f, Length);
            to = Mathf.Clamp(to, from, Length);

            float bestDistance = from;
            float bestSqr = (PointAt(from) - point).sqrMagnitude;

            int first = SegmentAt(from);
            int last = SegmentAt(to);
            for (int i = first; i <= last; i++)
            {
                float segStart = Mathf.Max(from, _cumulative[i]);
                float segEnd = Mathf.Min(to, _cumulative[i + 1]);
                float segmentLength = _cumulative[i + 1] - _cumulative[i];

                float candidate = segStart;
                if (segmentLength > 0f)
                {
                    Vector2 a = _points[i];
                    Vector2 ab = _points[i + 1] - a;
                    float t = Vector2.Dot(point - a, ab) / (segmentLength * segmentLength);
                    candidate = Mathf.Clamp(_cumulative[i] + t * segmentLength, segStart, segEnd);
                }

                float sqr = (PointAt(candidate) - point).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    bestDistance = candidate;
                }
            }

            gap = Mathf.Sqrt(bestSqr);
            return bestDistance;
        }

        /// <summary>
        /// Копирует участок ломаной [from, to] в <paramref name="result"/> (список очищается).
        /// </summary>
        public void CopyRange(float from, float to, List<Vector2> result)
        {
            result.Clear();
            from = Mathf.Clamp(from, 0f, Length);
            to = Mathf.Clamp(to, from, Length);

            result.Add(PointAt(from));
            for (int i = 0; i < _points.Length; i++)
            {
                if (_cumulative[i] > from && _cumulative[i] < to)
                {
                    result.Add(_points[i]);
                }
            }
            result.Add(PointAt(to));
        }

        /// <summary>
        /// Все самопересечения ломаной (соседние отрезки, касающиеся в общей вершине, не считаются).
        /// </summary>
        public List<PolylineIntersection> FindSelfIntersections()
        {
            var result = new List<PolylineIntersection>();
            for (int i = 0; i < _points.Length - 1; i++)
            {
                for (int j = i + 2; j < _points.Length - 1; j++)
                {
                    if (TryIntersect(_points[i], _points[i + 1], _points[j], _points[j + 1], out float t, out float u))
                    {
                        float first = Mathf.Lerp(_cumulative[i], _cumulative[i + 1], t);
                        float second = Mathf.Lerp(_cumulative[j], _cumulative[j + 1], u);
                        result.Add(new PolylineIntersection(first, second, Vector2.Lerp(_points[i], _points[i + 1], t)));
                    }
                }
            }
            return result;
        }

        private int SegmentAt(float distance)
        {
            int lo = 0;
            int hi = _cumulative.Length - 2;
            while (lo < hi)
            {
                int mid = (lo + hi + 1) / 2;
                if (_cumulative[mid] <= distance) lo = mid;
                else hi = mid - 1;
            }
            return lo;
        }

        private static bool TryIntersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d, out float t, out float u)
        {
            Vector2 r = b - a;
            Vector2 s = d - c;
            float denominator = r.x * s.y - r.y * s.x;
            t = u = 0f;
            if (Mathf.Abs(denominator) < 1e-9f) return false;

            Vector2 ac = c - a;
            t = (ac.x * s.y - ac.y * s.x) / denominator;
            u = (ac.x * r.y - ac.y * r.x) / denominator;
            // Полуинтервалы [0, 1), чтобы пересечение в общей вершине не считалось дважды.
            return t >= 0f && t < 1f && u >= 0f && u < 1f;
        }
    }
}
