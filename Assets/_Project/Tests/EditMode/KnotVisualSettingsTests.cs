using NUnit.Framework;
using UnityEngine;

namespace CaveGame.Tests
{
    public class KnotVisualSettingsTests
    {
        [Test]
        public void SegmentColor_StandingPartUsesOwnColor()
        {
            var settings = new KnotVisualSettings();
            Assert.AreEqual(settings.standingColor, settings.SegmentColor(KnotCrossing.StandingPart));
        }

        [Test]
        public void SegmentColor_StepsUsePaletteInOrder()
        {
            var settings = new KnotVisualSettings();
            for (int i = 0; i < settings.stepColors.Length; i++)
            {
                Assert.AreEqual(settings.stepColors[i], settings.SegmentColor(i));
            }
        }

        [Test]
        public void SegmentColor_NeighbouringSegmentsDiffer()
        {
            var settings = new KnotVisualSettings();
            for (int i = KnotCrossing.StandingPart; i < 5; i++)
            {
                Assert.AreNotEqual(settings.SegmentColor(i), settings.SegmentColor(i + 1), $"участки {i} и {i + 1}");
            }
        }

        [Test]
        public void DefaultPalette_GivesEveryBowlineStepItsOwnColor()
        {
            // У булиня 5 шагов: ни один шаг не должен совпасть по цвету с другим.
            var settings = new KnotVisualSettings();
            for (int a = 0; a < 5; a++)
            {
                for (int b = a + 1; b < 5; b++)
                {
                    Assert.AreNotEqual(settings.SegmentColor(a), settings.SegmentColor(b), $"шаги {a + 1} и {b + 1}");
                }
            }
        }

        [Test]
        public void SegmentColor_PaletteRepeatsWhenStepsRunOut()
        {
            var settings = new KnotVisualSettings
            {
                stepColors = new[] { Color.green, Color.blue }
            };
            Assert.AreEqual(Color.green, settings.SegmentColor(2));
            Assert.AreEqual(Color.blue, settings.SegmentColor(3));
        }

        [Test]
        public void SegmentColor_EmptyPaletteFallsBackToRopeColor()
        {
            var settings = new KnotVisualSettings { stepColors = new Color[0] };
            Assert.AreEqual(settings.ropeColor, settings.SegmentColor(1));
        }
    }
}
