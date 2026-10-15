using System;
using System.Collections.Generic;

namespace CaveGame
{
    /// <summary>
    /// Навигация по упорядоченному списку узлов: текущий узел, есть ли следующий, переход.
    /// </summary>
    public sealed class KnotNavigator
    {
        private readonly List<KnotDefinition> _knots;

        public KnotNavigator(IReadOnlyList<KnotDefinition> knots, int startIndex = 0)
        {
            if (knots == null || knots.Count == 0)
            {
                throw new ArgumentException("В каталоге нет ни одного узла.", nameof(knots));
            }

            _knots = new List<KnotDefinition>(knots);
            Select(startIndex);
        }

        public int Count => _knots.Count;
        public int CurrentIndex { get; private set; }
        public KnotDefinition Current => _knots[CurrentIndex];
        public bool HasNext => CurrentIndex < _knots.Count - 1;

        public void Select(int index)
        {
            if (index < 0 || index >= _knots.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"В каталоге {_knots.Count} узлов.");
            }

            CurrentIndex = index;
        }

        /// <summary>Переходит к следующему узлу. На последнем узле ничего не делает и возвращает false.</summary>
        public bool MoveNext()
        {
            if (!HasNext) return false;

            CurrentIndex++;
            return true;
        }
    }
}
