using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace CaveGame.Tests
{
    public class KnotBuilderTests
    {
        // Коренная часть по y=0, затем шаг 1 — вверх и через неё вниз (одно пересечение),
        // шаг 2 — вправо и ещё раз через коренную (второе пересечение).
        private static readonly Vector2[] Standing = { new Vector2(-4, 0), new Vector2(-2, 0), new Vector2(2, 0) };

        private static readonly IReadOnlyList<Vector2>[] Steps =
        {
            new[] { new Vector2(3, 1), new Vector2(1, 2), new Vector2(0, 1), new Vector2(0, -1) },
            new[] { new Vector2(-1, -2), new Vector2(-3, -1), new Vector2(-3, 1) }
        };

        [Test]
        public void ExplicitLayers_AreAppliedInOrderAlongEachStep()
        {
            var layers = new IReadOnlyList<bool>[] { new[] { false }, new[] { true } };
            var result = KnotBuilder.Build(Standing, Steps, 0.05f, layers);

            Assert.AreEqual(2, result.Crossings.Count);
            Assert.AreEqual(0, result.Crossings[0].stepIndex);
            Assert.IsFalse(result.Crossings[0].isOver);
            Assert.AreEqual(1, result.Crossings[1].stepIndex);
            Assert.IsTrue(result.Crossings[1].isOver);
        }

        [Test]
        public void ExplicitLayers_WrongCount_Throws()
        {
            var layers = new IReadOnlyList<bool>[] { new[] { false, true }, new[] { true } };
            var error = Assert.Throws<InvalidOperationException>(() => KnotBuilder.Build(Standing, Steps, 0.05f, layers));
            StringAssert.Contains("Шаг 1", error.Message);
        }

        [Test]
        public void Support_CrossingsAreFoundAndLayeredWithRopeCrossings()
        {
            // Вертикальная перекладина x = 2.5 пересекает шаг 1 дважды: по пути вправо и обратно.
            // Коренная часть кончается на x = 2 и до неё не достаёт.
            var support = new KnotSupport { shape = KnotSupportShape.Bar, position = new Vector2(2.5f, 1f), length = 6f, angle = 90f };
            var layers = new IReadOnlyList<bool>[] { new[] { false, true, true }, new[] { false } };

            var result = KnotBuilder.Build(Standing, Steps, 0.05f, layers, support);

            var step1 = result.Crossings.FindAll(c => c.stepIndex == 0);
            step1.Sort((a, b) => a.distance.CompareTo(b.distance));
            Assert.AreEqual(3, step1.Count);
            Assert.IsTrue(step1[0].IsSupport, "первым шаг 1 встречает перекладину");
            Assert.IsFalse(step1[0].isOver);
            Assert.IsTrue(step1[1].IsSupport);
            Assert.IsTrue(step1[1].isOver);
            Assert.IsFalse(step1[2].IsSupport, "затем коренную часть");
        }

        [Test]
        public void NoSupport_NoSupportCrossings()
        {
            var result = KnotBuilder.Build(Standing, Steps, 0.05f);
            Assert.IsFalse(result.Crossings.Exists(c => c.IsSupport));
        }

        [Test]
        public void WithoutLayers_UsesAlternation()
        {
            var result = KnotBuilder.Build(Standing, Steps, 0.05f);
            // Проходы вдоль верёвки: коренная (−3), коренная (0), шаг 1 (0), шаг 2 (−3) → над, под, над, под.
            Assert.IsTrue(result.Crossings[0].isOver);
            Assert.IsFalse(result.Crossings[1].isOver);
        }
    }
}
