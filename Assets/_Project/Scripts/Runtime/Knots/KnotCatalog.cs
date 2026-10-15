using System.Collections.Generic;
using UnityEngine;

namespace CaveGame
{
    /// <summary>
    /// Упорядоченный список узлов. Порядок = порядок в меню и для кнопки «Следующий узел».
    /// </summary>
    [CreateAssetMenu(menuName = "Cave Game/Knot Catalog", fileName = "KnotCatalog")]
    public class KnotCatalog : ScriptableObject
    {
        public List<KnotDefinition> knots = new List<KnotDefinition>();

        /// <summary>Узлы каталога без пустых слотов.</summary>
        public List<KnotDefinition> AvailableKnots
        {
            get
            {
                var result = new List<KnotDefinition>(knots.Count);
                foreach (var knot in knots)
                {
                    if (knot != null) result.Add(knot);
                }
                return result;
            }
        }
    }
}
