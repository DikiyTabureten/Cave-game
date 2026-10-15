using System.Linq;
using CaveGame.EditorTools;
using NUnit.Framework;
using UnityEngine;

namespace CaveGame.Tests
{
    public class TwoHalfHitchesTests
    {
        private KnotDefinition _knot;

        [SetUp]
        public void SetUp()
        {
            _knot = TwoHalfHitchesRecipe.CreateDefinition();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_knot);
        }

        [Test]
        public void HasFourStepsWithRussianHints()
        {
            Assert.AreEqual("Штык", _knot.displayName);
            Assert.AreEqual(4, _knot.steps.Count);
            foreach (var step in _knot.steps)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(step.hint));
                Assert.Greater(step.points.Count, 10);
            }
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
            var crossings = StepCrossings(0);
            Assert.AreEqual(2, crossings.Length);
            Assert.IsTrue(crossings.All(c => c.IsSupport), "оборот пересекает только опору");
            Assert.IsFalse(crossings[0].isOver);
            Assert.IsTrue(crossings[1].isOver);
        }

        [TestCase(1)]
        [TestCase(2)]
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
        public void BothHalfHitchesTurnTheSameWay()
        {
            // Оборот вокруг коренной: конец поднимается НАД ней с одной стороны и опускается ПОД ней с другой.
            // В ту же сторону — значит, у обоих полуштыков «над» лежит с одной и той же стороны от «под».
            float Side(int step)
            {
                var c = StepCrossings(step);
                return Mathf.Sign(c[1].position.x - c[0].position.x);
            }

            Assert.AreEqual(Side(1), Side(2));
        }

        [Test]
        public void SecondHalfHitchIsFurtherFromSupport()
        {
            float first = StepCrossings(1)[0].position.x;
            float second = StepCrossings(2)[0].position.x;
            Assert.Greater(Mathf.Abs(second - _knot.support.position.x), Mathf.Abs(first - _knot.support.position.x));
        }

        [Test]
        public void LastStep_HasNoCrossings()
        {
            Assert.IsEmpty(StepCrossings(3));
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
