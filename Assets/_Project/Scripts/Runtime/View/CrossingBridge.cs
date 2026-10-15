using System.Collections.Generic;
using UnityEngine;

namespace CaveGame
{
    /// <summary>
    /// Длина верхнего участка перехлёста: достаточно длинный, чтобы закрыть нижнюю ветку,
    /// но не дотягивается до соседнего пересечения на той же ветке — иначе он перекроет
    /// чужой верхний участок (так бывает в тесных узлах, например в полуштыках).
    /// </summary>
    public static class CrossingBridge
    {
        /// <param name="upper">Где вдоль верёвки лежит центр верхнего участка.</param>
        /// <param name="otherPasses">Все проходы верёвки через пересечения (расстояния вдоль верёвки).</param>
        /// <param name="maxHalf">Обычная половина длины верхнего участка.</param>
        /// <param name="ropeWidth">Толщина верёвки.</param>
        public static float HalfLength(float upper, IEnumerable<float> otherPasses, float maxHalf, float ropeWidth)
        {
            const float samePoint = 1e-3f;

            float nearest = float.MaxValue;
            foreach (float pass in otherPasses)
            {
                float gap = Mathf.Abs(pass - upper);
                if (gap > samePoint && gap < nearest) nearest = gap;
            }

            // Круглый конец участка (радиус — полтолщины) должен остановиться до края соседней ветки.
            float fit = nearest - ropeWidth * 1.5f;
            return Mathf.Clamp(fit, ropeWidth, maxHalf);
        }
    }
}
