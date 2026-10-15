using System;
using System.Collections.Generic;
using UnityEngine;

namespace CaveGame
{
    public enum KnotSessionState
    {
        InProgress,
        Completed
    }

    /// <summary>
    /// Состояние вязки одного узла. Чистая логика: получает позиции указателя в координатах узла,
    /// ничего не знает о вводе и отрисовке.
    /// </summary>
    public sealed class KnotSession
    {
        // Насколько близко к концу шага нужно подойти, чтобы шаг засчитался.
        private const float StepEndEpsilon = 0.02f;

        private readonly Polyline[] _paths;
        private readonly float[] _tolerances;
        private readonly float _grabRadius;
        private readonly float _maxAdvance;

        /// <param name="toleranceScale">Множитель допуска и радиуса захвата: на тач-экране палец толще курсора.</param>
        public KnotSession(KnotDefinition knot, float toleranceScale = 1f)
            : this(knot.steps, knot.grabRadius, knot.maxAdvance, toleranceScale)
        {
        }

        /// <param name="grabRadius">Насколько близко к концу верёвки нужно нажать, чтобы его схватить.</param>
        /// <param name="maxAdvance">Окно поиска вдоль пути за одно движение: защищает от перескока.</param>
        /// <param name="toleranceScale">Множитель допуска и радиуса захвата: на тач-экране палец толще курсора.</param>
        public KnotSession(IReadOnlyList<KnotStep> steps, float grabRadius, float maxAdvance, float toleranceScale = 1f)
        {
            if (steps == null || steps.Count == 0)
            {
                throw new ArgumentException("У узла должен быть хотя бы один шаг.", nameof(steps));
            }

            _paths = new Polyline[steps.Count];
            _tolerances = new float[steps.Count];
            for (int i = 0; i < steps.Count; i++)
            {
                _paths[i] = new Polyline(steps[i].points);
                _tolerances[i] = steps[i].tolerance * toleranceScale;
            }

            _grabRadius = grabRadius * toleranceScale;
            _maxAdvance = maxAdvance;
        }

        /// <summary>Шаг пройден до конца. Аргумент — индекс пройденного шага.</summary>
        public event Action<int> StepCompleted;

        /// <summary>Пройден последний шаг.</summary>
        public event Action Completed;

        public KnotSessionState State { get; private set; } = KnotSessionState.InProgress;
        public int StepCount => _paths.Length;
        public int CurrentStepIndex { get; private set; }

        /// <summary>Пройденное расстояние вдоль пути текущего шага.</summary>
        public float StepDistance { get; private set; }

        /// <summary>Прогресс текущего шага от 0 до 1.</summary>
        public float StepProgress => State == KnotSessionState.Completed ? 1f : StepDistance / CurrentPath.Length;

        /// <summary>Игрок держит конец верёвки.</summary>
        public bool IsHolding { get; private set; }

        /// <summary>Указатель ушёл с пути дальше допуска.</summary>
        public bool IsDeviating { get; private set; }

        /// <summary>Текущее положение ходового конца.</summary>
        public Vector2 EndPosition => State == KnotSessionState.Completed
            ? _paths[_paths.Length - 1].End
            : CurrentPath.PointAt(StepDistance);

        public float CurrentTolerance => _tolerances[Mathf.Min(CurrentStepIndex, _paths.Length - 1)];

        private Polyline CurrentPath => _paths[Mathf.Min(CurrentStepIndex, _paths.Length - 1)];

        /// <summary>
        /// Нажатие. Конец верёвки хватается, только если нажали рядом с ним.
        /// </summary>
        public bool PointerDown(Vector2 position)
        {
            if (State == KnotSessionState.Completed) return false;

            IsHolding = Vector2.Distance(position, EndPosition) <= _grabRadius;
            IsDeviating = false;
            return IsHolding;
        }

        public void PointerMove(Vector2 position)
        {
            if (!IsHolding || State == KnotSessionState.Completed) return;

            var path = CurrentPath;
            // Ищем только рядом с концом: так нельзя перескочить на соседний виток или далеко вперёд.
            float closest = path.ClosestDistance(position, StepDistance - _maxAdvance, StepDistance + _maxAdvance, out float gap);

            IsDeviating = gap > CurrentTolerance;
            if (IsDeviating) return;

            if (closest > StepDistance)
            {
                StepDistance = closest;
            }

            if (StepDistance >= path.Length - StepEndEpsilon)
            {
                CompleteStep();
            }
        }

        public void PointerUp()
        {
            IsHolding = false;
            IsDeviating = false;
        }

        public void Reset()
        {
            State = KnotSessionState.InProgress;
            CurrentStepIndex = 0;
            StepDistance = 0f;
            IsHolding = false;
            IsDeviating = false;
        }

        private void CompleteStep()
        {
            int finished = CurrentStepIndex;
            StepDistance = 0f;

            if (finished == _paths.Length - 1)
            {
                State = KnotSessionState.Completed;
                IsHolding = false;
                IsDeviating = false;
                StepCompleted?.Invoke(finished);
                Completed?.Invoke();
                return;
            }

            CurrentStepIndex++;
            StepCompleted?.Invoke(finished);
        }
    }
}
