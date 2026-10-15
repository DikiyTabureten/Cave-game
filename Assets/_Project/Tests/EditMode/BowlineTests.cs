using System.Collections.Generic;
using System.Linq;
using CaveGame.EditorTools;
using NUnit.Framework;
using UnityEngine;

namespace CaveGame.Tests
{
    public class BowlineTests
    {
        private KnotDefinition _knot;

        [SetUp]
        public void SetUp()
        {
            _knot = BowlineRecipe.CreateDefinition();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_knot);
        }

        [Test]
        public void HasFiveStepsWithRussianHints()
        {
            Assert.AreEqual("Булинь", _knot.displayName);
            Assert.AreEqual(5, _knot.steps.Count);
            foreach (var step in _knot.steps)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(step.hint));
                Assert.Greater(step.points.Count, 10);
                Assert.Greater(step.tolerance, 0f);
            }
        }

        [Test]
        public void StepsAreContinuous()
        {
            PolylineTests.AssertVector(_knot.standingPart.Last(), _knot.steps[0].points[0]);
            for (int i = 1; i < _knot.steps.Count; i++)
            {
                PolylineTests.AssertVector(_knot.steps[i - 1].points.Last(), _knot.steps[i].points[0]);
            }
        }

        [Test]
        public void CrossingsMatchStepDescriptions()
        {
            // 1. Петелька: конец НАД коренной.
            // 2. В петельку снизу: ПОД краем, наружу НАД.
            // 3. Вокруг коренной сзади: ПОД.
            // 4. Обратно в петельку сверху: НАД, НАД, выход снизу ПОД.
            // 5. Затянуть — без пересечений.
            CollectionAssert.AreEqual(new[] { true }, Layers(0));
            CollectionAssert.AreEqual(new[] { false, true }, Layers(1));
            CollectionAssert.AreEqual(new[] { false }, Layers(2));
            CollectionAssert.AreEqual(new[] { true, true, false }, Layers(3));
            CollectionAssert.IsEmpty(Layers(4));
        }

        [Test]
        public void ThirdStepGoesBehindStandingPart()
        {
            var crossing = _knot.crossings.Single(c => c.stepIndex == 2);
            Assert.AreEqual(KnotCrossing.StandingPart, crossing.otherStepIndex);
            Assert.IsFalse(crossing.isOver);
        }

        [Test]
        public void SecondAndFourthStepsPassTheSmallLoopRim()
        {
            // И выход конца из петельки, и возвращение пересекают край малой петельки (шаг 1).
            Assert.IsTrue(_knot.crossings.Where(c => c.stepIndex == 1).All(c => c.otherStepIndex == 0));
            Assert.IsTrue(_knot.crossings.Where(c => c.stepIndex == 3).All(c => c.otherStepIndex == 0));
        }

        [Test]
        public void TailEndsInsideBigLoop()
        {
            // Большая петля — путь шага 2 (вниз, вокруг и обратно к петельке), замкнутый отрезком.
            var bigLoop = _knot.steps[1].points;
            Vector2 tail = _knot.steps.Last().points.Last();
            Assert.IsTrue(IsInside(tail, bigLoop), $"конец {tail} вне большой петли");
        }

        [Test]
        public void Bowline_CanBeTiedFromStartToFinish()
        {
            var session = new KnotSession(_knot);
            Assert.IsTrue(session.PointerDown(_knot.steps[0].points[0]));
            for (int s = 0; s < _knot.steps.Count; s++)
            {
                var points = _knot.steps[s].points;
                for (int i = 1; i < points.Count; i++)
                {
                    session.PointerMove(points[i]);
                    Assert.IsFalse(session.IsDeviating, $"отклонение: шаг {s + 1}, точка {i}");
                }
            }

            Assert.AreEqual(KnotSessionState.Completed, session.State);
        }

        private bool[] Layers(int step)
        {
            return _knot.crossings
                .Where(c => c.stepIndex == step)
                .OrderBy(c => c.distance)
                .Select(c => c.isOver)
                .ToArray();
        }

        private static bool IsInside(Vector2 point, IReadOnlyList<Vector2> polygon)
        {
            bool inside = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                Vector2 a = polygon[i];
                Vector2 b = polygon[j];
                if ((a.y > point.y) != (b.y > point.y) &&
                    point.x < (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x)
                {
                    inside = !inside;
                }
            }
            return inside;
        }
    }
}
