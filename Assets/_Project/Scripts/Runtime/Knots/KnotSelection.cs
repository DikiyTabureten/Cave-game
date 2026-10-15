using System.Collections.Generic;
using UnityEngine;

namespace CaveGame
{
    /// <summary>
    /// Выбор узла, который живёт между сценами: меню записывает, сцена вязки читает.
    /// </summary>
    public static class KnotSelection
    {
        /// <summary>Навигатор с выбранным узлом или null, если узел ещё не выбирали (сцену вязки открыли напрямую).</summary>
        public static KnotNavigator Navigator { get; private set; }

        public static void Select(IReadOnlyList<KnotDefinition> knots, int index)
        {
            Navigator = new KnotNavigator(knots, index);
        }

        // Сбрасываем при каждом входе в Play Mode, даже если перезагрузка домена отключена.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reset()
        {
            Navigator = null;
        }
    }
}
