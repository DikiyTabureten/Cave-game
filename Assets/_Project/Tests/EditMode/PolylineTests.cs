using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace CaveGame.Tests
{
    public class PolylineTests
    {
        private static Polyline LShape()
        {
            // (0,0) → (2,0) → (2,2): длина 4.
            return new Polyline(new[] { new Vector2(0, 0), new Vector2(2, 0), new Vector2(2, 2) });
        }

        [Test]
        public void Length_IsSumOfSegments()
        {
            Assert.AreEqual(4f, LShape().Length, 1e-5f);
        }

        [Test]
        public void PointAt_InterpolatesAlongPath()
        {
            var line = LShape();
            AssertVector(new Vector2(1, 0), line.PointAt(1f));
            AssertVector(new Vector2(2, 1), line.PointAt(3f));
        }

        [Test]
        public void PointAt_ClampsOutsideRange()
        {
            var line = LShape();
            AssertVector(new Vector2(0, 0), line.PointAt(-5f));
            AssertVector(new Vector2(2, 2), line.PointAt(50f));
        }

        [Test]
        public void ClosestDistance_ProjectsOntoPath()
        {
            var line = LShape();
            float d = line.ClosestDistance(new Vector2(1.5f, 0.3f), 0f, line.Length, out float gap);
            Assert.AreEqual(1.5f, d, 1e-4f);
            Assert.AreEqual(0.3f, gap, 1e-4f);
        }

        [Test]
        public void ClosestDistance_RespectsSearchRange()
        {
            var line = LShape();
            // Ближайшая точка всего пути — (2, 1.8), но ищем только на первом метре.
            float d = line.ClosestDistance(new Vector2(2.1f, 1.8f), 0f, 1f, out float gap);
            Assert.AreEqual(1f, d, 1e-4f);
            Assert.Greater(gap, 1f);
        }

        [Test]
        public void CopyRange_ReturnsSubPathWithInterpolatedEnds()
        {
            var result = new List<Vector2>();
            LShape().CopyRange(1f, 3f, result);
            Assert.AreEqual(3, result.Count);
            AssertVector(new Vector2(1, 0), result[0]);
            AssertVector(new Vector2(2, 0), result[1]);
            AssertVector(new Vector2(2, 1), result[2]);
        }

        [Test]
        public void FindSelfIntersections_FindsCrossOfX()
        {
            var points = new[] { new Vector2(0, 0), new Vector2(2, 2), new Vector2(2, 0), new Vector2(0, 2) };
            var crossings = new Polyline(points).FindSelfIntersections();
            Assert.AreEqual(1, crossings.Count);
            AssertVector(new Vector2(1, 1), crossings[0].Position);
            Assert.Less(crossings[0].FirstDistance, crossings[0].SecondDistance);
        }

        [Test]
        public void FindSelfIntersections_CrossingThroughVertex_CountedOnce()
        {
            // Горизонталь с вершинами через каждые 0.05, вертикаль проходит точно через вершину x = -2.10.
            var points = new List<Vector2>();
            for (int k = 0; k <= 46; k++) points.Add(new Vector2(-3.5f + 0.05f * k, 0f));
            points.Add(new Vector2(-1.0f, 1f));
            points.Add(new Vector2(-2.1f, 1f));
            points.Add(new Vector2(-2.1f, -1f));

            Assert.AreEqual(1, new Polyline(points).FindSelfIntersections().Count);
        }

        [Test]
        public void FindIntersections_ThroughVertex_CountedOnce()
        {
            var points = new List<Vector2>();
            for (int k = 0; k <= 46; k++) points.Add(new Vector2(-3.5f + 0.05f * k, 0f));
            Assert.AreEqual(1, new Polyline(points).FindIntersections(new Vector2(-2.1f, -1f), new Vector2(-2.1f, 1f)).Count);
        }

        [Test]
        public void FindSelfIntersections_IgnoresJointsOfAdjacentSegments()
        {
            Assert.AreEqual(0, LShape().FindSelfIntersections().Count);
        }

        [Test]
        public void FindIntersections_WithSegment_ReturnsDistancesAlongPath()
        {
            // L-образная ломаная пересекает вертикальный отрезок x = 1 один раз, на расстоянии 1.
            var hits = LShape().FindIntersections(new Vector2(1, -1), new Vector2(1, 1));
            Assert.AreEqual(1, hits.Count);
            Assert.AreEqual(1f, hits[0].distance, 1e-4f);
            AssertVector(new Vector2(1, 0), hits[0].position);
        }

        [Test]
        public void FindIntersections_SegmentMissingPath_ReturnsNothing()
        {
            Assert.AreEqual(0, LShape().FindIntersections(new Vector2(5, -1), new Vector2(5, 1)).Count);
        }

        internal static void AssertVector(Vector2 expected, Vector2 actual, float eps = 1e-4f)
        {
            Assert.Less(Vector2.Distance(expected, actual), eps, $"ожидали {expected}, получили {actual}");
        }
    }
}
