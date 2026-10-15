using NUnit.Framework;

namespace CaveGame.Tests
{
    public class CrossingBridgeTests
    {
        private const float MaxHalf = 0.55f;
        private const float RopeWidth = 0.22f;

        [Test]
        public void FarFromOtherCrossings_UsesFullLength()
        {
            float half = CrossingBridge.HalfLength(5f, new[] { 0f, 10f }, MaxHalf, RopeWidth);
            Assert.AreEqual(MaxHalf, half, 1e-5f);
        }

        [Test]
        public void NearAnotherCrossing_StopsBeforeItsStrand()
        {
            // Соседнее пересечение в 0.6 по той же ветке: участок с круглым концом не должен залезть
            // на соседнюю ветку (её край — в 0.6 − 0.11 от центра).
            float half = CrossingBridge.HalfLength(5f, new[] { 5.6f }, MaxHalf, RopeWidth);
            Assert.Less(half + RopeWidth * 0.5f, 0.6f - RopeWidth * 0.5f + 1e-4f);
            Assert.Less(half, MaxHalf);
        }

        [Test]
        public void VeryCloseCrossing_KeepsMinimumToCoverOwnCrossing()
        {
            // Даже вплотную верхний участок закрывает хотя бы саму нижнюю ветку.
            float half = CrossingBridge.HalfLength(5f, new[] { 5.2f }, MaxHalf, RopeWidth);
            Assert.GreaterOrEqual(half, RopeWidth);
        }

        [Test]
        public void IgnoresItsOwnPosition()
        {
            float half = CrossingBridge.HalfLength(5f, new[] { 5f, 20f }, MaxHalf, RopeWidth);
            Assert.AreEqual(MaxHalf, half, 1e-5f);
        }
    }
}
