using System.Collections.Generic;
using UnityEngine;

namespace CaveGame
{
    /// <summary>Одно активное касание (или нажатая кнопка мыши) в текущем кадре.</summary>
    public readonly struct PointerContact
    {
        public readonly int Id;
        public readonly Vector2 ScreenPosition;

        public PointerContact(int id, Vector2 screenPosition)
        {
            Id = id;
            ScreenPosition = screenPosition;
        }
    }

    /// <summary>
    /// Превращает набор касаний кадра в один указатель: ведёт палец, который коснулся первым,
    /// остальные игнорирует. Второй палец не прерывает вязку и не перехватывает верёвку,
    /// даже когда первый отпущен — новое ведение начинается только со свежего касания.
    /// </summary>
    public sealed class PointerTracker
    {
        private readonly HashSet<int> _previouslyDown = new HashSet<int>();
        private readonly HashSet<int> _currentlyDown = new HashSet<int>();
        private int? _trackedId;
        private Vector2 _lastPosition;

        /// <param name="contacts">Все касания, активные в этом кадре.</param>
        public PointerSample Update(IReadOnlyList<PointerContact> contacts)
        {
            _currentlyDown.Clear();
            foreach (var contact in contacts) _currentlyDown.Add(contact.Id);

            var sample = Evaluate(contacts);

            _previouslyDown.Clear();
            _previouslyDown.UnionWith(_currentlyDown);
            return sample;
        }

        /// <summary>
        /// Забыть отслеживаемое касание. Пальцы, лежащие на экране, после сброса не считаются новыми нажатиями.
        /// </summary>
        public void Reset()
        {
            _trackedId = null;
        }

        private PointerSample Evaluate(IReadOnlyList<PointerContact> contacts)
        {
            if (_trackedId.HasValue)
            {
                foreach (var contact in contacts)
                {
                    if (contact.Id != _trackedId.Value) continue;
                    _lastPosition = contact.ScreenPosition;
                    return new PointerSample(PointerPhase.Moved, _lastPosition);
                }

                _trackedId = null;
                return new PointerSample(PointerPhase.Released, _lastPosition);
            }

            foreach (var contact in contacts)
            {
                if (_previouslyDown.Contains(contact.Id)) continue;
                _trackedId = contact.Id;
                _lastPosition = contact.ScreenPosition;
                return new PointerSample(PointerPhase.Pressed, _lastPosition);
            }

            return new PointerSample(PointerPhase.None, _lastPosition);
        }
    }
}
