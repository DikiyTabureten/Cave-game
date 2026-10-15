using System;
using System.Collections.Generic;
using UnityEngine;

namespace CaveGame
{
    /// <summary>
    /// Один шаг вязки: участок пути, по которому игрок ведёт ходовой конец.
    /// </summary>
    [Serializable]
    public class KnotStep
    {
        [Tooltip("Подсказка для игрока")]
        [TextArea(2, 4)]
        public string hint;

        [Tooltip("Точки пути в локальных координатах узла. Первая совпадает с концом предыдущего шага.")]
        public List<Vector2> points = new List<Vector2>();

        [Tooltip("Насколько далеко (в юнитах) указатель может отойти от пути, не считаясь ошибкой")]
        [Min(0.01f)]
        public float tolerance = 0.35f;
    }

    /// <summary>
    /// Перехлёст: ведущийся конец на шаге <see cref="stepIndex"/> пересекает уже уложенный участок.
    /// </summary>
    [Serializable]
    public struct KnotCrossing
    {
        /// <summary>Значение <see cref="otherStepIndex"/>, когда уложенный участок — коренная часть.</summary>
        public const int StandingPart = -1;

        [Tooltip("Шаг, на котором ведущийся конец проходит пересечение")]
        public int stepIndex;

        [Tooltip("Расстояние вдоль пути этого шага")]
        public float distance;

        [Tooltip("true — ведущийся конец проходит НАД уложенным участком, false — ПОД ним")]
        public bool isOver;

        [Tooltip("Шаг уложенного участка (-1 — коренная часть)")]
        public int otherStepIndex;

        [Tooltip("Расстояние вдоль пути уложенного участка")]
        public float otherDistance;

        public Vector2 position;
    }

    /// <summary>
    /// Описание узла: коренная часть, шаги вязки и порядок слоёв в перехлёстах.
    /// </summary>
    [CreateAssetMenu(menuName = "Cave Game/Knot Definition", fileName = "Knot")]
    public class KnotDefinition : ScriptableObject
    {
        public string displayName;

        [TextArea(2, 5)]
        public string description;

        [Tooltip("Неподвижная коренная часть, локальные координаты. Последняя точка — начало первого шага.")]
        public List<Vector2> standingPart = new List<Vector2>();

        public List<KnotStep> steps = new List<KnotStep>();

        public List<KnotCrossing> crossings = new List<KnotCrossing>();

        [Header("Управление")]
        [Tooltip("Насколько близко к концу верёвки нужно нажать, чтобы его схватить")]
        [Min(0.05f)]
        public float grabRadius = 0.6f;

        [Tooltip("Насколько далеко вперёд по пути конец может продвинуться за одно движение указателя. " +
                 "Защищает от перескока на соседний виток пути.")]
        [Min(0.05f)]
        public float maxAdvance = 0.8f;
    }
}
