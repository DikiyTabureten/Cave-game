using System;
using UnityEngine;

namespace CaveGame
{
    /// <summary>
    /// Внешний вид прототипа: толщины, цвета, размеры. Настраивается в инспекторе KnotPrototypeController.
    /// </summary>
    [Serializable]
    public class KnotVisualSettings
    {
        [Header("Верёвка")]
        [Min(0.01f)] public float ropeWidth = 0.22f;
        public Color ropeColor = new Color(1f, 0.72f, 0.25f);
        [Tooltip("Тёмная обводка вокруг всей верёвки")]
        public Color outlineColor = new Color(0.12f, 0.08f, 0.04f);
        [Min(1f)] public float outlineScale = 1.35f;

        [Header("Перехлёсты")]
        [Tooltip("Тень под верхним участком в месте пересечения. Цвет близок к фону: нижняя ветка выглядит прерванной, как на схеме узла.")]
        public Color shadowColor = new Color(0.035f, 0.04f, 0.05f, 1f);
        [Tooltip("Ширина тени относительно толщины верёвки")]
        [Min(1f)] public float shadowScale = 2.1f;
        [Tooltip("Длина тени вдоль верхнего участка, в толщинах верёвки")]
        [Min(0.5f)] public float shadowLengthInWidths = 3f;
        [Tooltip("Половина длины верхнего участка, который рисуется поверх нижнего")]
        [Min(0.05f)] public float crossingHalfLength = 0.55f;

        [Header("Направляющая")]
        public Color guideColor = new Color(1f, 1f, 1f, 0.4f);
        [Min(0.01f)] public float guideWidth = 0.09f;
        [Min(0.05f)] public float dashLength = 0.25f;

        [Header("Хват")]
        [Min(0.05f)] public float gripDiameter = 0.5f;
        public Color gripColor = new Color(0.3f, 0.85f, 1f);

        [Header("Ошибка и фон")]
        public Color errorColor = new Color(1f, 0.3f, 0.3f);
        public Color backgroundColor = new Color(0.07f, 0.08f, 0.1f);
    }
}
