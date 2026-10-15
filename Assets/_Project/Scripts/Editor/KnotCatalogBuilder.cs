using UnityEditor;
using UnityEngine;

namespace CaveGame.EditorTools
{
    /// <summary>
    /// Создаёт каталог узлов Data/KnotCatalog.asset. Порядок в каталоге: Восьмёрка, Штык, Булинь —
    /// пока существует только Восьмёрка; новые узлы добавляются в список каталога в инспекторе.
    /// </summary>
    public static class KnotCatalogBuilder
    {
        public const string AssetPath = "Assets/_Project/Data/KnotCatalog.asset";

        [MenuItem("Tools/Cave Game/Create Knot Catalog")]
        public static KnotCatalog CreateOrUpdate()
        {
            var figureEight = AssetDatabase.LoadAssetAtPath<KnotDefinition>(FigureEightRecipe.AssetPath);
            if (figureEight == null) figureEight = FigureEightRecipe.CreateOrUpdateAsset();

            var catalog = AssetDatabase.LoadAssetAtPath<KnotCatalog>(AssetPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<KnotCatalog>();
                AssetDatabase.CreateAsset(catalog, AssetPath);
            }

            // Существующий порядок не трогаем, только гарантируем, что Восьмёрка стоит первой.
            catalog.knots.Remove(figureEight);
            catalog.knots.Insert(0, figureEight);

            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            return catalog;
        }
    }
}
