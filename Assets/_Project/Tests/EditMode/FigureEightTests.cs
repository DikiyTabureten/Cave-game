using System.Collections.Generic;
using System.Linq;
using CaveGame.EditorTools;
using NUnit.Framework;
using UnityEngine;

namespace CaveGame.Tests
{
    public class FigureEightTests
    {
        private KnotDefinition _knot;

        [SetUp]
        public void SetUp()
        {
            _knot = FigureEightRecipe.CreateDefinition();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_knot);
        }

        [Test]
        public void HasFourStepsWithRussianHints()
        {
            Assert.AreEqual("Восьмёрка", _knot.displayName);
            Assert.AreEqual(4, _knot.steps.Count);
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
        public void HasFourCrossings()
        {
            Assert.AreEqual(4, _knot.crossings.Count);
        }

        [Test]
        public void CrossingsAlternateOverUnderAlongRope()
        {
            var layout = new KnotLayout(_knot);
            // Каждое пересечение даёт два прохода: ведущийся конец и уже уложенный участок.
            var passes = new List<(float distance, bool over)>();
            foreach (var c in _knot.crossings)
            {
                passes.Add((layout.ToRopeDistance(c.stepIndex, c.distance), c.isOver));
                passes.Add((layout.ToRopeDistance(c.otherStepIndex, c.otherDistance), !c.isOver));
            }

            var ordered = passes.OrderBy(p => p.distance).ToList();
            for (int i = 1; i < ordered.Count; i++)
            {
                Assert.AreNotEqual(ordered[i - 1].over, ordered[i].over, $"проходы {i - 1} и {i} не чередуются");
            }
        }

        [Test]
        public void CrossingsMatchStepDescriptions()
        {
            // Шаг 1 — НАД, шаг 2 — ПОД коренной частью, шаг 3 — сначала НАД, потом ПОД, шаг 4 без пересечений.
            bool[] Layers(int step) => _knot.crossings
                .Where(c => c.stepIndex == step)
                .OrderBy(c => c.distance)
                .Select(c => c.isOver)
                .ToArray();

            CollectionAssert.AreEqual(new[] { true }, Layers(0));
            CollectionAssert.AreEqual(new[] { false }, Layers(1));
            CollectionAssert.AreEqual(new[] { true, false }, Layers(2));
            CollectionAssert.IsEmpty(Layers(3));
        }

        [Test]
        public void SecondStepPassesUnderStandingPart()
        {
            var crossing = _knot.crossings.Single(c => c.stepIndex == 1);
            Assert.AreEqual(KnotCrossing.StandingPart, crossing.otherStepIndex);
        }

        [Test]
        public void WorkingEndLeavesAwayFromStandingPart()
        {
            Vector2 standingDirection = _knot.standingPart[0] - _knot.standingPart.Last();
            var lastStep = _knot.steps.Last().points;
            Vector2 tailDirection = lastStep.Last() - lastStep[0];
            Assert.Less(Vector2.Dot(standingDirection.normalized, tailDirection.normalized), -0.5f);
        }
    }
}
