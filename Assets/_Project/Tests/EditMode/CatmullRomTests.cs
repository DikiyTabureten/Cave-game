using NUnit.Framework;
using UnityEngine;

namespace CaveGame.Tests
{
    public class CatmullRomTests
    {
        private static readonly Vector2[] Controls =
        {
            new Vector2(0, 0), new Vector2(1, 1), new Vector2(2, 0), new Vector2(3, 1)
        };

        [Test]
        public void Sample_PassesThroughAllControlPoints()
        {
            var samples = CatmullRom.Sample(Controls, 0.05f, out var controlIndices);
            Assert.AreEqual(Controls.Length, controlIndices.Count);
            for (int i = 0; i < Controls.Length; i++)
            {
                PolylineTests.AssertVector(Controls[i], samples[controlIndices[i]]);
            }
        }

        [Test]
        public void Sample_RespectsMaxSpacing()
        {
            var samples = CatmullRom.Sample(Controls, 0.05f, out _);
            for (int i = 1; i < samples.Count; i++)
            {
                Assert.LessOrEqual(Vector2.Distance(samples[i - 1], samples[i]), 0.05f + 1e-5f);
            }
        }

        [Test]
        public void Sample_CollinearPointsStayStraight()
        {
            var line = new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(3, 0) };
            var samples = CatmullRom.Sample(line, 0.1f, out _);
            foreach (var p in samples)
            {
                Assert.AreEqual(0f, p.y, 1e-4f);
            }
        }
    }
}
