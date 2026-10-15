using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace CaveGame.Tests
{
    public class PointerTrackerTests
    {
        private PointerTracker _tracker;

        [SetUp]
        public void SetUp()
        {
            _tracker = new PointerTracker();
        }

        private PointerSample Frame(params PointerContact[] contacts)
        {
            return _tracker.Update(new List<PointerContact>(contacts));
        }

        private static PointerContact Finger(int id, float x, float y)
        {
            return new PointerContact(id, new Vector2(x, y));
        }

        [Test]
        public void NoContacts_IsNone()
        {
            Assert.AreEqual(PointerPhase.None, Frame().Phase);
        }

        [Test]
        public void SingleTouch_PressMoveRelease()
        {
            var down = Frame(Finger(1, 10, 10));
            Assert.AreEqual(PointerPhase.Pressed, down.Phase);
            Assert.AreEqual(new Vector2(10, 10), down.ScreenPosition);

            var move = Frame(Finger(1, 30, 12));
            Assert.AreEqual(PointerPhase.Moved, move.Phase);
            Assert.AreEqual(new Vector2(30, 12), move.ScreenPosition);

            var up = Frame();
            Assert.AreEqual(PointerPhase.Released, up.Phase);
            Assert.AreEqual(new Vector2(30, 12), up.ScreenPosition, "отпускание — там, где палец был последним");

            Assert.AreEqual(PointerPhase.None, Frame().Phase);
        }

        [Test]
        public void SecondFinger_IsIgnoredWhileFirstIsDown()
        {
            Frame(Finger(1, 10, 10));
            var both = Frame(Finger(1, 20, 10), Finger(2, 500, 500));
            Assert.AreEqual(PointerPhase.Moved, both.Phase);
            Assert.AreEqual(new Vector2(20, 10), both.ScreenPosition, "ведёт по-прежнему первый палец");

            var secondLifted = Frame(Finger(1, 25, 10));
            Assert.AreEqual(PointerPhase.Moved, secondLifted.Phase, "второй палец ушёл — вязка не прерывается");
        }

        [Test]
        public void FirstFingerLifted_SecondFingerDoesNotTakeOver()
        {
            Frame(Finger(1, 10, 10));
            Frame(Finger(1, 20, 10), Finger(2, 500, 500));

            var firstUp = Frame(Finger(2, 510, 500));
            Assert.AreEqual(PointerPhase.Released, firstUp.Phase);

            // Второй палец лежал раньше — это не новое нажатие, он не должен внезапно схватить верёвку.
            Assert.AreEqual(PointerPhase.None, Frame(Finger(2, 520, 500)).Phase);
        }

        [Test]
        public void NewTouchAfterAllReleased_StartsTracking()
        {
            Frame(Finger(1, 10, 10));
            Frame();
            var again = Frame(Finger(3, 40, 40));
            Assert.AreEqual(PointerPhase.Pressed, again.Phase);
            Assert.AreEqual(new Vector2(40, 40), again.ScreenPosition);
        }

        [Test]
        public void TwoFingersSameFrame_TracksTheFirstInList()
        {
            var down = Frame(Finger(4, 1, 1), Finger(5, 2, 2));
            Assert.AreEqual(PointerPhase.Pressed, down.Phase);
            Assert.AreEqual(new Vector2(1, 1), down.ScreenPosition);
        }

        [Test]
        public void Reset_DropsTracking()
        {
            Frame(Finger(1, 10, 10));
            _tracker.Reset();
            // После сброса удерживаемый палец не считается новым нажатием.
            Assert.AreEqual(PointerPhase.None, Frame(Finger(1, 12, 10)).Phase);
        }
    }
}
