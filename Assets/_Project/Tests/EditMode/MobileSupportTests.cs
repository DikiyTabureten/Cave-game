using NUnit.Framework;
using UnityEngine;

namespace CaveGame.Tests
{
    public class MobileSupportTests
    {
        [TestCase(PlatformMode.Auto, true, true)]
        [TestCase(PlatformMode.Auto, false, false)]
        [TestCase(PlatformMode.Mobile, false, true)]
        [TestCase(PlatformMode.Desktop, true, false)]
        public void PlatformProfile_ResolvesMobile(PlatformMode mode, bool isMobilePlatform, bool expected)
        {
            Assert.AreEqual(expected, PlatformProfile.IsMobile(mode, isMobilePlatform));
        }

        [Test]
        public void SafeArea_FullScreen_StretchesToEdges()
        {
            SafeAreaMath.ToAnchors(new Rect(0, 0, 1920, 1080), new Vector2(1920, 1080), out var min, out var max);
            Assert.AreEqual(Vector2.zero, min);
            Assert.AreEqual(Vector2.one, max);
        }

        [Test]
        public void SafeArea_Notch_ShrinksAnchors()
        {
            // Вырез слева 100 px и закругление снизу 40 px.
            SafeAreaMath.ToAnchors(new Rect(100, 40, 1920, 1040), new Vector2(2020, 1080), out var min, out var max);
            Assert.AreEqual(100f / 2020f, min.x, 1e-5f);
            Assert.AreEqual(40f / 1080f, min.y, 1e-5f);
            Assert.AreEqual(1f, max.x, 1e-5f);
            Assert.AreEqual(1f, max.y, 1e-5f);
        }

        [Test]
        public void SafeArea_ZeroScreen_DoesNotBreak()
        {
            SafeAreaMath.ToAnchors(new Rect(0, 0, 0, 0), Vector2.zero, out var min, out var max);
            Assert.AreEqual(Vector2.zero, min);
            Assert.AreEqual(Vector2.one, max);
        }
    }
}
