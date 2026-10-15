using System.Collections.Generic;
using UnityEngine;

namespace CaveGame
{
    /// <summary>
    /// Центростремительный сплайн Catmull-Rom: плавная кривая через все контрольные точки без петель и выбросов.
    /// </summary>
    public static class CatmullRom
    {
        private const float Alpha = 0.5f;
        private const int LengthEstimateSamples = 16;
        private const int MaxSubdivisionDepth = 8;

        /// <summary>
        /// Строит ломаную по сплайну. Расстояние между соседними точками не больше <paramref name="maxSpacing"/>.
        /// <paramref name="controlIndices"/> — индекс каждой контрольной точки в результате.
        /// </summary>
        public static List<Vector2> Sample(IReadOnlyList<Vector2> controls, float maxSpacing, out List<int> controlIndices)
        {
            var samples = new List<Vector2> { controls[0] };
            controlIndices = new List<int> { 0 };

            for (int i = 0; i < controls.Count - 1; i++)
            {
                Vector2 p0 = i > 0 ? controls[i - 1] : 2f * controls[0] - controls[1];
                Vector2 p1 = controls[i];
                Vector2 p2 = controls[i + 1];
                Vector2 p3 = i + 2 < controls.Count ? controls[i + 2] : 2f * p2 - p1;

                int count = Mathf.Max(1, Mathf.CeilToInt(EstimateLength(p0, p1, p2, p3) / maxSpacing));
                float previousT = 0f;
                Vector2 previous = p1;
                for (int k = 1; k <= count; k++)
                {
                    float t = (float)k / count;
                    Vector2 current = k == count ? p2 : Evaluate(p0, p1, p2, p3, t);
                    // Скорость по параметру неравномерна: где точки разошлись, досэмплируем делением пополам.
                    Subdivide(p0, p1, p2, p3, previousT, previous, t, current, maxSpacing, samples, MaxSubdivisionDepth);
                    samples.Add(current);
                    previousT = t;
                    previous = current;
                }

                controlIndices.Add(samples.Count - 1);
            }

            return samples;
        }

        public static Vector2 Evaluate(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float t0 = 0f;
            float t1 = NextKnot(t0, p0, p1);
            float t2 = NextKnot(t1, p1, p2);
            float t3 = NextKnot(t2, p2, p3);
            float tt = Mathf.Lerp(t1, t2, t);

            Vector2 a1 = Blend(p0, p1, t0, t1, tt);
            Vector2 a2 = Blend(p1, p2, t1, t2, tt);
            Vector2 a3 = Blend(p2, p3, t2, t3, tt);
            Vector2 b1 = Blend(a1, a2, t0, t2, tt);
            Vector2 b2 = Blend(a2, a3, t1, t3, tt);
            return Blend(b1, b2, t1, t2, tt);
        }

        private static void Subdivide(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3,
            float ta, Vector2 a, float tb, Vector2 b, float maxSpacing, List<Vector2> samples, int depth)
        {
            if (depth == 0 || Vector2.Distance(a, b) <= maxSpacing) return;

            float tm = 0.5f * (ta + tb);
            Vector2 m = Evaluate(p0, p1, p2, p3, tm);
            Subdivide(p0, p1, p2, p3, ta, a, tm, m, maxSpacing, samples, depth - 1);
            samples.Add(m);
            Subdivide(p0, p1, p2, p3, tm, m, tb, b, maxSpacing, samples, depth - 1);
        }

        private static float NextKnot(float previous, Vector2 a, Vector2 b)
        {
            return previous + Mathf.Pow(Mathf.Max(Vector2.Distance(a, b), 1e-4f), Alpha);
        }

        private static Vector2 Blend(Vector2 a, Vector2 b, float ta, float tb, float t)
        {
            return ((tb - t) * a + (t - ta) * b) / (tb - ta);
        }

        private static float EstimateLength(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
        {
            float length = 0f;
            Vector2 previous = p1;
            for (int k = 1; k <= LengthEstimateSamples; k++)
            {
                Vector2 current = Evaluate(p0, p1, p2, p3, (float)k / LengthEstimateSamples);
                length += Vector2.Distance(previous, current);
                previous = current;
            }
            return length;
        }
    }
}
