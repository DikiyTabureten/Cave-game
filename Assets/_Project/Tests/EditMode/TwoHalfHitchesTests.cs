using System.Collections.Generic;
using System.Linq;
using CaveGame.EditorTools;
using NUnit.Framework;
using UnityEngine;

namespace CaveGame.Tests
{
    public class TwoHalfHitchesTests
    {
        private const int TurnStep = 0;
        private static readonly int[] HitchSteps = { 1, 2, 3 };
        private const int TailStep = 4;

        private KnotDefinition _knot;

        [SetUp]
        public void SetUp()
        {
            _knot = TwoHalfHitchesRecipe.CreateDefinition(TwoHalfHitchesRecipe.DefaultTailLength);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_knot);
        }

        [Test]
        public void HasFiveStepsWithRussianHints()
        {
            Assert.AreEqual("Штык", _knot.displayName);
            Assert.AreEqual(5, _knot.steps.Count);
            foreach (var step in _knot.steps)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(step.hint));
                Assert.Greater(step.points.Count, 10);
            }

            StringAssert.StartsWith("Первый полуштык", _knot.steps[1].hint);
            StringAssert.StartsWith("Второй полуштык", _knot.steps[2].hint);
            StringAssert.StartsWith("Третий полуштык", _knot.steps[3].hint);
            StringAssert.StartsWith("Затяни", _knot.steps[TailStep].hint);
        }

        [Test]
        public void HasBarSupport()
        {
            Assert.IsTrue(_knot.HasSupport);
            Assert.AreEqual(KnotSupportShape.Bar, _knot.support.shape);
            Assert.Greater(_knot.support.length, 0f);
            Assert.Greater(_knot.support.thickness, 0f);
        }

        [Test]
        public void OtherKnotsHaveNoSupport()
        {
            var figureEight = FigureEightRecipe.CreateDefinition();
            var bowline = BowlineRecipe.CreateDefinition();
            try
            {
                Assert.IsFalse(figureEight.HasSupport);
                Assert.IsFalse(bowline.HasSupport);
            }
            finally
            {
                Object.DestroyImmediate(figureEight);
                Object.DestroyImmediate(bowline);
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
        public void TurnAroundBar_GoesUnderThenOver()
        {
            var crossings = StepCrossings(TurnStep);
            Assert.AreEqual(2, crossings.Length);
            Assert.IsTrue(crossings.All(c => c.IsSupport), "оборот пересекает только опору");
            Assert.IsFalse(crossings[0].isOver);
            Assert.IsTrue(crossings[1].isOver);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void HalfHitch_OverStanding_UnderStanding_OverOwnStrand(int step)
        {
            var crossings = StepCrossings(step);
            Assert.AreEqual(3, crossings.Length);
            CollectionAssert.AreEqual(new[] { true, false, true }, crossings.Select(c => c.isOver).ToArray());

            Assert.AreEqual(KnotCrossing.StandingPart, crossings[0].otherStepIndex);
            Assert.AreEqual(KnotCrossing.StandingPart, crossings[1].otherStepIndex);
            Assert.AreEqual(step, crossings[2].otherStepIndex, "конец продевается в петлю из своей же ветки");
        }

        [Test]
        public void AllHalfHitchesTurnTheSameWay()
        {
            // Над коренной конец поднимается с одной стороны, под ней опускается с другой —
            // у всех полуштыков «над» лежит с той же стороны от «под».
            var sides = HitchSteps.Select(step =>
            {
                var c = StepCrossings(step);
                return Mathf.Sign(c[1].position.x - c[0].position.x);
            }).Distinct().ToArray();

            Assert.AreEqual(1, sides.Length);
        }

        [Test]
        public void HalfHitchesAreEvenlySpacedAwayFromSupport()
        {
            // Каждый следующий полуштык дальше от опоры, и расстояние между соседними одинаковое.
            float[] x = HitchSteps.Select(step => StepCrossings(step)[0].position.x).ToArray();
            float support = _knot.support.position.x;
            for (int i = 1; i < x.Length; i++)
            {
                Assert.Greater(Mathf.Abs(x[i] - support), Mathf.Abs(x[i - 1] - support));
            }
            Assert.AreEqual(x[0] - x[1], x[1] - x[2], 0.05f);
        }

        [Test]
        public void HalfHitchesHaveTheSameShape()
        {
            // Одинаковая форма: длина пути и ширина обноса совпадают у всех трёх.
            var lengths = HitchSteps.Select(step => new Polyline(_knot.steps[step].points).Length).ToArray();
            var widths = HitchSteps.Select(step =>
            {
                var c = StepCrossings(step);
                return Mathf.Abs(c[1].position.x - c[0].position.x);
            }).ToArray();

            // Первый полуштык начинается от оборота, остальные — от предыдущего: заход у них разный,
            // поэтому сравниваем второй и третий по длине, а ширину обноса — у всех.
            Assert.AreEqual(lengths[1], lengths[2], 0.05f);
            Assert.AreEqual(widths[0], widths[1], 0.02f);
            Assert.AreEqual(widths[1], widths[2], 0.02f);
        }

        [Test]
        public void Tail_IsAtLeastTwiceTheOldOneAndNotShorterThanHalfHitch()
        {
            float tail = new Polyline(_knot.steps[TailStep].points).Length;
            const float oldTail = 0.8f;
            var hitch = StepCrossings(HitchSteps[0]);
            float hitchWidth = Mathf.Abs(hitch[1].position.x - hitch[0].position.x);

            Assert.GreaterOrEqual(tail, oldTail * 2f);
            Assert.GreaterOrEqual(tail, hitchWidth);
        }

        [Test]
        public void TailLength_IsControlledByParameter()
        {
            var longer = TwoHalfHitchesRecipe.CreateDefinition(TwoHalfHitchesRecipe.DefaultTailLength + 1f);
            try
            {
                float baseTail = new Polyline(_knot.steps[TailStep].points).Length;
                float longTail = new Polyline(longer.steps[TailStep].points).Length;
                Assert.AreEqual(baseTail + 1f, longTail, 0.05f);
            }
            finally
            {
                Object.DestroyImmediate(longer);
            }
        }

        [Test]
        public void Tail_LiesAlongStandingPartAwayFromSupport()
        {
            var tail = _knot.steps[TailStep].points;
            Vector2 direction = tail.Last() - tail[0];
            Vector2 standingAway = _knot.standingPart[0] - _knot.standingPart.Last();
            Assert.Greater(Vector2.Dot(direction.normalized, standingAway.normalized), 0.9f, "хвост уходит от опоры вдоль коренной");
            Assert.IsEmpty(StepCrossings(TailStep), "хвост ничего не пересекает");
        }

        [Test]
        public void Tail_DoesNotOverlapHalfHitches()
        {
            // Кроме самого начала (оно общее с третьим полуштыком), хвост держится в стороне от полуштыков.
            var tail = new Polyline(_knot.steps[TailStep].points);
            var hitchPoints = HitchSteps.SelectMany(step => _knot.steps[step].points).ToList();
            for (float d = 0.6f; d <= tail.Length; d += 0.1f)
            {
                Vector2 p = tail.PointAt(d);
                float nearest = hitchPoints.Min(h => Vector2.Distance(h, p));
                Assert.Greater(nearest, 0.3f, $"хвост в {p} слишком близко к полуштыку");
            }
        }

        [Test]
        public void TwoHalfHitches_CanBeTiedFromStartToFinish()
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

        private KnotCrossing[] StepCrossings(int step)
        {
            return _knot.crossings.Where(c => c.stepIndex == step).OrderBy(c => c.distance).ToArray();
        }
    }
}
