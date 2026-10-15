using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CaveGame.EditorTools
{
    /// <summary>
    /// Создаёт каталог узлов Data/KnotCatalog.asset. Порядок: Восьмёрка, Штык, Булинь —
    /// дальше узлы, добавленные в каталог вручную, в своём порядке.
    /// </summary>
    public static class KnotCatalogBuilder
    {
        public const string AssetPath = "Assets/_Project/Data/KnotCatalog.asset";

        [MenuItem("Tools/Cave Game/Create Knot Catalog")]
        public static KnotCatalog CreateOrUpdate()
        {
            var ordered = new List<KnotDefinition>
            {
                LoadOrCreate(FigureEightRecipe.AssetPath, FigureEightRecipe.CreateOrUpdateAsset),
                LoadOrCreate(TwoHalfHitchesRecipe.AssetPath, TwoHalfHitchesRecipe.CreateOrUpdateAsset),
                LoadOrCreate(BowlineRecipe.AssetPath, BowlineRecipe.CreateOrUpdateAsset)
            };

            var catalog = AssetDatabase.LoadAssetAtPath<KnotCatalog>(AssetPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<KnotCatalog>();
                AssetDatabase.CreateAsset(catalog, AssetPath);
            }

            // Свои узлы — в начале в заданном порядке, добавленные вручную — следом, как были.
            foreach (var knot in catalog.knots)
            {
                if (knot != null && !ordered.Contains(knot)) ordered.Add(knot);
            }
            catalog.knots = ordered;

            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            return catalog;
        }

        private static KnotDefinition LoadOrCreate(string path, System.Func<KnotDefinition> create)
        {
            var knot = AssetDatabase.LoadAssetAtPath<KnotDefinition>(path);
            return knot != null ? knot : create();
        }
    }
}
