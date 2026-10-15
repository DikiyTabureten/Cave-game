using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace CaveGame.Tests
{
    public class KnotSessionTests
    {
        private const float Tolerance = 0.35f;
        private const float GrabRadius = 0.5f;
        private const float MaxAdvance = 0.8f;

        // Шаг 1: (0,0) → (3,0). Шаг 2: (3,0) → (3,2).
        private static KnotSession TwoStepSession()
        {
            var steps = new List<KnotStep>
            {
                Step(new Vector2(0, 0), new Vector2(3, 0)),
                Step(new Vector2(3, 0), new Vector2(3, 2))
            };
            return new KnotSession(steps, GrabRadius, MaxAdvance);
        }

        private static KnotStep Step(params Vector2[] corners)
        {
            // Густые точки, как у настоящего пути узла.
            var points = new List<Vector2> { corners[0] };
            for (int i = 1; i < corners.Length; i++)
            {
                int count = Mathf.CeilToInt(Vector2.Distance(corners[i - 1], corners[i]) / 0.05f);
                for (int k = 1; k <= count; k++)
                {
                    points.Add(Vector2.Lerp(corners[i - 1], corners[i], (float)k / count));
                }
            }
            return new KnotStep { hint = "тест", points = points, tolerance = Tolerance };
        }

        /// <summary>Ведёт указатель мелкими шагами, как это делает мышь.</summary>
        private static void DragTo(KnotSession session, Vector2 from, Vector2 to, float stepSize = 0.1f)
        {
            int count = Mathf.Max(1, Mathf.CeilToInt(Vector2.Distance(from, to) / stepSize));
            for (int k = 1; k <= count; k++)
            {
                session.PointerMove(Vector2.Lerp(from, to, (float)k / count));
            }
        }

        [Test]
        public void StartsAtBeginningOfFirstStep()
        {
            var session = TwoStepSession();
            Assert.AreEqual(KnotSessionState.InProgress, session.State);
            Assert.AreEqual(0, session.CurrentStepIndex);
            Assert.AreEqual(0f, session.StepProgress);
            PolylineTests.AssertVector(Vector2.zero, session.EndPosition);
        }

        [Test]
        public void Grab_OnlyNearRopeEnd()
        {
            var session = TwoStepSession();
            Assert.IsFalse(session.PointerDown(new Vector2(2f, 0f)), "середина пути — не конец верёвки");
            Assert.IsFalse(session.IsHolding);

            Assert.IsTrue(session.PointerDown(new Vector2(0.2f, 0.1f)));
            Assert.IsTrue(session.IsHolding);
        }

        [Test]
        public void Move_WithoutGrab_DoesNothing()
        {
            var session = TwoStepSession();
            DragTo(session, Vector2.zero, new Vector2(1.5f, 0f));
            Assert.AreEqual(0f, session.StepProgress);
        }

        [Test]
        public void Move_AlongPath_AdvancesProgress()
        {
            var session = TwoStepSession();
            session.PointerDown(Vector2.zero);
            DragTo(session, Vector2.zero, new Vector2(1.5f, 0.1f));

            Assert.AreEqual(0.5f, session.StepProgress, 0.02f);
            Assert.IsFalse(session.IsDeviating);
            PolylineTests.AssertVector(new Vector2(1.5f, 0f), session.EndPosition, 0.05f);
        }

        [Test]
        public void Move_Backwards_DoesNotReduceProgress()
        {
            var session = TwoStepSession();
            session.PointerDown(Vector2.zero);
            DragTo(session, Vector2.zero, new Vector2(1.5f, 0f));
            DragTo(session, new Vector2(1.5f, 0f), new Vector2(1.0f, 0f));

            Assert.AreEqual(0.5f, session.StepProgress, 0.02f);
            Assert.IsFalse(session.IsDeviating);
        }

        [Test]
        public void Deviation_FlagsErrorAndStopsProgress()
        {
            var session = TwoStepSession();
            session.PointerDown(Vector2.zero);
            DragTo(session, Vector2.zero, new Vector2(1f, 0f));
            float before = session.StepProgress;

            session.PointerMove(new Vector2(1.2f, 1f));

            Assert.IsTrue(session.IsDeviating);
            Assert.AreEqual(before, session.StepProgress);
        }

        [Test]
        public void ReturningToPath_ClearsDeviation()
        {
            var session = TwoStepSession();
            session.PointerDown(Vector2.zero);
            DragTo(session, Vector2.zero, new Vector2(1f, 0f));
            session.PointerMove(new Vector2(1.2f, 1f));

            session.PointerMove(new Vector2(1.1f, 0.1f));

            Assert.IsFalse(session.IsDeviating);
            Assert.Greater(session.StepProgress, 1f / 3f);
        }

        [Test]
        public void JumpFarAheadOnPath_IsNotCounted()
        {
            var session = TwoStepSession();
            session.PointerDown(Vector2.zero);

            // Точка лежит на пути, но далеко впереди конца — перескок запрещён.
            session.PointerMove(new Vector2(2.5f, 0f));

            Assert.IsTrue(session.IsDeviating);
            Assert.AreEqual(0f, session.StepProgress);
        }

        [Test]
        public void NearbyLaterLoopOfPath_DoesNotCaptureProgress()
        {
            // Путь-«шпилька»: туда по y=0 и обратно по y=0.3, обратная ветка в пределах допуска.
            var steps = new List<KnotStep>
            {
                Step(new Vector2(0, 0), new Vector2(3, 0), new Vector2(3, 0.3f), new Vector2(0, 0.3f))
            };
            var session = new KnotSession(steps, GrabRadius, MaxAdvance);
            session.PointerDown(Vector2.zero);
            DragTo(session, Vector2.zero, new Vector2(1f, 0.25f));

            // Засчитана только первая ветка, а не обратная (там было бы ~5.3 из 6.3).
            Assert.AreEqual(1f, session.StepDistance, 0.1f);
        }

        [Test]
        public void ReachingEndOfStep_SwitchesToNextStep()
        {
            var session = TwoStepSession();
            int completedStep = -1;
            session.StepCompleted += index => completedStep = index;

            session.PointerDown(Vector2.zero);
            DragTo(session, Vector2.zero, new Vector2(3f, 0f));

            Assert.AreEqual(0, completedStep);
            Assert.AreEqual(1, session.CurrentStepIndex);
            Assert.AreEqual(0f, session.StepProgress);
            Assert.IsTrue(session.IsHolding, "можно вести дальше, не отпуская");
            PolylineTests.AssertVector(new Vector2(3f, 0f), session.EndPosition, 0.05f);
        }

        [Test]
        public void ReleaseAndGrabAgain_ContinuesFromRopeEnd()
        {
            var session = TwoStepSession();
            session.PointerDown(Vector2.zero);
            DragTo(session, Vector2.zero, new Vector2(1.5f, 0f));
            session.PointerUp();
            Assert.IsFalse(session.IsHolding);

            Assert.IsFalse(session.PointerDown(Vector2.zero), "старое начало — уже не конец верёвки");
            Assert.IsTrue(session.PointerDown(new Vector2(1.5f, 0.1f)));
            DragTo(session, new Vector2(1.5f, 0f), new Vector2(2.5f, 0f));
            Assert.AreEqual(2.5f / 3f, session.StepProgress, 0.02f);
        }

        [Test]
        public void LastStepDone_CompletesKnot()
        {
            var session = TwoStepSession();
            bool completed = false;
            session.Completed += () => completed = true;

            session.PointerDown(Vector2.zero);
            DragTo(session, Vector2.zero, new Vector2(3f, 0f));
            DragTo(session, new Vector2(3f, 0f), new Vector2(3f, 2f));

            Assert.IsTrue(completed);
            Assert.AreEqual(KnotSessionState.Completed, session.State);
            Assert.IsFalse(session.IsHolding);
            Assert.AreEqual(2, session.StepCount);
        }

        [Test]
        public void AfterCompletion_InputIsIgnored()
        {
            var session = TwoStepSession();
            session.PointerDown(Vector2.zero);
            DragTo(session, Vector2.zero, new Vector2(3f, 0f));
            DragTo(session, new Vector2(3f, 0f), new Vector2(3f, 2f));

            Assert.IsFalse(session.PointerDown(new Vector2(3f, 2f)));
            Assert.AreEqual(KnotSessionState.Completed, session.State);
        }

        [Test]
        public void Reset_ReturnsToStart()
        {
            var session = TwoStepSession();
            session.PointerDown(Vector2.zero);
            DragTo(session, Vector2.zero, new Vector2(3f, 0f));
            DragTo(session, new Vector2(3f, 0f), new Vector2(3f, 1f));
            session.PointerMove(new Vector2(5f, 1f));

            session.Reset();

            Assert.AreEqual(KnotSessionState.InProgress, session.State);
            Assert.AreEqual(0, session.CurrentStepIndex);
            Assert.AreEqual(0f, session.StepProgress);
            Assert.IsFalse(session.IsHolding);
            Assert.IsFalse(session.IsDeviating);
            PolylineTests.AssertVector(Vector2.zero, session.EndPosition);
        }

        [Test]
        public void ToleranceScale_WidensAllowedDeviation()
        {
            var steps = new List<KnotStep> { Step(new Vector2(0, 0), new Vector2(3, 0)) };

            // 0.45 от пути: при допуске 0.35 — ошибка, при допуске 0.35 × 1.5 = 0.525 — нет.
            var desktop = new KnotSession(steps, GrabRadius, MaxAdvance);
            desktop.PointerDown(Vector2.zero);
            desktop.PointerMove(new Vector2(0.3f, 0.45f));
            Assert.IsTrue(desktop.IsDeviating);

            var touch = new KnotSession(steps, GrabRadius, MaxAdvance, toleranceScale: 1.5f);
            touch.PointerDown(Vector2.zero);
            touch.PointerMove(new Vector2(0.3f, 0.45f));
            Assert.IsFalse(touch.IsDeviating);
            Assert.AreEqual(Tolerance * 1.5f, touch.CurrentTolerance, 1e-5f);
        }

        [Test]
        public void ToleranceScale_AlsoWidensGrabRadius()
        {
            var steps = new List<KnotStep> { Step(new Vector2(0, 0), new Vector2(3, 0)) };

            // Палец промахивается сильнее курсора: 0.7 от конца не хватает при радиусе 0.5, хватает при 0.75.
            Assert.IsFalse(new KnotSession(steps, GrabRadius, MaxAdvance).PointerDown(new Vector2(0, 0.7f)));
            Assert.IsTrue(new KnotSession(steps, GrabRadius, MaxAdvance, toleranceScale: 1.5f).PointerDown(new Vector2(0, 0.7f)));
        }

        [Test]
        public void FigureEight_CanBeTiedFromStartToFinish()
        {
            var knot = EditorTools.FigureEightRecipe.CreateDefinition();
            try
            {
                var session = new KnotSession(knot);
                Assert.IsTrue(session.PointerDown(knot.steps[0].points[0]));
                foreach (var step in knot.steps)
                {
                    for (int i = 1; i < step.points.Count; i++)
                    {
                        session.PointerMove(step.points[i]);
                        Assert.IsFalse(session.IsDeviating, $"отклонение на точке {i}");
                    }
                }

                Assert.AreEqual(KnotSessionState.Completed, session.State);
            }
            finally
            {
                Object.DestroyImmediate(knot);
            }
        }
    }
}
